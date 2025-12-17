using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Evaluacion.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.Evaluacion;

public class EvaluacionRepository : IEvaluacionRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public EvaluacionRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    // RF-13: Crear evaluacion con dimensiones
    public async Task<int> CrearEvaluacion(EvaluacionModel evaluacion, List<EvaluacionDimensionDetalle> dimensiones)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insertar evaluacion principal
            const string sqlEvaluacion = @"
                INSERT INTO Evaluacion (propuesta_id, usuario_evaluador_id, usuario_evaluado_id,
                                        calificacion_general, comentario, fecha_evaluacion)
                VALUES (@PropuestaId, @UsuarioEvaluadorId, @UsuarioEvaluadoId,
                        @CalificacionGeneral, @Comentario, @FechaEvaluacion);
                SELECT SCOPE_IDENTITY();";

            int evaluacionId;
            using (var command = new SqlCommand(sqlEvaluacion, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = evaluacion.PropuestaId });
                command.Parameters.Add(new SqlParameter("@UsuarioEvaluadorId", SqlDbType.Int) { Value = evaluacion.UsuarioEvaluadorId });
                command.Parameters.Add(new SqlParameter("@UsuarioEvaluadoId", SqlDbType.Int) { Value = evaluacion.UsuarioEvaluadoId });
                command.Parameters.Add(new SqlParameter("@CalificacionGeneral", SqlDbType.Int) { Value = evaluacion.CalificacionGeneral });
                command.Parameters.Add(new SqlParameter("@Comentario", SqlDbType.VarChar, 250) { Value = (object?)evaluacion.Comentario ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@FechaEvaluacion", SqlDbType.DateTime) { Value = DateTime.Now });

                var result = await command.ExecuteScalarAsync();
                evaluacionId = Convert.ToInt32(result);
            }

            // RF-13: Insertar calificaciones por dimension SOLO si existen en DimensionEvaluacion
            // Primero verificar que los dimension_id existan en la BD para evitar FK violation
            if (dimensiones.Count > 0)
            {
                // Obtener dimension_id validos de la BD
                const string sqlValidar = "SELECT dimension_id FROM DimensionEvaluacion";
                var dimensionesValidas = new HashSet<int>();

                using (var cmdValidar = new SqlCommand(sqlValidar, connection, transaction))
                using (var reader = await cmdValidar.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dimensionesValidas.Add(reader.GetInt32(0));
                    }
                }

                // Solo insertar dimensiones que existan en la BD
                const string sqlDimension = @"
                    INSERT INTO EvaluacionDimension (evaluacion_id, dimension_id, calificacion)
                    VALUES (@EvaluacionId, @DimensionId, @Calificacion)";

                foreach (var dimension in dimensiones)
                {
                    // Verificar que dimension_id existe en DimensionEvaluacion
                    if (dimensionesValidas.Contains(dimension.DimensionId))
                    {
                        using var command = new SqlCommand(sqlDimension, connection, transaction);
                        command.Parameters.Add(new SqlParameter("@EvaluacionId", SqlDbType.Int) { Value = evaluacionId });
                        command.Parameters.Add(new SqlParameter("@DimensionId", SqlDbType.Int) { Value = dimension.DimensionId });
                        command.Parameters.Add(new SqlParameter("@Calificacion", SqlDbType.Int) { Value = dimension.Calificacion });
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

            await transaction.CommitAsync();
            return evaluacionId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ExisteEvaluacion(int propuestaId, int usuarioEvaluadorId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM Evaluacion
            WHERE propuesta_id = @PropuestaId AND usuario_evaluador_id = @UsuarioEvaluadorId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioEvaluadorId", SqlDbType.Int) { Value = usuarioEvaluadorId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Esquema 3FN
        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.estado_propuesta_id,
                   dp_sol.prenda_id AS prenda_solicitada_id,
                   p_sol.usuario_id AS usuario_receptor_id
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            WHERE pt.propuesta_id = @PropuestaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PropuestaTrueque
            {
                PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                UsuarioProponenteId = reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
                UsuarioReceptorId = reader.GetInt32(reader.GetOrdinal("usuario_receptor_id")),
                EstadoPropuestaId = reader.GetInt32(reader.GetOrdinal("estado_propuesta_id"))
            };
        }

        return null;
    }

    // RF-14: Calculo ponderado de reputacion
    public async Task<bool> ActualizarReputacionUsuario(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // RF-14: Calcular promedio ponderado por dimensiones
            // Formula: SUM(calificacion * peso) / SUM(peso) para cada evaluacion, luego promedio general
            // Nota: tabla Usuario no tiene total_evaluaciones, solo reputacion_promedio DECIMAL(3,2)
            const string sqlPonderado = @"
                UPDATE Usuario
                SET reputacion_promedio = (
                    SELECT ISNULL(CAST(AVG(promedio_ponderado) AS DECIMAL(3,2)), 0)
                    FROM (
                        SELECT e.evaluacion_id,
                               CASE
                                   WHEN SUM(d.peso) > 0 THEN
                                       CAST(SUM(ed.calificacion * d.peso) AS DECIMAL(5,2)) / CAST(SUM(d.peso) AS DECIMAL(5,2))
                                   ELSE
                                       CAST(e.calificacion_general AS DECIMAL(5,2))
                               END AS promedio_ponderado
                        FROM Evaluacion e
                        LEFT JOIN EvaluacionDimension ed ON e.evaluacion_id = ed.evaluacion_id
                        LEFT JOIN DimensionEvaluacion d ON ed.dimension_id = d.dimension_id
                        WHERE e.usuario_evaluado_id = @UsuarioId
                        GROUP BY e.evaluacion_id, e.calificacion_general
                    ) AS promedios
                )
                WHERE usuario_id = @UsuarioId";

            using (var command = new SqlCommand(sqlPonderado, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            // Nota: tabla HistorialReputacion no existe en el esquema actual
            // Si se necesita historial, se debe crear la tabla primero

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<DimensionEvaluacion>> ObtenerDimensiones()
    {
        var dimensiones = new List<DimensionEvaluacion>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT dimension_id, nombre_dimension, descripcion, peso FROM DimensionEvaluacion ORDER BY dimension_id";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            dimensiones.Add(new DimensionEvaluacion
            {
                DimensionId = reader.GetInt32(0),
                NombreDimension = reader.GetString(1),
                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                Peso = reader.IsDBNull(3) ? 1.0m : reader.GetDecimal(3)
            });
        }

        // Si DimensionEvaluacion esta vacia, retornar lista vacia
        // NO inventar IDs que no existen en la BD - causaria FK violation
        return dimensiones;
    }

    public async Task<List<EvaluacionModel>> ObtenerEvaluacionesUsuario(int usuarioId)
    {
        var evaluaciones = new List<EvaluacionModel>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT e.evaluacion_id, e.propuesta_id, e.usuario_evaluador_id, e.usuario_evaluado_id,
                   e.calificacion_general, e.comentario, e.fecha_evaluacion,
                   u.nombre, u.apellido, u.url_foto_perfil
            FROM Evaluacion e
            INNER JOIN Usuario u ON e.usuario_evaluador_id = u.usuario_id
            WHERE e.usuario_evaluado_id = @UsuarioId
            ORDER BY e.fecha_evaluacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            evaluaciones.Add(new EvaluacionModel
            {
                EvaluacionId = reader.GetInt32(0),
                PropuestaId = reader.GetInt32(1),
                UsuarioEvaluadorId = reader.GetInt32(2),
                UsuarioEvaluadoId = reader.GetInt32(3),
                CalificacionGeneral = reader.GetInt32(4),
                Comentario = reader.IsDBNull(5) ? null : reader.GetString(5),
                FechaEvaluacion = reader.GetDateTime(6),
                UsuarioEvaluador = new Usuario
                {
                    Nombre = reader.GetString(7),
                    Apellido = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UrlFotoPerfil = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
                }
            });
        }

        return evaluaciones;
    }

    public async Task<EvaluacionModel?> ObtenerEvaluacion(int propuestaId, int usuarioEvaluadorId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT e.evaluacion_id, e.propuesta_id, e.usuario_evaluador_id, e.usuario_evaluado_id,
                   e.calificacion_general, e.comentario, e.fecha_evaluacion
            FROM Evaluacion e
            WHERE e.propuesta_id = @PropuestaId AND e.usuario_evaluador_id = @UsuarioEvaluadorId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioEvaluadorId", SqlDbType.Int) { Value = usuarioEvaluadorId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new EvaluacionModel
            {
                EvaluacionId = reader.GetInt32(0),
                PropuestaId = reader.GetInt32(1),
                UsuarioEvaluadorId = reader.GetInt32(2),
                UsuarioEvaluadoId = reader.GetInt32(3),
                CalificacionGeneral = reader.GetInt32(4),
                Comentario = reader.IsDBNull(5) ? null : reader.GetString(5),
                FechaEvaluacion = reader.GetDateTime(6)
            };
        }

        return null;
    }

    public async Task<(decimal Promedio, int Total)> ObtenerEstadisticasUsuario(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Usuario no tiene total_evaluaciones, se calcula desde tabla Evaluacion
        const string sql = @"
            SELECT ISNULL(u.reputacion_promedio, 0),
                   (SELECT COUNT(*) FROM Evaluacion WHERE usuario_evaluado_id = @UsuarioId)
            FROM Usuario u WHERE u.usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader.GetDecimal(0), reader.GetInt32(1));
        }

        return (0, 0);
    }

    public async Task<Usuario?> ObtenerUsuarioAEvaluar(int propuestaId, int usuarioEvaluadorId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Obtener la contraparte del trueque (usuario a evaluar)
        // Usuario no tiene total_evaluaciones, se calcula desde Evaluacion
        const string sql = @"
            SELECT u.usuario_id, u.nombre, u.apellido, u.url_foto_perfil,
                   ISNULL(u.reputacion_promedio, 0) AS reputacion_promedio,
                   (SELECT COUNT(*) FROM Evaluacion e WHERE e.usuario_evaluado_id = u.usuario_id) AS total_evaluaciones
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            INNER JOIN Usuario u ON u.usuario_id = CASE
                WHEN pt.usuario_proponente_id = @UsuarioEvaluadorId THEN p_sol.usuario_id
                ELSE pt.usuario_proponente_id
            END
            WHERE pt.propuesta_id = @PropuestaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioEvaluadorId", SqlDbType.Int) { Value = usuarioEvaluadorId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                UsuarioId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellido = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                UrlFotoPerfil = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ReputacionPromedio = reader.GetDecimal(4),
                Estadisticas = new Estadisticas
                {
                    Valoraciones = reader.GetInt32(5)
                }
            };
        }

        return null;
    }
}
