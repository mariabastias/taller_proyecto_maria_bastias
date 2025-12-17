using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Repositories;

public class PrendaSharedRepository : IPrendaSharedRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public PrendaSharedRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<Prenda>> GetPrendasAsync()
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.url_imagen_principal, p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria, ep.nombre_estado
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            prendas.Add(MapPrendaFromReader(reader));
        }

        return prendas;
    }

    public async Task<List<Prenda>> GetPrendasDisponiblesAsync()
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.url_imagen_principal, p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria, ep.nombre_estado
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.estado_publicacion_id = 1
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            prendas.Add(MapPrendaFromReader(reader));
        }

        return prendas;
    }

    public async Task<Prenda?> GetPrendaByIdAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.url_imagen_principal, p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria, ep.nombre_estado
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = id });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPrendaFromReader(reader);
        }

        return null;
    }

    public async Task<List<Prenda>> GetPrendasByUsuarioIdAsync(int usuarioId)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.url_imagen_principal, p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria, ep.nombre_estado
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.usuario_id = @UsuarioId
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapPrendaFromReader(reader));
        }

        return prendas;
    }

    public async Task<int> CreatePrendaAsync(Prenda prenda)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO Prenda (usuario_id, titulo_publicacion, descripcion_publicacion,
                                categoria_id, talla, estado_prenda_id, estado_publicacion_id,
                                url_imagen_principal, fecha_publicacion, fecha_actualizacion)
            VALUES (@UsuarioId, @TituloPublicacion, @DescripcionPublicacion,
                    @CategoriaId, @Talla, @EstadoPrendaId, @EstadoPublicacionId,
                    @UrlImagenPrincipal, GETDATE(), GETDATE());
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = prenda.UsuarioId });
        command.Parameters.Add(new SqlParameter("@TituloPublicacion", SqlDbType.VarChar, 200) { Value = prenda.TituloPublicacion });
        command.Parameters.Add(new SqlParameter("@DescripcionPublicacion", SqlDbType.VarChar, 1000) { Value = prenda.DescripcionPublicacion });
        command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = prenda.CategoriaId });
        command.Parameters.Add(new SqlParameter("@Talla", SqlDbType.VarChar, 10) { Value = prenda.Talla });
        command.Parameters.Add(new SqlParameter("@EstadoPrendaId", SqlDbType.Int) { Value = prenda.EstadoPrendaId });
        command.Parameters.Add(new SqlParameter("@EstadoPublicacionId", SqlDbType.Int) { Value = prenda.EstadoPublicacionId });
        command.Parameters.Add(new SqlParameter("@UrlImagenPrincipal", SqlDbType.VarChar, 500) { Value = (object?)prenda.UrlImagenPrincipal ?? DBNull.Value });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdatePrendaAsync(Prenda prenda)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET titulo_publicacion = @TituloPublicacion,
                descripcion_publicacion = @DescripcionPublicacion,
                categoria_id = @CategoriaId,
                talla = @Talla,
                estado_prenda_id = @EstadoPrendaId,
                estado_publicacion_id = @EstadoPublicacionId,
                url_imagen_principal = @UrlImagenPrincipal,
                fecha_actualizacion = GETDATE()
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prenda.PrendaId });
        command.Parameters.Add(new SqlParameter("@TituloPublicacion", SqlDbType.VarChar, 200) { Value = prenda.TituloPublicacion });
        command.Parameters.Add(new SqlParameter("@DescripcionPublicacion", SqlDbType.VarChar, 1000) { Value = prenda.DescripcionPublicacion });
        command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = prenda.CategoriaId });
        command.Parameters.Add(new SqlParameter("@Talla", SqlDbType.VarChar, 10) { Value = prenda.Talla });
        command.Parameters.Add(new SqlParameter("@EstadoPrendaId", SqlDbType.Int) { Value = prenda.EstadoPrendaId });
        command.Parameters.Add(new SqlParameter("@EstadoPublicacionId", SqlDbType.Int) { Value = prenda.EstadoPublicacionId });
        command.Parameters.Add(new SqlParameter("@UrlImagenPrincipal", SqlDbType.VarChar, 500) { Value = (object?)prenda.UrlImagenPrincipal ?? DBNull.Value });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeletePrendaAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "DELETE FROM Prenda WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CambiarDisponibilidadAsync(int id, bool disponible)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET estado_publicacion_id = @EstadoPublicacionId,
                fecha_actualizacion = GETDATE()
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = id });
        command.Parameters.Add(new SqlParameter("@EstadoPublicacionId", SqlDbType.Int) { Value = disponible ? 1 : 2 });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> IncrementarVistasAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET fecha_actualizacion = GETDATE()
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Prenda MapPrendaFromReader(SqlDataReader reader)
    {
        return new Prenda
        {
            PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_id")),
            UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
            TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_publicacion")),
            DescripcionPublicacion = reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
            CategoriaId = reader.GetInt32(reader.GetOrdinal("categoria_id")),
            Talla = reader.GetString(reader.GetOrdinal("talla")),
            EstadoPrendaId = reader.GetInt32(reader.GetOrdinal("estado_prenda_id")),
            EstadoPublicacionId = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")),
            UrlImagenPrincipal = reader.IsDBNull(reader.GetOrdinal("url_imagen_principal"))
                ? null
                : reader.GetString(reader.GetOrdinal("url_imagen_principal")),
            FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
            FechaActualizacion = reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion")),
            Categoria = !reader.IsDBNull(reader.GetOrdinal("nombre_categoria"))
                ? new CategoriaPrenda
                {
                    CategoriaId = reader.GetInt32(reader.GetOrdinal("categoria_id")),
                    NombreCategoria = reader.GetString(reader.GetOrdinal("nombre_categoria"))
                }
                : null,
            EstadoPrendaNav = !reader.IsDBNull(reader.GetOrdinal("nombre_estado"))
                ? new EstadoPrenda
                {
                    EstadoId = reader.GetInt32(reader.GetOrdinal("estado_prenda_id")),
                    NombreEstado = reader.GetString(reader.GetOrdinal("nombre_estado"))
                }
                : null
        };
    }
}
