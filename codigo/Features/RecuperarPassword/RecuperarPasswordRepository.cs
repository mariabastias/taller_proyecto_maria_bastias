using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.RecuperarPassword.Interfaces;
using TruequeTextil.Shared.Infrastructure;

namespace TruequeTextil.Features.RecuperarPassword;

public class RecuperarPasswordRepository : IRecuperarPasswordRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public RecuperarPasswordRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<int?> ObtenerUsuarioIdPorEmail(string email)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT usuario_id FROM Usuario WHERE correo_electronico = @Email";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email });

        var result = await command.ExecuteScalarAsync();
        return result != null ? (int?)result : null;
    }

    public async Task GuardarTokenRecuperacion(int usuarioId, string token, DateTime expiracion)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Invalidar tokens anteriores y crear nuevo
        const string sql = @"
            UPDATE TokensVerificacion SET usado = 1 WHERE usuario_id = @UsuarioId AND tipo_token = 'recuperacion';
            INSERT INTO TokensVerificacion (usuario_id, token, tipo_token, fecha_creacion, fecha_expiracion, usado)
            VALUES (@UsuarioId, @Token, 'recuperacion', @FechaCreacion, @FechaExpiracion, 0);";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@Token", SqlDbType.NVarChar, 256) { Value = token });
        command.Parameters.Add(new SqlParameter("@FechaCreacion", SqlDbType.DateTime2) { Value = DateTime.Now });
        command.Parameters.Add(new SqlParameter("@FechaExpiracion", SqlDbType.DateTime2) { Value = expiracion });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int?> ValidarTokenRecuperacion(string token)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT usuario_id
            FROM TokensVerificacion
            WHERE token = @Token
              AND tipo_token = 'recuperacion'
              AND fecha_expiracion > @FechaActual
              AND usado = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Token", SqlDbType.NVarChar, 256) { Value = token });
        command.Parameters.Add(new SqlParameter("@FechaActual", SqlDbType.DateTime2) { Value = DateTime.Now });

        var result = await command.ExecuteScalarAsync();
        return result != null ? (int?)result : null;
    }

    public async Task ActualizarPassword(int usuarioId, string passwordHash)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Usuario SET password_hash = @PasswordHash WHERE usuario_id = @UsuarioId;
            UPDATE TokensVerificacion SET usado = 1 WHERE usuario_id = @UsuarioId AND tipo_token = 'recuperacion';";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 256) { Value = passwordHash });

        await command.ExecuteNonQueryAsync();
    }
}
