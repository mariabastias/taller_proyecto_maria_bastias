using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.PropuestasRecibidas.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PropuestasRecibidas;

public class PropuestasRecibidasRepository : IPropuestasRecibidasRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public PropuestasRecibidasRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<PropuestaTrueque>> ObtenerPropuestasRecibidas(int usuarioId)
    {
        var propuestas = new List<PropuestaTrueque>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Esquema 3FN: PropuestaTrueque + DetallePropuesta
        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion,
                   ep.nombre_estado,
                   u.nombre, u.apellido, u.url_foto_perfil,
                   dp_of.prenda_id AS prenda_ofrecida_id,
                   dp_sol.prenda_id AS prenda_solicitada_id,
                   p_of.titulo_publicacion AS titulo_ofrecida,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_of.prenda_id AND es_principal = 1) AS imagen_ofrecida,
                   p_sol.titulo_publicacion AS titulo_solicitada,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_sol.prenda_id AND es_principal = 1) AS imagen_solicitada
            FROM PropuestaTrueque pt
            INNER JOIN EstadoPropuesta ep ON pt.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN Usuario u ON pt.usuario_proponente_id = u.usuario_id
            INNER JOIN DetallePropuesta dp_of ON pt.propuesta_id = dp_of.propuesta_id AND dp_of.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_of ON dp_of.prenda_id = p_of.prenda_id
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            WHERE p_sol.usuario_id = @UsuarioId
            ORDER BY pt.fecha_propuesta DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            propuestas.Add(MapearPropuesta(reader));
        }

        return propuestas;
    }

    public async Task<List<PropuestaTrueque>> ObtenerPropuestasEnviadas(int usuarioId)
    {
        var propuestas = new List<PropuestaTrueque>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion,
                   ep.nombre_estado,
                   u.nombre, u.apellido, u.url_foto_perfil,
                   dp_of.prenda_id AS prenda_ofrecida_id,
                   dp_sol.prenda_id AS prenda_solicitada_id,
                   p_of.titulo_publicacion AS titulo_ofrecida,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_of.prenda_id AND es_principal = 1) AS imagen_ofrecida,
                   p_sol.titulo_publicacion AS titulo_solicitada,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_sol.prenda_id AND es_principal = 1) AS imagen_solicitada,
                   u_receptor.usuario_id AS usuario_receptor_id,
                   u_receptor.nombre AS nombre_receptor, u_receptor.apellido AS apellido_receptor
            FROM PropuestaTrueque pt
            INNER JOIN EstadoPropuesta ep ON pt.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN Usuario u ON pt.usuario_proponente_id = u.usuario_id
            INNER JOIN DetallePropuesta dp_of ON pt.propuesta_id = dp_of.propuesta_id AND dp_of.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_of ON dp_of.prenda_id = p_of.prenda_id
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            INNER JOIN Usuario u_receptor ON p_sol.usuario_id = u_receptor.usuario_id
            WHERE pt.usuario_proponente_id = @UsuarioId
            ORDER BY pt.fecha_propuesta DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var propuesta = MapearPropuesta(reader);
            // Para propuestas enviadas, el receptor es el dueno de la prenda solicitada
            if (!reader.IsDBNull(reader.GetOrdinal("usuario_receptor_id")))
            {
                propuesta.UsuarioReceptor = new Usuario
                {
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_receptor_id")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombre_receptor")),
                    Apellido = reader.IsDBNull(reader.GetOrdinal("apellido_receptor"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("apellido_receptor"))
                };
            }
            propuestas.Add(propuesta);
        }

        return propuestas;
    }

    public async Task<int> ContarPropuestasPendientes(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            WHERE p_sol.usuario_id = @UsuarioId
              AND pt.estado_propuesta_id = 1
              AND (pt.fecha_expiracion IS NULL OR pt.fecha_expiracion > GETDATE())";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<PropuestaTrueque?> ObtenerPropuestaPorId(int propuestaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion,
                   ep.nombre_estado,
                   u.nombre, u.apellido, u.url_foto_perfil,
                   dp_of.prenda_id AS prenda_ofrecida_id,
                   dp_sol.prenda_id AS prenda_solicitada_id,
                   p_of.titulo_publicacion AS titulo_ofrecida, p_of.usuario_id AS usuario_ofrecida_id,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_of.prenda_id AND es_principal = 1) AS imagen_ofrecida,
                   p_sol.titulo_publicacion AS titulo_solicitada, p_sol.usuario_id AS usuario_solicitada_id,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_sol.prenda_id AND es_principal = 1) AS imagen_solicitada
            FROM PropuestaTrueque pt
            INNER JOIN EstadoPropuesta ep ON pt.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN Usuario u ON pt.usuario_proponente_id = u.usuario_id
            INNER JOIN DetallePropuesta dp_of ON pt.propuesta_id = dp_of.propuesta_id AND dp_of.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_of ON dp_of.prenda_id = p_of.prenda_id
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            WHERE pt.propuesta_id = @PropuestaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var propuesta = MapearPropuesta(reader);
            propuesta.PrendaOfrecida!.UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_ofrecida_id"));
            propuesta.PrendaSolicitada!.UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_solicitada_id"));
            return propuesta;
        }

        return null;
    }

    // RF-10: Aceptar propuesta
    public async Task<bool> AceptarPropuesta(int propuestaId, string? mensajeAceptacion)
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
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-11: Cambiar estado de ambas prendas a "en negociacion" (estado_publicacion_id = 2)
    public async Task<bool> CambiarEstadoPrendasANegociacion(int prendaOfrecidaId, int prendaSolicitadaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET estado_publicacion_id = 2,
                fecha_actualizacion = GETDATE()
            WHERE prenda_id IN (@PrendaOfrecidaId, @PrendaSolicitadaId)
              AND estado_publicacion_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaOfrecidaId", SqlDbType.Int) { Value = prendaOfrecidaId });
        command.Parameters.Add(new SqlParameter("@PrendaSolicitadaId", SqlDbType.Int) { Value = prendaSolicitadaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-11: Rechazar propuesta
    public async Task<bool> RechazarPropuesta(int propuestaId, string? motivoRechazo)
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
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // Cancelar propuesta
    public async Task<bool> CancelarPropuesta(int propuestaId, string? motivoCancelacion)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 7,
                fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId
              AND estado_propuesta_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // Marcar propuestas expiradas (> 7 dias sin respuesta)
    public async Task<int> MarcarPropuestasExpiradas()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 5
            WHERE estado_propuesta_id = 1
              AND fecha_expiracion IS NOT NULL
              AND fecha_expiracion < GETDATE()";

        using var command = new SqlCommand(sql, connection);
        return await command.ExecuteNonQueryAsync();
    }

    private static PropuestaTrueque MapearPropuesta(SqlDataReader reader)
    {
        var estadoId = reader.GetInt32(reader.GetOrdinal("estado_propuesta_id"));

        return new PropuestaTrueque
        {
            PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
            UsuarioProponenteId = reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
            MensajeAcompanante = reader.IsDBNull(reader.GetOrdinal("mensaje_acompanante"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("mensaje_acompanante")),
            EstadoPropuestaId = estadoId,
            FechaPropuesta = reader.GetDateTime(reader.GetOrdinal("fecha_propuesta")),
            FechaRespuesta = reader.IsDBNull(reader.GetOrdinal("fecha_respuesta"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_respuesta")),
            FechaExpiracion = reader.IsDBNull(reader.GetOrdinal("fecha_expiracion"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_expiracion")),
            PrendaOfrecidaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
            PrendaSolicitadaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
            PrendaOfrecida = new Prenda
            {
                PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
                TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_ofrecida")),
                Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_ofrecida"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("imagen_ofrecida"))
            },
            PrendaSolicitada = new Prenda
            {
                PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
                TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_solicitada")),
                Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_solicitada"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("imagen_solicitada"))
            },
            UsuarioProponente = new Usuario
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.IsDBNull(reader.GetOrdinal("apellido"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("apellido")),
                UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("url_foto_perfil"))
            }
        };
    }
}
