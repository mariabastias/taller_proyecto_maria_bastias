using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Repositories;

public class TruequeSharedRepository : ITruequeSharedRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public TruequeSharedRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasAsync()
    {
        var propuestas = new List<PropuestaTrueque>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.prioridad, pt.es_contraoferta,
                   pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion
            FROM PropuestaTrueque pt
            ORDER BY pt.fecha_propuesta DESC";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            propuestas.Add(MapPropuestaTruequeFromReader(reader));
        }

        return propuestas;
    }

    public async Task<PropuestaTrueque?> GetPropuestaByIdAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.prioridad, pt.es_contraoferta,
                   pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion
            FROM PropuestaTrueque pt
            WHERE pt.propuesta_id = @PropuestaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = id });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPropuestaTruequeFromReader(reader);
        }

        return null;
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasByUsuarioProponenteIdAsync(int usuarioId)
    {
        var propuestas = new List<PropuestaTrueque>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.prioridad, pt.es_contraoferta,
                   pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion
            FROM PropuestaTrueque pt
            WHERE pt.usuario_proponente_id = @UsuarioId
            ORDER BY pt.fecha_propuesta DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            propuestas.Add(MapPropuestaTruequeFromReader(reader));
        }

        return propuestas;
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasByUsuarioReceptorIdAsync(int usuarioId)
    {
        var propuestas = new List<PropuestaTrueque>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT DISTINCT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.prioridad, pt.es_contraoferta,
                   pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp ON pt.propuesta_id = dp.propuesta_id
            INNER JOIN Prenda p ON dp.prenda_id = p.prenda_id
            WHERE p.usuario_id = @UsuarioId
              AND dp.tipo_participacion = 'solicitada'
            ORDER BY pt.fecha_propuesta DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            propuestas.Add(MapPropuestaTruequeFromReader(reader));
        }

        return propuestas;
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasByPrendaIdAsync(int prendaId)
    {
        var propuestas = new List<PropuestaTrueque>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT DISTINCT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.prioridad, pt.es_contraoferta,
                   pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp ON pt.propuesta_id = dp.propuesta_id
            WHERE dp.prenda_id = @PrendaId
            ORDER BY pt.fecha_propuesta DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            propuestas.Add(MapPropuestaTruequeFromReader(reader));
        }

        return propuestas;
    }

    public async Task<int> CreatePropuestaAsync(PropuestaTrueque propuesta)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlPropuesta = @"
                INSERT INTO PropuestaTrueque (usuario_proponente_id, mensaje_acompanante,
                                              estado_propuesta_id, prioridad, es_contraoferta,
                                              fecha_propuesta, fecha_expiracion)
                VALUES (@UsuarioProponenteId, @MensajeAcompanante,
                        @EstadoPropuestaId, @Prioridad, @EsContraoferta,
                        GETDATE(), @FechaExpiracion);
                SELECT SCOPE_IDENTITY();";

            int propuestaId;
            using (var command = new SqlCommand(sqlPropuesta, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UsuarioProponenteId", SqlDbType.Int) { Value = propuesta.UsuarioProponenteId });
                command.Parameters.Add(new SqlParameter("@MensajeAcompanante", SqlDbType.VarChar, 500) { Value = (object?)propuesta.MensajeAcompanante ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@EstadoPropuestaId", SqlDbType.Int) { Value = 1 }); // Pendiente
                command.Parameters.Add(new SqlParameter("@Prioridad", SqlDbType.Int) { Value = propuesta.Prioridad });
                command.Parameters.Add(new SqlParameter("@EsContraoferta", SqlDbType.Bit) { Value = propuesta.EsContraoferta });
                command.Parameters.Add(new SqlParameter("@FechaExpiracion", SqlDbType.DateTime) { Value = (object?)propuesta.FechaExpiracion ?? DBNull.Value });

                var result = await command.ExecuteScalarAsync();
                propuestaId = Convert.ToInt32(result);
            }

            // Insertar detalles de propuesta (prenda ofrecida y solicitada)
            const string sqlDetalle = @"
                INSERT INTO DetallePropuesta (propuesta_id, prenda_id, tipo_participacion, fecha_agregado)
                VALUES (@PropuestaId, @PrendaId, @TipoParticipacion, GETDATE())";

            // Prenda ofrecida
            using (var command = new SqlCommand(sqlDetalle, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
                command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = propuesta.PrendaOfrecidaId });
                command.Parameters.Add(new SqlParameter("@TipoParticipacion", SqlDbType.VarChar, 20) { Value = "ofrecida" });
                await command.ExecuteNonQueryAsync();
            }

            // Prenda solicitada
            using (var command = new SqlCommand(sqlDetalle, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
                command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = propuesta.PrendaSolicitadaId });
                command.Parameters.Add(new SqlParameter("@TipoParticipacion", SqlDbType.VarChar, 20) { Value = "solicitada" });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return propuestaId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> AceptarPropuestaAsync(int id, string? mensaje = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 2,
                fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId
              AND estado_propuesta_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> RechazarPropuestaAsync(int id, string motivo)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 3,
                fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId
              AND estado_propuesta_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CompletarPropuestaAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 6,
                fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId
              AND estado_propuesta_id = 2";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CancelarPropuestaAsync(int id, string motivo)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 7,
                fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId
              AND estado_propuesta_id IN (1, 2)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private PropuestaTrueque MapPropuestaTruequeFromReader(SqlDataReader reader)
    {
        return new PropuestaTrueque
        {
            PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
            UsuarioProponenteId = reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
            MensajeAcompanante = reader.IsDBNull(reader.GetOrdinal("mensaje_acompanante"))
                ? null
                : reader.GetString(reader.GetOrdinal("mensaje_acompanante")),
            EstadoPropuestaId = reader.GetInt32(reader.GetOrdinal("estado_propuesta_id")),
            Prioridad = reader.GetInt32(reader.GetOrdinal("prioridad")),
            EsContraoferta = reader.GetBoolean(reader.GetOrdinal("es_contraoferta")),
            FechaPropuesta = reader.GetDateTime(reader.GetOrdinal("fecha_propuesta")),
            FechaRespuesta = reader.IsDBNull(reader.GetOrdinal("fecha_respuesta"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_respuesta")),
            FechaExpiracion = reader.IsDBNull(reader.GetOrdinal("fecha_expiracion"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_expiracion"))
        };
    }
}
