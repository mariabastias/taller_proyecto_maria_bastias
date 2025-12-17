using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.PublicarPrenda.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PublicarPrenda;

public class PublicarPrendaRepository : IPublicarPrendaRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public PublicarPrendaRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<int> CrearPrenda(Prenda prenda)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO Prenda (usuario_id, titulo_publicacion, descripcion_publicacion,
                                categoria_id, talla, estado_prenda_id, estado_publicacion_id,
                                fecha_publicacion, fecha_actualizacion)
            VALUES (@UsuarioId, @Titulo, @Descripcion, @CategoriaId, @Talla,
                    @EstadoPrendaId, @EstadoPublicacionId, @FechaPublicacion, @FechaActualizacion);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = prenda.UsuarioId });
        command.Parameters.Add(new SqlParameter("@Titulo", SqlDbType.NVarChar, 100) { Value = prenda.TituloPublicacion });
        command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = prenda.DescripcionPublicacion });
        command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = prenda.CategoriaId });
        command.Parameters.Add(new SqlParameter("@Talla", SqlDbType.NVarChar, 20) { Value = prenda.Talla });
        command.Parameters.Add(new SqlParameter("@EstadoPrendaId", SqlDbType.Int) { Value = prenda.EstadoPrendaId });
        command.Parameters.Add(new SqlParameter("@EstadoPublicacionId", SqlDbType.Int) { Value = 1 }); // disponible
        command.Parameters.Add(new SqlParameter("@FechaPublicacion", SqlDbType.DateTime2) { Value = DateTime.Now });
        command.Parameters.Add(new SqlParameter("@FechaActualizacion", SqlDbType.DateTime2) { Value = DateTime.Now });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
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

    public async Task<bool> ActualizarImagenPrincipal(int prendaId, string imageUrl)
    {
        // Este método no debería usarse ya que no hay columna url_imagen_principal en Prenda
        // Las imágenes se manejan únicamente en la tabla ImagenPrenda
        // Si se necesita actualizar la imagen principal, modificar la lógica en ImagenPrenda
        throw new NotImplementedException("La columna url_imagen_principal no existe en la tabla Prenda. Las imágenes se manejan en ImagenPrenda.");
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
