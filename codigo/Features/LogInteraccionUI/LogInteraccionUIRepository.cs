using System.Data.SqlClient;
using TruequeTextil.Features.LogInteraccionUI.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.LogInteraccionUI;

public class LogInteraccionUIRepository : ILogInteraccionUIRepository
{
    private readonly string _connectionString;

    public LogInteraccionUIRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<bool> RegistrarInteraccion(LogInteraccionUIModel interaccion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO LogInteraccionUI (usuario_id, elemento_ui, accion, valor_anterior, valor_nuevo,
                                         fecha_interaccion, ip_address, user_agent)
            VALUES (@UsuarioId, @ElementoUi, @Accion, @ValorAnterior, @ValorNuevo,
                    @FechaInteraccion, @IpAddress, @UserAgent)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int)
        { Value = interaccion.UsuarioId as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@ElementoUi", SqlDbType.NVarChar, 255) { Value = interaccion.ElementoUi });
        command.Parameters.Add(new SqlParameter("@Accion", SqlDbType.NVarChar, 100) { Value = interaccion.Accion });
        command.Parameters.Add(new SqlParameter("@ValorAnterior", SqlDbType.NVarChar, -1)
        { Value = interaccion.ValorAnterior as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@ValorNuevo", SqlDbType.NVarChar, -1)
        { Value = interaccion.ValorNuevo as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@FechaInteraccion", SqlDbType.DateTime2)
        { Value = interaccion.FechaInteraccion ?? DateTime.Now });
        command.Parameters.Add(new SqlParameter("@IpAddress", SqlDbType.NVarChar, 45)
        { Value = interaccion.IpAddress as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@UserAgent", SqlDbType.NVarChar, 500)
        { Value = interaccion.UserAgent as object ?? DBNull.Value });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<List<LogInteraccionUIModel>> ObtenerInteraccionesPorUsuario(int? usuarioId, int limite = 100)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP (@Limite) log_id, usuario_id, elemento_ui, accion, valor_anterior, valor_nuevo,
                   fecha_interaccion, ip_address, user_agent
            FROM LogInteraccionUI
            WHERE (@UsuarioId IS NULL OR usuario_id = @UsuarioId)
            ORDER BY fecha_interaccion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int)
        { Value = usuarioId as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Limite", SqlDbType.Int) { Value = limite });

        var interacciones = new List<LogInteraccionUIModel>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            interacciones.Add(new LogInteraccionUIModel
            {
                LogId = reader.GetInt32(reader.GetOrdinal("log_id")),
                UsuarioId = reader.IsDBNull(reader.GetOrdinal("usuario_id"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("usuario_id")),
                ElementoUi = reader.GetString(reader.GetOrdinal("elemento_ui")),
                Accion = reader.GetString(reader.GetOrdinal("accion")),
                ValorAnterior = reader.IsDBNull(reader.GetOrdinal("valor_anterior"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valor_anterior")),
                ValorNuevo = reader.IsDBNull(reader.GetOrdinal("valor_nuevo"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valor_nuevo")),
                FechaInteraccion = reader.IsDBNull(reader.GetOrdinal("fecha_interaccion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_interaccion")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ip_address")),
                UserAgent = reader.IsDBNull(reader.GetOrdinal("user_agent"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("user_agent"))
            });
        }

        return interacciones;
    }

    public async Task<List<LogInteraccionUIModel>> ObtenerInteraccionesPorElemento(string elementoUi, int limite = 100)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP (@Limite) log_id, usuario_id, elemento_ui, accion, valor_anterior, valor_nuevo,
                   fecha_interaccion, ip_address, user_agent
            FROM LogInteraccionUI
            WHERE elemento_ui = @ElementoUi
            ORDER BY fecha_interaccion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ElementoUi", SqlDbType.NVarChar, 255) { Value = elementoUi });
        command.Parameters.Add(new SqlParameter("@Limite", SqlDbType.Int) { Value = limite });

        var interacciones = new List<LogInteraccionUIModel>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            interacciones.Add(new LogInteraccionUIModel
            {
                LogId = reader.GetInt32(reader.GetOrdinal("log_id")),
                UsuarioId = reader.IsDBNull(reader.GetOrdinal("usuario_id"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("usuario_id")),
                ElementoUi = reader.GetString(reader.GetOrdinal("elemento_ui")),
                Accion = reader.GetString(reader.GetOrdinal("accion")),
                ValorAnterior = reader.IsDBNull(reader.GetOrdinal("valor_anterior"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valor_anterior")),
                ValorNuevo = reader.IsDBNull(reader.GetOrdinal("valor_nuevo"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valor_nuevo")),
                FechaInteraccion = reader.IsDBNull(reader.GetOrdinal("fecha_interaccion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_interaccion")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ip_address")),
                UserAgent = reader.IsDBNull(reader.GetOrdinal("user_agent"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("user_agent"))
            });
        }

        return interacciones;
    }

    public async Task<List<LogInteraccionUIModel>> ObtenerInteraccionesRecientes(int limite = 50)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP (@Limite) log_id, usuario_id, elemento_ui, accion, valor_anterior, valor_nuevo,
                   fecha_interaccion, ip_address, user_agent
            FROM LogInteraccionUI
            ORDER BY fecha_interaccion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Limite", SqlDbType.Int) { Value = limite });

        var interacciones = new List<LogInteraccionUIModel>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            interacciones.Add(new LogInteraccionUIModel
            {
                LogId = reader.GetInt32(reader.GetOrdinal("log_id")),
                UsuarioId = reader.IsDBNull(reader.GetOrdinal("usuario_id"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("usuario_id")),
                ElementoUi = reader.GetString(reader.GetOrdinal("elemento_ui")),
                Accion = reader.GetString(reader.GetOrdinal("accion")),
                ValorAnterior = reader.IsDBNull(reader.GetOrdinal("valor_anterior"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valor_anterior")),
                ValorNuevo = reader.IsDBNull(reader.GetOrdinal("valor_nuevo"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valor_nuevo")),
                FechaInteraccion = reader.IsDBNull(reader.GetOrdinal("fecha_interaccion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_interaccion")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ip_address")),
                UserAgent = reader.IsDBNull(reader.GetOrdinal("user_agent"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("user_agent"))
            });
        }

        return interacciones;
    }

    public async Task<int> ContarInteraccionesPorUsuario(int? usuarioId, DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT COUNT(*)
            FROM LogInteraccionUI
            WHERE (@UsuarioId IS NULL OR usuario_id = @UsuarioId)
              AND (@Desde IS NULL OR fecha_interaccion >= @Desde)
              AND (@Hasta IS NULL OR fecha_interaccion <= @Hasta)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int)
        { Value = usuarioId as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Desde", SqlDbType.DateTime2)
        { Value = desde as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Hasta", SqlDbType.DateTime2)
        { Value = hasta as object ?? DBNull.Value });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count;
    }
}
