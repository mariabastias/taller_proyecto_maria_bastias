using System.Data.SqlClient;
using TruequeTextil.Features.SeguimientoUsuario.Interfaces;

namespace TruequeTextil.Features.SeguimientoUsuario;

public class SeguimientoUsuarioRepository : ISeguimientoUsuarioRepository
{
    private readonly string _connectionString;

    public SeguimientoUsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<bool> SeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO SeguimientoUsuario (usuario_seguidor_id, usuario_seguido_id, fecha_seguimiento)
            VALUES (@UsuarioSeguidorId, @UsuarioSeguidoId, GETDATE())";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioSeguidorId", SqlDbType.Int) { Value = usuarioSeguidorId });
        command.Parameters.Add(new SqlParameter("@UsuarioSeguidoId", SqlDbType.Int) { Value = usuarioSeguidoId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DejarDeSeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            DELETE FROM SeguimientoUsuario
            WHERE usuario_seguidor_id = @UsuarioSeguidorId AND usuario_seguido_id = @UsuarioSeguidoId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioSeguidorId", SqlDbType.Int) { Value = usuarioSeguidorId });
        command.Parameters.Add(new SqlParameter("@UsuarioSeguidoId", SqlDbType.Int) { Value = usuarioSeguidoId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EstaSiguiendo(int usuarioSeguidorId, int usuarioSeguidoId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM SeguimientoUsuario
            WHERE usuario_seguidor_id = @UsuarioSeguidorId AND usuario_seguido_id = @UsuarioSeguidoId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioSeguidorId", SqlDbType.Int) { Value = usuarioSeguidorId });
        command.Parameters.Add(new SqlParameter("@UsuarioSeguidoId", SqlDbType.Int) { Value = usuarioSeguidoId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<List<int>> ObtenerSeguidores(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT usuario_seguidor_id FROM SeguimientoUsuario
            WHERE usuario_seguido_id = @UsuarioId
            ORDER BY fecha_seguimiento DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var seguidores = new List<int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            seguidores.Add(reader.GetInt32(reader.GetOrdinal("usuario_seguidor_id")));
        }

        return seguidores;
    }

    public async Task<List<int>> ObtenerSeguidos(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT usuario_seguido_id FROM SeguimientoUsuario
            WHERE usuario_seguidor_id = @UsuarioId
            ORDER BY fecha_seguimiento DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var seguidos = new List<int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            seguidos.Add(reader.GetInt32(reader.GetOrdinal("usuario_seguido_id")));
        }

        return seguidos;
    }

    public async Task<int> ContarSeguidores(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM SeguimientoUsuario
            WHERE usuario_seguido_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count;
    }

    public async Task<int> ContarSeguidos(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM SeguimientoUsuario
            WHERE usuario_seguidor_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count;
    }
}
