using System.Data.SqlClient;
using TruequeTextil.Features.ConfiguracionModulo.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionModulo;

public class ConfiguracionModuloRepository : IConfiguracionModuloRepository
{
    private readonly string _connectionString;

    public ConfiguracionModuloRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<ConfiguracionModuloModel?> ObtenerConfiguracionPorUsuarioYModulo(int usuarioId, string modulo)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT config_modulo_id, modulo, usuario_id, configuracion_json, fecha_actualizacion
            FROM ConfiguracionModulo
            WHERE usuario_id = @UsuarioId AND modulo = @Modulo";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@Modulo", SqlDbType.NVarChar, 100) { Value = modulo });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ConfiguracionModuloModel
            {
                ConfigModuloId = reader.GetInt32(reader.GetOrdinal("config_modulo_id")),
                Modulo = reader.GetString(reader.GetOrdinal("modulo")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                ConfiguracionJson = reader.IsDBNull(reader.GetOrdinal("configuracion_json"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("configuracion_json")),
                FechaActualizacion = reader.IsDBNull(reader.GetOrdinal("fecha_actualizacion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion"))
            };
        }

        return null;
    }

    public async Task<List<ConfiguracionModuloModel>> ObtenerConfiguracionesPorUsuario(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT config_modulo_id, modulo, usuario_id, configuracion_json, fecha_actualizacion
            FROM ConfiguracionModulo
            WHERE usuario_id = @UsuarioId
            ORDER BY modulo";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var configuraciones = new List<ConfiguracionModuloModel>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            configuraciones.Add(new ConfiguracionModuloModel
            {
                ConfigModuloId = reader.GetInt32(reader.GetOrdinal("config_modulo_id")),
                Modulo = reader.GetString(reader.GetOrdinal("modulo")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                ConfiguracionJson = reader.IsDBNull(reader.GetOrdinal("configuracion_json"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("configuracion_json")),
                FechaActualizacion = reader.IsDBNull(reader.GetOrdinal("fecha_actualizacion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion"))
            });
        }

        return configuraciones;
    }

    public async Task<bool> CrearConfiguracion(ConfiguracionModuloModel configuracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO ConfiguracionModulo (modulo, usuario_id, configuracion_json, fecha_actualizacion)
            VALUES (@Modulo, @UsuarioId, @ConfiguracionJson, GETDATE())";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Modulo", SqlDbType.NVarChar, 100) { Value = configuracion.Modulo });
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = configuracion.UsuarioId });
        command.Parameters.Add(new SqlParameter("@ConfiguracionJson", SqlDbType.NVarChar, -1)
        { Value = configuracion.ConfiguracionJson as object ?? DBNull.Value });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ActualizarConfiguracion(ConfiguracionModuloModel configuracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            UPDATE ConfiguracionModulo
            SET configuracion_json = @ConfiguracionJson, fecha_actualizacion = GETDATE()
            WHERE usuario_id = @UsuarioId AND modulo = @Modulo";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = configuracion.UsuarioId });
        command.Parameters.Add(new SqlParameter("@Modulo", SqlDbType.NVarChar, 100) { Value = configuracion.Modulo });
        command.Parameters.Add(new SqlParameter("@ConfiguracionJson", SqlDbType.NVarChar, -1)
        { Value = configuracion.ConfiguracionJson as object ?? DBNull.Value });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EliminarConfiguracion(int usuarioId, string modulo)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM ConfiguracionModulo WHERE usuario_id = @UsuarioId AND modulo = @Modulo";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@Modulo", SqlDbType.NVarChar, 100) { Value = modulo });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EliminarTodasConfiguracionesUsuario(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM ConfiguracionModulo WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
