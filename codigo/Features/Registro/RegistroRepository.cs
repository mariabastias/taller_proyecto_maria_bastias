using System.Data.SqlClient;
using TruequeTextil.Features.Registro.Interfaces;

namespace TruequeTextil.Features.Registro;

public class RegistroRepository : IRegistroRepository
{
    private readonly string _connectionString;

    public RegistroRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<bool> EmailDisponible(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM Usuario WHERE correo_electronico = @Email";
        using var command = new SqlCommand(query, connection);
        command.Parameters.Add(new SqlParameter("@Email", System.Data.SqlDbType.VarChar, 100) { Value = email });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count == 0;
    }

    public async Task<int> CrearUsuario(string nombre, string apellido, string email, string passwordHash, int comunaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO Usuario (correo_electronico, password_hash, nombre, apellido, comuna_id,
                                 cuenta_verificada, fecha_registro, rol, reputacion_promedio, estado_usuario)
            VALUES (@Email, @PasswordHash, @Nombre, @Apellido, @ComunaId, 0, GETDATE(), 'usuario', 0.00, 'activo');
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(query, connection);
        command.Parameters.Add(new SqlParameter("@Email", System.Data.SqlDbType.VarChar, 100) { Value = email });
        command.Parameters.Add(new SqlParameter("@PasswordHash", System.Data.SqlDbType.VarChar, 500) { Value = passwordHash });
        command.Parameters.Add(new SqlParameter("@Nombre", System.Data.SqlDbType.NVarChar, 100) { Value = nombre });
        command.Parameters.Add(new SqlParameter("@Apellido", System.Data.SqlDbType.NVarChar, 100) { Value = apellido });
        command.Parameters.Add(new SqlParameter("@ComunaId", System.Data.SqlDbType.Int) { Value = comunaId });

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return id;
    }

    public async Task GuardarTokenVerificacion(int usuarioId, string token, DateTime expiracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET token_verificacion = @Token,
                token_verificacion_expiracion = @Expiracion
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.Add(new SqlParameter("@Token", System.Data.SqlDbType.VarChar, 100) { Value = token });
        command.Parameters.Add(new SqlParameter("@Expiracion", System.Data.SqlDbType.DateTime) { Value = expiracion });
        command.Parameters.Add(new SqlParameter("@UsuarioId", System.Data.SqlDbType.Int) { Value = usuarioId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int?> ObtenerUsuarioIdPorEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT usuario_id FROM Usuario WHERE correo_electronico = @Email";
        using var command = new SqlCommand(query, connection);
        command.Parameters.Add(new SqlParameter("@Email", System.Data.SqlDbType.VarChar, 100) { Value = email });

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : null;
    }
}
