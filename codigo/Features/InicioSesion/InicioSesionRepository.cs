using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.InicioSesion.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.InicioSesion;

public class InicioSesionRepository : IInicioSesionRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public InicioSesionRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<Usuario?> ObtenerUsuarioPorEmail(string email)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.correo_electronico, u.password_hash, u.nombre, u.apellido,
                   u.cuenta_verificada, u.estado_usuario, u.rol, u.fecha_ultimo_login, u.comuna_id
            FROM Usuario u
            WHERE u.correo_electronico = @Email";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                CorreoElectronico = reader.GetString(reader.GetOrdinal("correo_electronico")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("apellido")),
                CuentaVerificada = reader.GetBoolean(reader.GetOrdinal("cuenta_verificada")),
                EstadoUsuario = reader.GetString(reader.GetOrdinal("estado_usuario")),
                Rol = reader.GetString(reader.GetOrdinal("rol")),
                FechaUltimoLogin = reader.IsDBNull(reader.GetOrdinal("fecha_ultimo_login"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_ultimo_login")),
                ComunaId = reader.GetInt32(reader.GetOrdinal("comuna_id"))
            };
        }

        return null;
    }

    public async Task<string?> ObtenerEstadoUsuario(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT estado_usuario FROM Usuario WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }

    public async Task ActualizarUltimoLogin(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "UPDATE Usuario SET fecha_ultimo_login = @FechaUltimoLogin WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@FechaUltimoLogin", SqlDbType.DateTime2) { Value = DateTime.Now });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> OnboardingCompletado(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT onboarding_completado FROM Usuario WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value && (bool)result;
    }

    public async Task<bool> PerfilCompleto(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT CASE
                WHEN nombre IS NOT NULL AND nombre != ''
                 AND apellido IS NOT NULL AND apellido != ''
                 AND comuna_id IS NOT NULL AND comuna_id > 0
                 AND biografia IS NOT NULL AND biografia != ''
                THEN 1 ELSE 0 END
            FROM Usuario WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value && (int)result == 1;
    }
}
