using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Notificaciones.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Notificaciones;

public class NotificacionesRepository : INotificacionesRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public NotificacionesRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<Notificacion>> ObtenerNotificaciones(int usuarioId)
    {
        var notificaciones = new List<Notificacion>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
                        SELECT 
                            notificacion_id,
                            usuario_id,
                            tipo_notificacion_id,
                            titulo,
                            mensaje,
                            enlace,
                            leida,
                            fecha_creacion,
                            fecha_envio,
                            metodo_envio
                        FROM Notificacion
                        WHERE usuario_id = @UsuarioId
                        ORDER BY fecha_creacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tipoNotificacionId = reader.GetInt32(reader.GetOrdinal("tipo_notificacion_id"));

            notificaciones.Add(new Notificacion
            {
                Id = reader.GetInt32(reader.GetOrdinal("notificacion_id")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                TipoNotificacionId = tipoNotificacionId,
                Titulo = reader.GetString(reader.GetOrdinal("titulo")),
                Mensaje = reader.GetString(reader.GetOrdinal("mensaje")),
                Enlace = reader.IsDBNull(reader.GetOrdinal("enlace"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("enlace")),
                Leida = !reader.IsDBNull(reader.GetOrdinal("leida")) && reader.GetBoolean(reader.GetOrdinal("leida")),
                FechaCreacion = reader.IsDBNull(reader.GetOrdinal("fecha_creacion"))
                        ? DateTime.Now
                        : reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                FechaEnvio = reader.IsDBNull(reader.GetOrdinal("fecha_envio"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("fecha_envio")),
                MetodoEnvio = reader.IsDBNull(reader.GetOrdinal("metodo_envio"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("metodo_envio"))
            });
        }

        return notificaciones;
    }

    public async Task<int> ContarNotificacionesNoLeidas(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT COUNT(*) FROM Notificacion WHERE usuario_id = @UsuarioId AND leida = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> MarcarComoLeida(int notificacionId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "UPDATE Notificacion SET leida = 1 WHERE notificacion_id = @NotificacionId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@NotificacionId", SqlDbType.Int) { Value = notificacionId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> MarcarTodasComoLeidas(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "UPDATE Notificacion SET leida = 1 WHERE usuario_id = @UsuarioId AND leida = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> CrearNotificacion(Notificacion notificacion)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
                        INSERT INTO Notificacion
                        (
                            usuario_id,
                            tipo_notificacion_id,
                            titulo,
                            mensaje,
                            enlace,
                            leida,
                            fecha_creacion,
                            metodo_envio
                        )
                        VALUES
                        (
                            @UsuarioId,
                            @TipoNotificacionId,
                            @Titulo,
                            @Mensaje,
                            @Enlace,
                            0,
                            SYSDATETIME(),
                            @MetodoEnvio
                        );
                        SELECT SCOPE_IDENTITY();";


        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = notificacion.UsuarioId;
        command.Parameters.Add("@TipoNotificacionId", SqlDbType.Int).Value = notificacion.TipoNotificacionId;
        command.Parameters.Add("@Titulo", SqlDbType.VarChar, 200).Value = notificacion.Titulo;
        command.Parameters.Add("@Mensaje", SqlDbType.VarChar, 500).Value = notificacion.Mensaje;
        command.Parameters.Add("@Enlace", SqlDbType.VarChar, 500).Value = (object?)notificacion.Enlace ?? DBNull.Value;
        command.Parameters.Add("@MetodoEnvio", SqlDbType.VarChar, 20).Value = (object?)notificacion.MetodoEnvio ?? DBNull.Value;


        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
