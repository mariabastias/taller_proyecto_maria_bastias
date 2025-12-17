using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.EditarPrenda.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPrenda;

public class EditarPrendaRepository : IEditarPrendaRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public EditarPrendaRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria,
                   ep.nombre_estado as estado_prenda_nombre,
                   epp.nombre_estado as estado_publicacion_nombre,
                   u.nombre, u.apellido, u.url_foto_perfil
            FROM Prenda p
            INNER JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            INNER JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            INNER JOIN EstadoPublicacion epp ON p.estado_publicacion_id = epp.estado_publicacion_id
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            WHERE p.prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Prenda
            {
                PrendaId = reader.GetInt32(0),
                UsuarioId = reader.GetInt32(1),
                TituloPublicacion = reader.GetString(2),
                DescripcionPublicacion = reader.GetString(3),
                CategoriaId = reader.GetInt32(4),
                Talla = reader.GetString(5),
                EstadoPrendaId = reader.GetInt32(6),
                EstadoPublicacionId = reader.GetInt32(7),
                FechaPublicacion = reader.GetDateTime(8),
                FechaActualizacion = reader.GetDateTime(9),
                Categoria = new CategoriaPrenda
                {
                    CategoriaId = reader.GetInt32(4),
                    NombreCategoria = reader.GetString(10)
                },
                EstadoPrendaNav = new EstadoPrenda
                {
                    EstadoId = reader.GetInt32(6),
                    NombreEstado = reader.GetString(11)
                },
                EstadoPublicacionNav = new EstadoPublicacion
                {
                    EstadoPublicacionId = reader.GetInt32(7),
                    NombreEstado = reader.GetString(12)
                },
                Usuario = new Usuario
                {
                    UsuarioId = reader.GetInt32(1),
                    Nombre = reader.GetString(13),
                    Apellido = reader.GetString(14),
                    UrlFotoPerfil = reader.IsDBNull(15) ? null : reader.GetString(15)
                }
            };
        }

        return null;
    }

    public async Task<bool> ActualizarPrenda(Prenda prenda)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET titulo_publicacion = @Titulo,
                descripcion_publicacion = @Descripcion,
                categoria_id = @CategoriaId,
                talla = @Talla,
                estado_prenda_id = @EstadoPrendaId,
                fecha_actualizacion = @FechaActualizacion
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prenda.PrendaId });
        command.Parameters.Add(new SqlParameter("@Titulo", SqlDbType.NVarChar, 100) { Value = prenda.TituloPublicacion });
        command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = prenda.DescripcionPublicacion });
        command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = prenda.CategoriaId });
        command.Parameters.Add(new SqlParameter("@Talla", SqlDbType.NVarChar, 20) { Value = prenda.Talla });
        command.Parameters.Add(new SqlParameter("@EstadoPrendaId", SqlDbType.Int) { Value = prenda.EstadoPrendaId });
        command.Parameters.Add(new SqlParameter("@FechaActualizacion", SqlDbType.DateTime2) { Value = DateTime.Now });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EliminarImagenesPrenda(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "DELETE FROM ImagenPrenda WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected >= 0; // Success if no error
    }

    public async Task<bool> AgregarImagenPrenda(int prendaId, string imageUrl, int orden, bool esPrincipal)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO ImagenPrenda (prenda_id, imagen_url, orden, es_principal, fecha_subida)
            VALUES (@PrendaId, @ImagenUrl, @Orden, @EsPrincipal, @FechaSubida)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
        command.Parameters.Add(new SqlParameter("@ImagenUrl", SqlDbType.NVarChar, 500) { Value = imageUrl });
        command.Parameters.Add(new SqlParameter("@Orden", SqlDbType.Int) { Value = orden });
        command.Parameters.Add(new SqlParameter("@EsPrincipal", SqlDbType.Bit) { Value = esPrincipal });
        command.Parameters.Add(new SqlParameter("@FechaSubida", SqlDbType.DateTime2) { Value = DateTime.Now });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<List<CategoriaPrenda>> ObtenerCategorias()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT categoria_id, nombre_categoria FROM CategoriaPrenda ORDER BY nombre_categoria";

        using var command = new SqlCommand(sql, connection);
        var categorias = new List<CategoriaPrenda>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categorias.Add(new CategoriaPrenda
            {
                CategoriaId = reader.GetInt32(0),
                NombreCategoria = reader.GetString(1)
            });
        }

        return categorias;
    }

    public async Task<List<EstadoPrenda>> ObtenerEstadosPrenda()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT estado_id, nombre_estado FROM EstadoPrenda ORDER BY estado_id";

        using var command = new SqlCommand(sql, connection);
        var estados = new List<EstadoPrenda>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            estados.Add(new EstadoPrenda
            {
                EstadoId = reader.GetInt32(0),
                NombreEstado = reader.GetString(1)
            });
        }

        return estados;
    }
}