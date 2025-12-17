using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Home.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Home;

public class HomeRepository : IHomeRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public HomeRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<Prenda>> ObtenerPrendasRecientes(int limite = 8)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP (@Limite) p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                p.categoria_id, cp.nombre_categoria,
                p.talla, p.estado_prenda_id, ep.nombre_estado as estado_prenda,
                p.estado_publicacion_id, p.fecha_publicacion,
                u.nombre, u.apellido, c.nombre_comuna,
                (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN CategoriaPrenda cp ON p.categoria_id = cp.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.estado_publicacion_id = 1
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Limite", SqlDbType.Int) { Value = limite });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapearPrenda(reader));
        }

        return prendas;
    }


    public async Task<List<Prenda>> ObtenerPrendasDestacadas(int limite = 4)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP (@Limite) p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                p.categoria_id, cp.nombre_categoria,
                p.talla, p.estado_prenda_id, ep.nombre_estado as estado_prenda,
                p.estado_publicacion_id, p.fecha_publicacion,
                u.nombre, u.apellido, c.nombre_comuna,
                (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN CategoriaPrenda cp ON p.categoria_id = cp.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.estado_publicacion_id = 1
            ORDER BY p.fecha_publicacion DESC";  // Nota: se cambió el ORDER BY ya que 'vistas' no existe

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Limite", SqlDbType.Int) { Value = limite });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapearPrenda(reader));
        }

        return prendas;
    }


    public async Task<int> ContarUsuariosActivos()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*) FROM Usuario
            WHERE activo = 1 AND fecha_ultimo_login >= DATEADD(month, -1, GETDATE())";

        using var command = new SqlCommand(sql, connection);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> ContarPrendasDisponibles()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT COUNT(*) FROM Prenda WHERE estado_publicacion_id = 1";

        using var command = new SqlCommand(sql, connection);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> ContarTruequesCompletados()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT COUNT(*) FROM PropuestaTrueque WHERE estado = 'completado'";

        using var command = new SqlCommand(sql, connection);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static Prenda MapearPrenda(SqlDataReader reader)
    {
        return new Prenda
        {
            Id = reader.GetInt32(reader.GetOrdinal("prenda_id")),
            Titulo = reader.GetString(reader.GetOrdinal("titulo_publicacion")),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion_publicacion"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
            Tipo = reader.GetString(reader.GetOrdinal("nombre_categoria")),
            Talla = reader.GetString(reader.GetOrdinal("talla")),
            Estado = reader.GetString(reader.GetOrdinal("estado_prenda")),
            Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_principal"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("imagen_principal")),
            FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
            Ubicacion = reader.IsDBNull(reader.GetOrdinal("nombre_comuna"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("nombre_comuna")),
            Usuario = new Usuario
            {
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("apellido"))
            }
        };
    }

}
