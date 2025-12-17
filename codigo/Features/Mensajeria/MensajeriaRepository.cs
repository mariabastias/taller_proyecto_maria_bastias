using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Mensajeria.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Mensajeria;

public class MensajeriaRepository : IMensajeriaRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public MensajeriaRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId)
    {
        var mensajes = new List<MensajeNegociacion>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT m.mensaje_id, m.propuesta_id, m.usuario_id, m.mensaje_texto,
                   m.tipo_mensaje, m.fecha_envio, m.leido,
                   u.nombre AS usuario_nombre, u.url_foto_perfil
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
                TipoMensaje = reader.IsDBNull(reader.GetOrdinal("tipo_mensaje"))
                    ? "texto"
                    : reader.GetString(reader.GetOrdinal("tipo_mensaje")),
                FechaEnvio = reader.GetDateTime(reader.GetOrdinal("fecha_envio")),
                Leido = reader.GetBoolean(reader.GetOrdinal("leido")),
                Usuario = new Usuario
                {
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    Nombre = reader.GetString(reader.GetOrdinal("usuario_nombre")),
                    UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("url_foto_perfil"))
                }
            });
        }

        return mensajes;
    }

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

    public async Task<bool> MarcarMensajesComoLeidos(int propuestaId, int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Marcar como leidos los mensajes que NO fueron enviados por el usuario actual
        const string sql = @"
            UPDATE MensajeNegociacion
            SET leido = 1
            WHERE propuesta_id = @PropuestaId
              AND usuario_id != @UsuarioId
              AND leido = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected >= 0;
    }

    public async Task<int> ContarMensajesNoLeidos(int propuestaId, int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Contar mensajes no leidos que NO fueron enviados por el usuario actual
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

    public async Task<int> ContarTotalMensajesNoLeidos(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Contar mensajes no leidos en todas las propuestas donde participa el usuario
        const string sql = @"
            SELECT COUNT(*)
            FROM MensajeNegociacion m
            INNER JOIN PropuestaTrueque p ON m.propuesta_id = p.propuesta_id
            INNER JOIN DetallePropuesta dp ON p.propuesta_id = dp.propuesta_id
            INNER JOIN Prenda pr ON dp.prenda_id = pr.prenda_id
            WHERE m.usuario_id != @UsuarioId
              AND m.leido = 0
              AND (p.usuario_proponente_id = @UsuarioId OR pr.usuario_id = @UsuarioId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<ConversacionResumen>> ObtenerConversaciones(int usuarioId)
    {
        var conversaciones = new List<ConversacionResumen>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            WITH PropuestasUsuario AS (
                SELECT DISTINCT p.propuesta_id, p.usuario_proponente_id, p.estado_propuesta_id, p.fecha_propuesta,
                       CASE WHEN p.usuario_proponente_id = @UsuarioId THEN 1 ELSE 0 END AS es_proponente
                FROM PropuestaTrueque p
                INNER JOIN DetallePropuesta dp ON p.propuesta_id = dp.propuesta_id
                INNER JOIN Prenda pr ON dp.prenda_id = pr.prenda_id
                WHERE p.usuario_proponente_id = @UsuarioId OR pr.usuario_id = @UsuarioId
            ),
            UltimoMensaje AS (
                SELECT propuesta_id, MAX(fecha_envio) AS ultima_fecha
                FROM MensajeNegociacion
                GROUP BY propuesta_id
            )
            SELECT
                pu.propuesta_id,
                pu.es_proponente,
                pu.fecha_propuesta,
                ep.nombre_estado AS estado_propuesta,
                -- Prenda ofrecida (del proponente)
                po.prenda_id AS prenda_ofrecida_id,
                po.titulo_publicacion AS titulo_ofrecida,
                co.nombre_categoria AS tipo_ofrecida,
                (SELECT TOP 1 pio.imagen_url FROM ImagenPrenda pio WHERE pio.prenda_id = po.prenda_id AND pio.es_principal = 1) AS imagen_ofrecida,
                -- Prenda solicitada (del receptor)
                ps.prenda_id AS prenda_solicitada_id,
                ps.titulo_publicacion AS titulo_solicitada,
                cs.nombre_categoria AS tipo_solicitada,
                (SELECT TOP 1 pis.imagen_url FROM ImagenPrenda pis WHERE pis.prenda_id = ps.prenda_id AND pis.es_principal = 1) AS imagen_solicitada,
                -- Otro usuario
                CASE WHEN pu.es_proponente = 1 THEN ps.usuario_id ELSE po.usuario_id END AS otro_usuario_id,
                CASE WHEN pu.es_proponente = 1 THEN ur.nombre ELSE up.nombre END AS nombre_otro_usuario,
                CASE WHEN pu.es_proponente = 1 THEN ur.url_foto_perfil ELSE up.url_foto_perfil END AS url_foto_otro_usuario,
                -- Ultimo mensaje
                um.mensaje_texto AS ultimo_mensaje,
                um.fecha_envio AS fecha_ultimo_mensaje,
                -- No leidos
                (SELECT COUNT(*) FROM MensajeNegociacion mn
                 WHERE mn.propuesta_id = pu.propuesta_id
                   AND mn.usuario_id != @UsuarioId AND mn.leido = 0) AS mensajes_no_leidos
            FROM PropuestasUsuario pu
            INNER JOIN EstadoPropuesta ep ON pu.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN DetallePropuesta dpo ON pu.propuesta_id = dpo.propuesta_id AND dpo.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dps ON pu.propuesta_id = dps.propuesta_id AND dps.tipo_participacion = 'solicitada'
            INNER JOIN Prenda po ON dpo.prenda_id = po.prenda_id
            INNER JOIN Prenda ps ON dps.prenda_id = ps.prenda_id
            INNER JOIN CategoriaPrenda co ON po.categoria_id = co.categoria_id
            INNER JOIN CategoriaPrenda cs ON ps.categoria_id = cs.categoria_id
            INNER JOIN Usuario up ON pu.usuario_proponente_id = up.usuario_id
            INNER JOIN Usuario ur ON ps.usuario_id = ur.usuario_id
            LEFT JOIN UltimoMensaje ulm ON pu.propuesta_id = ulm.propuesta_id
            LEFT JOIN MensajeNegociacion um ON um.propuesta_id = pu.propuesta_id AND um.fecha_envio = ulm.ultima_fecha
            WHERE EXISTS (SELECT 1 FROM MensajeNegociacion m WHERE m.propuesta_id = pu.propuesta_id)
            ORDER BY COALESCE(um.fecha_envio, pu.fecha_propuesta) DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        try
        {
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversaciones.Add(new ConversacionResumen
                {
                    PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                    EsProponente = reader.GetInt32(reader.GetOrdinal("es_proponente")) == 1,
                    EstadoPropuesta = reader.GetString(reader.GetOrdinal("estado_propuesta")),
                    PrendaOfrecidaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
                    TituloPrendaOfrecida = reader.GetString(reader.GetOrdinal("titulo_ofrecida")),
                    TipoPrendaOfrecida = reader.GetString(reader.GetOrdinal("tipo_ofrecida")),
                    ImagenPrendaOfrecida = reader.IsDBNull(reader.GetOrdinal("imagen_ofrecida"))
                        ? "/images/placeholder.png"
                        : reader.GetString(reader.GetOrdinal("imagen_ofrecida")),
                    PrendaSolicitadaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
                    TituloPrendaSolicitada = reader.GetString(reader.GetOrdinal("titulo_solicitada")),
                    TipoPrendaSolicitada = reader.GetString(reader.GetOrdinal("tipo_solicitada")),
                    ImagenPrendaSolicitada = reader.IsDBNull(reader.GetOrdinal("imagen_solicitada"))
                        ? "/images/placeholder.png"
                        : reader.GetString(reader.GetOrdinal("imagen_solicitada")),
                    OtroUsuarioId = reader.GetInt32(reader.GetOrdinal("otro_usuario_id")),
                    NombreOtroUsuario = reader.GetString(reader.GetOrdinal("nombre_otro_usuario")),
                    FotoOtroUsuario = reader.IsDBNull(reader.GetOrdinal("url_foto_otro_usuario"))
                        ? "/images/default-avatar.png"
                        : reader.GetString(reader.GetOrdinal("url_foto_otro_usuario")),
                    UltimoMensaje = reader.IsDBNull(reader.GetOrdinal("ultimo_mensaje"))
                        ? ""
                        : reader.GetString(reader.GetOrdinal("ultimo_mensaje")),
                    FechaUltimoMensaje = reader.IsDBNull(reader.GetOrdinal("fecha_ultimo_mensaje"))
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("fecha_ultimo_mensaje")),
                    MensajesNoLeidos = reader.GetInt32(reader.GetOrdinal("mensajes_no_leidos"))
                });
            }
        }
        catch
        {
            // Si falla la query compleja, usar una mas simple
            return await ObtenerConversacionesSimple(usuarioId);
        }

        return conversaciones;
    }

    private async Task<List<ConversacionResumen>> ObtenerConversacionesSimple(int usuarioId)
    {
        var conversaciones = new List<ConversacionResumen>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT DISTINCT
                p.propuesta_id,
                p.usuario_proponente_id,
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN 1 ELSE 0 END AS es_proponente,
                ep.nombre_estado AS estado_propuesta,
                -- Prenda ofrecida
                po.prenda_id AS prenda_ofrecida_id,
                po.titulo_publicacion AS titulo_ofrecida,
                COALESCE(co.nombre_categoria, '') AS tipo_ofrecida,
                COALESCE((SELECT TOP 1 pio.imagen_url FROM ImagenPrenda pio WHERE pio.prenda_id = po.prenda_id AND pio.es_principal = 1), '/images/placeholder.png') AS imagen_ofrecida,
                -- Prenda solicitada
                ps.prenda_id AS prenda_solicitada_id,
                ps.titulo_publicacion AS titulo_solicitada,
                COALESCE(cs.nombre_categoria, '') AS tipo_solicitada,
                COALESCE((SELECT TOP 1 pis.imagen_url FROM ImagenPrenda pis WHERE pis.prenda_id = ps.prenda_id AND pis.es_principal = 1), '/images/placeholder.png') AS imagen_solicitada,
                -- Otro usuario
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN ps.usuario_id ELSE po.usuario_id END AS otro_usuario_id,
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN ur.nombre ELSE up.nombre END AS nombre_otro_usuario,
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN ur.url_foto_perfil ELSE up.url_foto_perfil END AS url_foto_otro_usuario,
                -- Ultimo mensaje
                (SELECT TOP 1 m.mensaje_texto FROM MensajeNegociacion m
                 WHERE m.propuesta_id = p.propuesta_id ORDER BY m.fecha_envio DESC) AS ultimo_mensaje,
                (SELECT TOP 1 m.fecha_envio FROM MensajeNegociacion m
                 WHERE m.propuesta_id = p.propuesta_id ORDER BY m.fecha_envio DESC) AS fecha_ultimo_mensaje,
                (SELECT COUNT(*) FROM MensajeNegociacion m
                 WHERE m.propuesta_id = p.propuesta_id AND m.usuario_id != @UsuarioId AND m.leido = 0) AS mensajes_no_leidos
            FROM PropuestaTrueque p
            INNER JOIN EstadoPropuesta ep ON p.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN DetallePropuesta dpo ON p.propuesta_id = dpo.propuesta_id AND dpo.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dps ON p.propuesta_id = dps.propuesta_id AND dps.tipo_participacion = 'solicitada'
            INNER JOIN Prenda po ON dpo.prenda_id = po.prenda_id
            INNER JOIN Prenda ps ON dps.prenda_id = ps.prenda_id
            LEFT JOIN CategoriaPrenda co ON po.categoria_id = co.categoria_id
            LEFT JOIN CategoriaPrenda cs ON ps.categoria_id = cs.categoria_id
            INNER JOIN Usuario up ON p.usuario_proponente_id = up.usuario_id
            INNER JOIN Usuario ur ON ps.usuario_id = ur.usuario_id
            WHERE (p.usuario_proponente_id = @UsuarioId OR ps.usuario_id = @UsuarioId OR po.usuario_id = @UsuarioId)
              AND EXISTS (SELECT 1 FROM MensajeNegociacion m WHERE m.propuesta_id = p.propuesta_id)
            ORDER BY fecha_ultimo_mensaje DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            conversaciones.Add(new ConversacionResumen
            {
                PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                EsProponente = reader.GetInt32(reader.GetOrdinal("es_proponente")) == 1,
                EstadoPropuesta = reader.GetString(reader.GetOrdinal("estado_propuesta")),
                PrendaOfrecidaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
                TituloPrendaOfrecida = reader.GetString(reader.GetOrdinal("titulo_ofrecida")),
                TipoPrendaOfrecida = reader.GetString(reader.GetOrdinal("tipo_ofrecida")),
                ImagenPrendaOfrecida = reader.IsDBNull(reader.GetOrdinal("imagen_ofrecida"))
                    ? "/images/placeholder.png"
                    : reader.GetString(reader.GetOrdinal("imagen_ofrecida")),
                PrendaSolicitadaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
                TituloPrendaSolicitada = reader.GetString(reader.GetOrdinal("titulo_solicitada")),
                TipoPrendaSolicitada = reader.GetString(reader.GetOrdinal("tipo_solicitada")),
                ImagenPrendaSolicitada = reader.IsDBNull(reader.GetOrdinal("imagen_solicitada"))
                    ? "/images/placeholder.png"
                    : reader.GetString(reader.GetOrdinal("imagen_solicitada")),
                OtroUsuarioId = reader.GetInt32(reader.GetOrdinal("otro_usuario_id")),
                NombreOtroUsuario = reader.GetString(reader.GetOrdinal("nombre_otro_usuario")),
                FotoOtroUsuario = reader.IsDBNull(reader.GetOrdinal("url_foto_otro_usuario"))
                    ? "/images/default-avatar.png"
                    : reader.GetString(reader.GetOrdinal("url_foto_otro_usuario")),
                UltimoMensaje = reader.IsDBNull(reader.GetOrdinal("ultimo_mensaje"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("ultimo_mensaje")),
                FechaUltimoMensaje = reader.IsDBNull(reader.GetOrdinal("fecha_ultimo_mensaje"))
                    ? DateTime.MinValue
                    : reader.GetDateTime(reader.GetOrdinal("fecha_ultimo_mensaje")),
                MensajesNoLeidos = reader.GetInt32(reader.GetOrdinal("mensajes_no_leidos"))
            });
        }

        return conversaciones;
    }

    public async Task<bool> PuedeEnviarMensaje(int propuestaId, int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Verificar que el usuario participa en la propuesta y que esta activa (pendiente o aceptada)
        const string sql = @"
            SELECT COUNT(*)
            FROM PropuestaTrueque p
            INNER JOIN DetallePropuesta dp ON p.propuesta_id = dp.propuesta_id
            INNER JOIN Prenda pr ON dp.prenda_id = pr.prenda_id
            WHERE p.propuesta_id = @PropuestaId
              AND (p.usuario_proponente_id = @UsuarioId OR pr.usuario_id = @UsuarioId)
              AND p.estado_propuesta_id IN (1, 2)"; // 1=Pendiente, 2=Aceptada

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<MensajeNegociacion?> ObtenerUltimoMensaje(int propuestaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP 1 m.mensaje_id, m.propuesta_id, m.usuario_id, m.mensaje_texto,
                   m.tipo_mensaje, m.fecha_envio, m.leido,
                   u.nombre AS usuario_nombre, u.url_foto_perfil
            FROM MensajeNegociacion m
            INNER JOIN Usuario u ON m.usuario_id = u.usuario_id
            WHERE m.propuesta_id = @PropuestaId
            ORDER BY m.fecha_envio DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new MensajeNegociacion
            {
                MensajeId = reader.GetInt32(reader.GetOrdinal("mensaje_id")),
                PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                MensajeTexto = reader.GetString(reader.GetOrdinal("mensaje_texto")),
                TipoMensaje = reader.IsDBNull(reader.GetOrdinal("tipo_mensaje"))
                    ? "texto"
                    : reader.GetString(reader.GetOrdinal("tipo_mensaje")),
                FechaEnvio = reader.GetDateTime(reader.GetOrdinal("fecha_envio")),
                Leido = reader.GetBoolean(reader.GetOrdinal("leido")),
                Usuario = new Usuario
                {
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    Nombre = reader.GetString(reader.GetOrdinal("usuario_nombre")),
                    UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("url_foto_perfil"))
                }
            };
        }

        return null;
    }

    // RF-12: Archivar conversacion cuando propuesta finaliza
    public async Task<bool> ArchivarConversacion(int propuestaId, string motivoCierre)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Insertar mensaje de cierre del sistema y marcar conversacion como archivada
        const string sql = @"
            -- Insertar mensaje de cierre
            INSERT INTO MensajeNegociacion (propuesta_id, usuario_id, mensaje_texto, tipo_mensaje, fecha_envio, leido)
            SELECT @PropuestaId, usuario_proponente_id, @MotivoCierre, 'sistema', GETDATE(), 0
            FROM PropuestaTrueque WHERE propuesta_id = @PropuestaId;

            -- Marcar como archivado (si existe la columna)
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MensajeNegociacion') AND name = 'archivado')
            BEGIN
                UPDATE MensajeNegociacion SET archivado = 1 WHERE propuesta_id = @PropuestaId;
            END";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@MotivoCierre", SqlDbType.NVarChar, 500) { Value = motivoCierre });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-12: Enviar mensaje de sistema (cierre de negociacion)
    public async Task<int> EnviarMensajeSistema(int propuestaId, string mensaje)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Usar usuario_id = 0 o el proponente para mensajes de sistema
        const string sql = @"
            INSERT INTO MensajeNegociacion (propuesta_id, usuario_id, mensaje_texto, tipo_mensaje, fecha_envio, leido)
            SELECT @PropuestaId, usuario_proponente_id, @MensajeTexto, 'sistema', GETDATE(), 0
            FROM PropuestaTrueque WHERE propuesta_id = @PropuestaId;
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@MensajeTexto", SqlDbType.NVarChar, 1000) { Value = mensaje });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<MensajeNegociacion?> ObtenerMensajePorId(int mensajeId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT m.mensaje_id, m.propuesta_id, m.usuario_id, m.mensaje_texto,
                   m.tipo_mensaje, m.fecha_envio, m.leido,
                   u.nombre AS usuario_nombre, u.url_foto_perfil
            FROM MensajeNegociacion m
            INNER JOIN Usuario u ON m.usuario_id = u.usuario_id
            WHERE m.mensaje_id = @MensajeId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@MensajeId", SqlDbType.Int) { Value = mensajeId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new MensajeNegociacion
            {
                MensajeId = reader.GetInt32(reader.GetOrdinal("mensaje_id")),
                PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                MensajeTexto = reader.GetString(reader.GetOrdinal("mensaje_texto")),
                TipoMensaje = reader.IsDBNull(reader.GetOrdinal("tipo_mensaje"))
                    ? "texto"
                    : reader.GetString(reader.GetOrdinal("tipo_mensaje")),
                FechaEnvio = reader.GetDateTime(reader.GetOrdinal("fecha_envio")),
                Leido = reader.GetBoolean(reader.GetOrdinal("leido")),
                Usuario = new Usuario
                {
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    Nombre = reader.GetString(reader.GetOrdinal("usuario_nombre")),
                    UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("url_foto_perfil"))
                }
            };
        }

        return null;
    }

    public async Task<ConversacionResumen?> ObtenerPropuestaPorId(int propuestaId, int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT
                p.propuesta_id,
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN 1 ELSE 0 END AS es_proponente,
                ep.nombre_estado AS estado_propuesta,
                -- Prenda ofrecida
                po.prenda_id AS prenda_ofrecida_id,
                po.titulo_publicacion AS titulo_ofrecida,
                COALESCE(co.nombre_categoria, '') AS tipo_ofrecida,
                COALESCE((SELECT TOP 1 pio.imagen_url FROM ImagenPrenda pio WHERE pio.prenda_id = po.prenda_id AND pio.es_principal = 1), '/images/placeholder.png') AS imagen_ofrecida,
                -- Prenda solicitada
                ps.prenda_id AS prenda_solicitada_id,
                ps.titulo_publicacion AS titulo_solicitada,
                COALESCE(cs.nombre_categoria, '') AS tipo_solicitada,
                COALESCE((SELECT TOP 1 pis.imagen_url FROM ImagenPrenda pis WHERE pis.prenda_id = ps.prenda_id AND pis.es_principal = 1), '/images/placeholder.png') AS imagen_solicitada,
                -- Otro usuario
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN ps.usuario_id ELSE po.usuario_id END AS otro_usuario_id,
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN ur.nombre ELSE up.nombre END AS nombre_otro_usuario,
                CASE WHEN p.usuario_proponente_id = @UsuarioId THEN ur.url_foto_perfil ELSE up.url_foto_perfil END AS url_foto_otro_usuario
            FROM PropuestaTrueque p
            INNER JOIN EstadoPropuesta ep ON p.estado_propuesta_id = ep.estado_propuesta_id
            INNER JOIN DetallePropuesta dpo ON p.propuesta_id = dpo.propuesta_id AND dpo.tipo_participacion = 'ofrecida'
            INNER JOIN DetallePropuesta dps ON p.propuesta_id = dps.propuesta_id AND dps.tipo_participacion = 'solicitada'
            INNER JOIN Prenda po ON dpo.prenda_id = po.prenda_id
            INNER JOIN Prenda ps ON dps.prenda_id = ps.prenda_id
            LEFT JOIN CategoriaPrenda co ON po.categoria_id = co.categoria_id
            LEFT JOIN CategoriaPrenda cs ON ps.categoria_id = cs.categoria_id
            INNER JOIN Usuario up ON p.usuario_proponente_id = up.usuario_id
            INNER JOIN Usuario ur ON ps.usuario_id = ur.usuario_id
            WHERE p.propuesta_id = @PropuestaId
              AND (p.usuario_proponente_id = @UsuarioId OR ps.usuario_id = @UsuarioId OR po.usuario_id = @UsuarioId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PropuestaId", SqlDbType.Int) { Value = propuestaId });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ConversacionResumen
            {
                PropuestaId = reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                EsProponente = reader.GetInt32(reader.GetOrdinal("es_proponente")) == 1,
                EstadoPropuesta = reader.GetString(reader.GetOrdinal("estado_propuesta")),
                PrendaOfrecidaId = reader.GetInt32(reader.GetOrdinal("prenda_ofrecida_id")),
                TituloPrendaOfrecida = reader.GetString(reader.GetOrdinal("titulo_ofrecida")),
                TipoPrendaOfrecida = reader.GetString(reader.GetOrdinal("tipo_ofrecida")),
                ImagenPrendaOfrecida = reader.IsDBNull(reader.GetOrdinal("imagen_ofrecida"))
                    ? "/images/placeholder.png"
                    : reader.GetString(reader.GetOrdinal("imagen_ofrecida")),
                PrendaSolicitadaId = reader.GetInt32(reader.GetOrdinal("prenda_solicitada_id")),
                TituloPrendaSolicitada = reader.GetString(reader.GetOrdinal("titulo_solicitada")),
                TipoPrendaSolicitada = reader.GetString(reader.GetOrdinal("tipo_solicitada")),
                ImagenPrendaSolicitada = reader.IsDBNull(reader.GetOrdinal("imagen_solicitada"))
                    ? "/images/placeholder.png"
                    : reader.GetString(reader.GetOrdinal("imagen_solicitada")),
                OtroUsuarioId = reader.GetInt32(reader.GetOrdinal("otro_usuario_id")),
                NombreOtroUsuario = reader.GetString(reader.GetOrdinal("nombre_otro_usuario")),
                FotoOtroUsuario = reader.IsDBNull(reader.GetOrdinal("url_foto_otro_usuario"))
                    ? "/images/default-avatar.png"
                    : reader.GetString(reader.GetOrdinal("url_foto_otro_usuario"))
            };
        }

        return null;
    }
}
