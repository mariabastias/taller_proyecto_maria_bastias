using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.ProponerTrueque.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ProponerTrueque;

public class ProponerTruequeRepository : IProponerTruequeRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public ProponerTruequeRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    // RF-09: Crear propuesta con detalles (tabla DetallePropuesta)
    public async Task<int> CrearPropuesta(PropuestaTrueque propuesta, int prendaOfrecidaId, int prendaSolicitadaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insertar PropuestaTrueque
            const string sqlPropuesta = @"
                INSERT INTO PropuestaTrueque (
                    usuario_proponente_id, mensaje_acompanante, estado_propuesta_id,
                    prioridad, es_contraoferta, fecha_propuesta, fecha_expiracion
                )
                VALUES (
                    @UsuarioProponenteId, @MensajeAcompanante, @EstadoPropuestaId,
                    @Prioridad, @EsContraoferta, @FechaPropuesta, @FechaExpiracion
                );
                SELECT SCOPE_IDENTITY();";

            int propuestaId;
            using (var command = new SqlCommand(sqlPropuesta, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UsuarioProponenteId", SqlDbType.Int) { Value = propuesta.UsuarioProponenteId });
                command.Parameters.Add(new SqlParameter("@MensajeAcompanante", SqlDbType.NVarChar, 500) { Value = (object?)propuesta.MensajeAcompanante ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@EstadoPropuestaId", SqlDbType.Int) { Value = 1 }); // Pendiente
                command.Parameters.Add(new SqlParameter("@Prioridad", SqlDbType.Int) { Value = propuesta.Prioridad });
                command.Parameters.Add(new SqlParameter("@EsContraoferta", SqlDbType.Bit) { Value = propuesta.EsContraoferta });
                command.Parameters.Add(new SqlParameter("@FechaPropuesta", SqlDbType.DateTime) { Value = DateTime.Now });
                command.Parameters.Add(new SqlParameter("@FechaExpiracion", SqlDbType.DateTime) { Value = DateTime.Now.AddDays(7) }); // RF-09: 7 dias

                var result = await command.ExecuteScalarAsync();
                propuestaId = Convert.ToInt32(result);
            }

            // Insertar DetallePropuesta para prenda ofrecida
            const string sqlDetalle = @"
                INSERT INTO DetallePropuesta (propuesta_id, prenda_id, tipo_participacion, fecha_agregado)
                VALUES (@PropuestaId, @PrendaId, @TipoParticipacion, @FechaAgregado)";

            using (var command = new SqlCommand(sqlDetalle, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
                command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaOfrecidaId });
                command.Parameters.Add(new SqlParameter("@TipoParticipacion", SqlDbType.NVarChar, 20) { Value = "ofrecida" });
                command.Parameters.Add(new SqlParameter("@FechaAgregado", SqlDbType.DateTime) { Value = DateTime.Now });
                await command.ExecuteNonQueryAsync();
            }

            // Insertar DetallePropuesta para prenda solicitada
            using (var command = new SqlCommand(sqlDetalle, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
                command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaSolicitadaId });
                command.Parameters.Add(new SqlParameter("@TipoParticipacion", SqlDbType.NVarChar, 20) { Value = "solicitada" });
                command.Parameters.Add(new SqlParameter("@FechaAgregado", SqlDbType.DateTime) { Value = DateTime.Now });
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

    public async Task<List<Prenda>> ObtenerPrendasDisponiblesUsuario(int usuarioId)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria, ep.nombre_estado,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.usuario_id = @UsuarioId
              AND p.estado_publicacion_id = 1
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapearPrenda(reader));
        }

        return prendas;
    }

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria, ep.nombre_estado,
                   u.nombre, u.apellido, u.url_foto_perfil,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var prenda = MapearPrenda(reader);
            prenda.UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id"));
            prenda.Usuario = new Usuario
            {
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.IsDBNull(reader.GetOrdinal("apellido"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("apellido")),
                UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("url_foto_perfil"))
            };
            return prenda;
        }

        return null;
    }

    // RF-09: Verificar limite de 3 propuestas activas por prenda
    public async Task<int> ContarPropuestasActivasPorPrenda(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Contar propuestas activas (pendiente=1, aceptada=2) donde la prenda participa
        const string sql = @"
            SELECT COUNT(DISTINCT pt.propuesta_id)
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp ON pt.propuesta_id = dp.propuesta_id
            WHERE dp.prenda_id = @PrendaId
              AND pt.estado_propuesta_id IN (1, 2)
              AND (pt.fecha_expiracion IS NULL OR pt.fecha_expiracion > GETDATE())";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> ExistePropuestaActiva(int prendaOfrecidaId, int prendaSolicitadaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Verificar si existe propuesta activa con estas prendas
        const string sql = @"
            SELECT COUNT(*)
            FROM PropuestaTrueque pt
            WHERE pt.estado_propuesta_id IN (1, 2)
              AND (pt.fecha_expiracion IS NULL OR pt.fecha_expiracion > GETDATE())
              AND EXISTS (
                  SELECT 1 FROM DetallePropuesta dp1
                  WHERE dp1.propuesta_id = pt.propuesta_id
                    AND dp1.prenda_id = @PrendaOfrecidaId
                    AND dp1.tipo_participacion = 'ofrecida'
              )
              AND EXISTS (
                  SELECT 1 FROM DetallePropuesta dp2
                  WHERE dp2.propuesta_id = pt.propuesta_id
                    AND dp2.prenda_id = @PrendaSolicitadaId
                    AND dp2.tipo_participacion = 'solicitada'
              )";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaOfrecidaId", SqlDbType.Int) { Value = prendaOfrecidaId });
        command.Parameters.Add(new SqlParameter("@PrendaSolicitadaId", SqlDbType.Int) { Value = prendaSolicitadaId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static Prenda MapearPrenda(SqlDataReader reader)
    {
        var prenda = new Prenda
        {
            PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_id")),
            TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_publicacion")),
            DescripcionPublicacion = reader.IsDBNull(reader.GetOrdinal("descripcion_publicacion"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
            CategoriaId = reader.GetInt32(reader.GetOrdinal("categoria_id")),
            Talla = reader.GetString(reader.GetOrdinal("talla")),
            EstadoPrendaId = reader.GetInt32(reader.GetOrdinal("estado_prenda_id")),
            EstadoPublicacionId = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")),
            FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
            FechaActualizacion = reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion"))
        };

        // Computed properties
        prenda.Tipo = reader.IsDBNull(reader.GetOrdinal("nombre_categoria"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_categoria"));

        prenda.Estado = reader.IsDBNull(reader.GetOrdinal("nombre_estado"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_estado"));

        prenda.Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_principal"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("imagen_principal"));

        return prenda;
    }
}
