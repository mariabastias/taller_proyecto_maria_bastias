using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Onboarding.Interfaces;
using TruequeTextil.Shared.Infrastructure;

namespace TruequeTextil.Features.Onboarding;

public class OnboardingRepository : IOnboardingRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public OnboardingRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
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

    public async Task MarcarOnboardingCompletado(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "UPDATE Usuario SET onboarding_completado = 1 WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        await command.ExecuteNonQueryAsync();
    }
}
