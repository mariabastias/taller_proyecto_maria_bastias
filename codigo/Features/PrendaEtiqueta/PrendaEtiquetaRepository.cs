using System.Data.SqlClient;
using TruequeTextil.Features.PrendaEtiqueta.Interfaces;

namespace TruequeTextil.Features.PrendaEtiqueta;

public class PrendaEtiquetaRepository : IPrendaEtiquetaRepository
{
    private readonly string _connectionString;

    public PrendaEtiquetaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<bool> AgregarEtiquetaAPrenda(int prendaId, int etiquetaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO PrendaEtiqueta (prenda_id, etiqueta_id)
            VALUES (@PrendaId, @EtiquetaId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
        command.Parameters.Add(new SqlParameter("@EtiquetaId", SqlDbType.Int) { Value = etiquetaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> RemoverEtiquetaDePrenda(int prendaId, int etiquetaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            DELETE FROM PrendaEtiqueta
            WHERE prenda_id = @PrendaId AND etiqueta_id = @EtiquetaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
        command.Parameters.Add(new SqlParameter("@EtiquetaId", SqlDbType.Int) { Value = etiquetaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<List<int>> ObtenerEtiquetasDePrenda(int prendaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT etiqueta_id FROM PrendaEtiqueta
            WHERE prenda_id = @PrendaId
            ORDER BY etiqueta_id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var etiquetas = new List<int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            etiquetas.Add(reader.GetInt32(reader.GetOrdinal("etiqueta_id")));
        }

        return etiquetas;
    }

    public async Task<List<int>> ObtenerPrendasConEtiqueta(int etiquetaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT prenda_id FROM PrendaEtiqueta
            WHERE etiqueta_id = @EtiquetaId
            ORDER BY prenda_id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@EtiquetaId", SqlDbType.Int) { Value = etiquetaId });

        var prendas = new List<int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(reader.GetInt32(reader.GetOrdinal("prenda_id")));
        }

        return prendas;
    }

    public async Task<bool> PrendaTieneEtiqueta(int prendaId, int etiquetaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM PrendaEtiqueta
            WHERE prenda_id = @PrendaId AND etiqueta_id = @EtiquetaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
        command.Parameters.Add(new SqlParameter("@EtiquetaId", SqlDbType.Int) { Value = etiquetaId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<int> ContarEtiquetasDePrenda(int prendaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM PrendaEtiqueta
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count;
    }

    public async Task<int> ContarPrendasConEtiqueta(int etiquetaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM PrendaEtiqueta
            WHERE etiqueta_id = @EtiquetaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@EtiquetaId", SqlDbType.Int) { Value = etiquetaId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count;
    }
}
