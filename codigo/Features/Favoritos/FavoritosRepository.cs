using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Favoritos.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Favoritos;

/// <summary>
/// Repository for favorites data access (RF-08)
/// </summary>
public class FavoritosRepository : IFavoritosRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public FavoritosRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<(List<Prenda> Prendas, int TotalCount)> ObtenerFavoritos(int usuarioId, int pagina, int itemsPorPagina)
    {
        var prendas = new List<Prenda>();
        var totalCount = 0;

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Count query
        const string countSql = @"
            SELECT COUNT(*)
            FROM Favorito f
            INNER JOIN Prenda p ON f.prenda_id = p.prenda_id
            WHERE f.usuario_id = @UsuarioId";

        using (var countCommand = new SqlCommand(countSql, connection))
        {
            countCommand.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
            totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        }

        // Data query with pagination
        var offset = (pagina - 1) * itemsPorPagina;

        const string dataSql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c_prenda.nombre_categoria,
                   ep.nombre_estado AS nombre_estado_prenda,
                   epub.nombre_estado AS nombre_estado_publicacion,
                   u.nombre AS nombre_usuario, u.url_foto_perfil,
                   co.nombre_comuna, r.nombre_region,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal,
                   f.fecha_agregado
            FROM Favorito f
            INNER JOIN Prenda p ON f.prenda_id = p.prenda_id
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN CategoriaPrenda c_prenda ON p.categoria_id = c_prenda.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            LEFT JOIN EstadoPublicacion epub ON p.estado_publicacion_id = epub.estado_publicacion_id
            LEFT JOIN Comuna co ON u.comuna_id = co.comuna_id
            LEFT JOIN Region r ON co.region_id = r.region_id
            WHERE f.usuario_id = @UsuarioId
            ORDER BY f.fecha_agregado DESC
            OFFSET @Offset ROWS FETCH NEXT @ItemsPorPagina ROWS ONLY";

        using var command = new SqlCommand(dataSql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
        command.Parameters.Add(new SqlParameter("@ItemsPorPagina", SqlDbType.Int) { Value = itemsPorPagina });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapearPrenda(reader));
        }

        return (prendas, totalCount);
    }

    public async Task<bool> EsFavorito(int usuarioId, int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(1)
            FROM Favorito
            WHERE usuario_id = @UsuarioId AND prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<List<int>> ObtenerIdsFavoritos(int usuarioId)
    {
        var ids = new List<int>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT prenda_id FROM Favorito WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    public async Task<bool> AgregarFavorito(int usuarioId, int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM Favorito WHERE usuario_id = @UsuarioId AND prenda_id = @PrendaId)
            BEGIN
                INSERT INTO Favorito (usuario_id, prenda_id, fecha_agregado)
                VALUES (@UsuarioId, @PrendaId, GETDATE())
            END";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> QuitarFavorito(int usuarioId, int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "DELETE FROM Favorito WHERE usuario_id = @UsuarioId AND prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> ContarFavoritos(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT COUNT(*) FROM Favorito WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static Prenda MapearPrenda(SqlDataReader reader)
    {
        var prenda = new Prenda
        {
            PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_id")),
            UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
            TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_publicacion")),
            DescripcionPublicacion = reader.IsDBNull(reader.GetOrdinal("descripcion_publicacion"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
            CategoriaId = reader.GetInt32(reader.GetOrdinal("categoria_id")),
            Talla = reader.GetString(reader.GetOrdinal("talla")),
            EstadoPrendaId = reader.GetInt32(reader.GetOrdinal("estado_prenda_id")),
            EstadoPublicacionId = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")),
            FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
            FechaActualizacion = reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion"))
        };

        // Set computed values
        prenda.Tipo = reader.IsDBNull(reader.GetOrdinal("nombre_categoria"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_categoria"));

        prenda.Estado = reader.IsDBNull(reader.GetOrdinal("nombre_estado_prenda"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_estado_prenda"));

        prenda.UrlImagenPrincipal = reader.IsDBNull(reader.GetOrdinal("imagen_principal"))
            ? null
            : reader.GetString(reader.GetOrdinal("imagen_principal"));

        prenda.Ubicacion = reader.IsDBNull(reader.GetOrdinal("nombre_comuna"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_comuna"));

        return prenda;
    }
}
