using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.DetallePropuesta.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePropuesta;

public class DetallePropuestaRepository : IDetallePropuestaRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public DetallePropuestaRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Esquema 3FN con DetallePropuesta
        const string sql = @"
            SELECT pt.propuesta_id, pt.usuario_proponente_id, pt.mensaje_acompanante,
                   pt.estado_propuesta_id, pt.fecha_propuesta, pt.fecha_respuesta, pt.fecha_expiracion,
                   pt.prioridad, pt.es_contraoferta,
                   ep.nombre_estado,
                   dp_of.prenda_id AS prenda_ofrecida_id,
                   dp_sol.prenda_id AS prenda_solicitada_id,
                   p_of.titulo_publicacion AS titulo_ofrecida, p_of.descripcion_publicacion AS desc_ofrecida,
                   p_of.talla AS talla_ofrecida, p_of.usuario_id AS usuario_ofrecida_id,
                   c_of.nombre_categoria AS tipo_ofrecida,
                   ep_of.nombre_estado AS estado_prenda_ofrecida,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_of.prenda_id AND es_principal = 1) AS imagen_ofrecida,
                   p_sol.titulo_publicacion AS titulo_solicitada, p_sol.descripcion_publicacion AS desc_solicitada,
                   p_sol.talla AS talla_solicitada, p_sol.usuario_id AS usuario_solicitada_id,
                   c_sol.nombre_categoria AS tipo_solicitada,
                   ep_sol.nombre_estado AS estado_prenda_solicitada,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = dp_sol.prenda_id AND es_principal = 1) AS imagen_solicitada,
                   u_prop.nombre AS nombre_proponente, u_prop.apellido AS apellido_proponente,
                   u_prop.url_foto_perfil AS foto_proponente, u_prop.reputacion_promedio AS rep_proponente,
                   u_rec.usuario_id AS usuario_receptor_id, u_rec.nombre AS nombre_receptor, u_rec.apellido AS apellido_receptor,
                   u_rec.url_foto_perfil AS foto_receptor, u_rec.reputacion_promedio AS rep_receptor
            FROM PropuestaTrueque pt
            INNER JOIN EstadoPropuesta ep ON pt.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN DetallePropuesta dp_of ON pt.propuesta_id = dp_of.propuesta_id AND dp_of.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_of ON dp_of.prenda_id = p_of.prenda_id
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            LEFT JOIN CategoriaPrenda c_of ON p_of.categoria_id = c_of.categoria_id
            LEFT JOIN CategoriaPrenda c_sol ON p_sol.categoria_id = c_sol.categoria_id
            LEFT JOIN EstadoPrenda ep_of ON p_of.estado_prenda_id = ep_of.estado_id
            LEFT JOIN EstadoPrenda ep_sol ON p_sol.estado_prenda_id = ep_sol.estado_id
            INNER JOIN Usuario u_prop ON pt.usuario_proponente_id = u_prop.usuario_id
            INNER JOIN Usuario u_rec ON p_sol.usuario_id = u_rec.usuario_id
            WHERE pt.propuesta_id = @PropuestaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapearPropuesta(reader);
        }

        return null;
    }

    public async Task<bool> AceptarPropuesta(int propuestaId, string? mensajeAceptacion = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 2, fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId AND estado_propuesta_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> RechazarPropuesta(int propuestaId, string? motivoRechazo = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 3, fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId AND estado_propuesta_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CancelarPropuesta(int propuestaId, string? motivoCancelacion = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 7, fecha_respuesta = GETDATE()
            WHERE propuesta_id = @PropuestaId AND estado_propuesta_id IN (1, 2)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CompletarTrueque(int propuestaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Marcar propuesta como completada
            const string sqlPropuesta = @"
                UPDATE PropuestaTrueque
                SET estado_propuesta_id = 6, fecha_respuesta = GETDATE()
                WHERE propuesta_id = @PropuestaId AND estado_propuesta_id = 2";

            using (var command = new SqlCommand(sqlPropuesta, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
                var rows = await command.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            // Marcar prendas como intercambiadas (estado_publicacion_id = 4)
            const string sqlPrendas = @"
                UPDATE Prenda
                SET estado_publicacion_id = 4
                WHERE prenda_id IN (
                    SELECT prenda_id FROM DetallePropuesta WHERE propuesta_id = @PropuestaId
                )";

            using (var command = new SqlCommand(sqlPrendas, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ActualizarDisponibilidadPrendas(int prendaOfrecidaId, int prendaSolicitadaId, bool disponible)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var estadoId = disponible ? 1 : 2; // 1=Disponible, 2=Reservada

        const string sql = @"
            UPDATE Prenda SET estado_publicacion_id = @EstadoId
            WHERE prenda_id IN (@PrendaOfrecidaId, @PrendaSolicitadaId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@EstadoId", SqlDbType.Int) { Value = estadoId });
        command.Parameters.Add(new SqlParameter("@PrendaOfrecidaId", SqlDbType.Int) { Value = prendaOfrecidaId });
        command.Parameters.Add(new SqlParameter("@PrendaSolicitadaId", SqlDbType.Int) { Value = prendaSolicitadaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-12: Obtener mensajes de negociacion
    public async Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId)
    {
        var mensajes = new List<MensajeNegociacion>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT m.mensaje_id, m.propuesta_id, m.usuario_id, m.mensaje_texto,
                   m.tipo_mensaje, m.fecha_envio, m.leido,
                   u.nombre, u.apellido, u.url_foto_perfil
            FROM MensajeNegociacion m
            INNER JOIN Usuario u ON m.usuario_id = u.usuario_id
            WHERE m.propuesta_id = @PropuestaId
            ORDER BY m.fecha_envio ASC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            mensajes.Add(new MensajeNegociacion
            {
                MensajeId = reader.GetInt32(reader.GetOrdinal("mensaje_id")),
                PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                MensajeTexto = reader.GetString(reader.GetOrdinal("mensaje_texto")),
                TipoMensaje = reader.GetString(reader.GetOrdinal("tipo_mensaje")),
                FechaEnvio = reader.GetDateTime(reader.GetOrdinal("fecha_envio")),
                Leido = reader.GetBoolean(reader.GetOrdinal("leido")),
                Usuario = new Usuario
                {
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    Apellido = reader.IsDBNull(reader.GetOrdinal("apellido"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("apellido")),
                    UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("url_foto_perfil"))
                }
            });
        }

        return mensajes;
    }

    // RF-12: Enviar mensaje
    public async Task<int> EnviarMensaje(int propuestaId, int usuarioId, string mensaje)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO MensajeNegociacion (propuesta_id, usuario_id, mensaje_texto, tipo_mensaje, fecha_envio, leido)
            VALUES (@PropuestaId, @UsuarioId, @MensajeTexto, 'texto', GETDATE(), 0);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@MensajeTexto", SqlDbType.NVarChar, 1000) { Value = mensaje });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // RF-12: Marcar mensajes como leidos
    public async Task<bool> MarcarMensajesComoLeidos(int propuestaId, int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Marcar como leidos los mensajes que NO fueron enviados por este usuario
        const string sql = @"
            UPDATE MensajeNegociacion
            SET leido = 1
            WHERE propuesta_id = @PropuestaId
              AND usuario_id != @UsuarioId
              AND leido = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        await command.ExecuteNonQueryAsync();
        return true;
    }

    // RF-12: Contar mensajes no leidos
    public async Task<int> ContarMensajesNoLeidos(int propuestaId, int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM MensajeNegociacion
            WHERE propuesta_id = @PropuestaId
              AND usuario_id != @UsuarioId
              AND leido = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static PropuestaTrueque MapearPropuesta(SqlDataReader reader)
    {
        return new PropuestaTrueque
        {
            PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
            UsuarioProponenteId = reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
            MensajeAcompanante = reader.IsDBNull(reader.GetOrdinal("mensaje_acompanante"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("mensaje_acompanante")),
            EstadoPropuestaId = reader.GetInt32(reader.GetOrdinal("estado_propuesta_id")),
            FechaPropuesta = reader.GetDateTime(reader.GetOrdinal("fecha_propuesta")),
            FechaRespuesta = reader.IsDBNull(reader.GetOrdinal("fecha_respuesta"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_respuesta")),
            FechaExpiracion = reader.IsDBNull(reader.GetOrdinal("fecha_expiracion"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_expiracion")),
            Prioridad = reader.GetInt32(reader.GetOrdinal("prioridad")),
            EsContraoferta = reader.GetBoolean(reader.GetOrdinal("es_contraoferta")),
            PrendaOfrecidaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
            PrendaSolicitadaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
            UsuarioReceptorId = reader.GetInt32(reader.GetOrdinal("usuario_receptor_id")),
            PrendaOfrecida = new Prenda
            {
                PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
                TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_ofrecida")),
                DescripcionPublicacion = reader.IsDBNull(reader.GetOrdinal("desc_ofrecida"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("desc_ofrecida")),
                Talla = reader.GetString(reader.GetOrdinal("talla_ofrecida")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_ofrecida_id")),
                Tipo = reader.IsDBNull(reader.GetOrdinal("tipo_ofrecida"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("tipo_ofrecida")),
                Estado = reader.IsDBNull(reader.GetOrdinal("estado_prenda_ofrecida"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("estado_prenda_ofrecida")),
                Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_ofrecida"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("imagen_ofrecida"))
            },
            PrendaSolicitada = new Prenda
            {
                PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
                TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_solicitada")),
                DescripcionPublicacion = reader.IsDBNull(reader.GetOrdinal("desc_solicitada"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("desc_solicitada")),
                Talla = reader.GetString(reader.GetOrdinal("talla_solicitada")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_solicitada_id")),
                Tipo = reader.IsDBNull(reader.GetOrdinal("tipo_solicitada"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("tipo_solicitada")),
                Estado = reader.IsDBNull(reader.GetOrdinal("estado_prenda_solicitada"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("estado_prenda_solicitada")),
                Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_solicitada"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("imagen_solicitada"))
            },
            UsuarioProponente = new Usuario
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre_proponente")),
                Apellido = reader.IsDBNull(reader.GetOrdinal("apellido_proponente"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("apellido_proponente")),
                UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("foto_proponente"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("foto_proponente")),
                ReputacionPromedio = reader.IsDBNull(reader.GetOrdinal("rep_proponente"))
                    ? 0
                    : reader.GetDecimal(reader.GetOrdinal("rep_proponente"))
            },
            UsuarioReceptor = new Usuario
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_receptor_id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre_receptor")),
                Apellido = reader.IsDBNull(reader.GetOrdinal("apellido_receptor"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("apellido_receptor")),
                UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("foto_receptor"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("foto_receptor")),
                ReputacionPromedio = reader.IsDBNull(reader.GetOrdinal("rep_receptor"))
                    ? 0
                    : reader.GetDecimal(reader.GetOrdinal("rep_receptor"))
            }
        };
    }
}
