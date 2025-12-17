using System.Data;
using System.Data.SqlClient;
using System.Text;
using TruequeTextil.Features.Explorar.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Explorar;

public class ExplorarRepository : IExplorarRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public ExplorarRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    // RF-07: Busqueda paginada con filtros. RNF-01: 20 items/pagina
    public async Task<(List<Prenda> Prendas, int TotalCount)> ObtenerPrendasDisponibles(
        string? busqueda = null,
        int? categoriaId = null,
        string? talla = null,
        int? estadoPrendaId = null,
        int? regionId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pagina = 1,
        int itemsPorPagina = 20)
    {
        var prendas = new List<Prenda>();
        var totalCount = 0;

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var parameters = new List<SqlParameter>();

        // Build WHERE clause
        var whereBuilder = new StringBuilder("WHERE p.estado_publicacion_id = 1");

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            whereBuilder.Append(" AND (p.titulo_publicacion LIKE @Busqueda OR p.descripcion_publicacion LIKE @Busqueda)");
            parameters.Add(new SqlParameter("@Busqueda", SqlDbType.NVarChar, 200) { Value = $"%{busqueda}%" });
        }

        if (categoriaId.HasValue && categoriaId > 0)
        {
            whereBuilder.Append(" AND p.categoria_id = @CategoriaId");
            parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = categoriaId.Value });
        }

        if (!string.IsNullOrWhiteSpace(talla))
        {
            whereBuilder.Append(" AND p.talla = @Talla");
            parameters.Add(new SqlParameter("@Talla", SqlDbType.NVarChar, 20) { Value = talla });
        }

        if (estadoPrendaId.HasValue && estadoPrendaId > 0)
        {
            whereBuilder.Append(" AND p.estado_prenda_id = @EstadoPrendaId");
            parameters.Add(new SqlParameter("@EstadoPrendaId", SqlDbType.Int) { Value = estadoPrendaId.Value });
        }

        if (regionId.HasValue && regionId > 0)
        {
            whereBuilder.Append(" AND r.region_id = @RegionId");
            parameters.Add(new SqlParameter("@RegionId", SqlDbType.Int) { Value = regionId.Value });
        }

        // RF-07: Filtro por rango de fechas
        if (fechaDesde.HasValue)
        {
            whereBuilder.Append(" AND p.fecha_publicacion >= @FechaDesde");
            parameters.Add(new SqlParameter("@FechaDesde", SqlDbType.DateTime2) { Value = fechaDesde.Value.Date });
        }

        if (fechaHasta.HasValue)
        {
            whereBuilder.Append(" AND p.fecha_publicacion <= @FechaHasta");
            parameters.Add(new SqlParameter("@FechaHasta", SqlDbType.DateTime2) { Value = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1) });
        }

        var whereClause = whereBuilder.ToString();

        // Count query
        var countSql = $@"
            SELECT COUNT(*)
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN Region r ON c.region_id = r.region_id
            {whereClause}";

        using (var countCommand = new SqlCommand(countSql, connection))
        {
            countCommand.Parameters.AddRange(parameters.Select(p => CloneParameter(p)).ToArray());
            totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        }

        // Data query with pagination (OFFSET-FETCH for SQL Server 2012+)
        var offset = (pagina - 1) * itemsPorPagina;

        var dataSql = $@"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c_prenda.nombre_categoria,
                   ep.nombre_estado AS nombre_estado_prenda,
                   u.nombre AS nombre_usuario, u.url_foto_perfil,
                   co.nombre_comuna, r.nombre_region,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN CategoriaPrenda c_prenda ON p.categoria_id = c_prenda.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            LEFT JOIN Comuna co ON u.comuna_id = co.comuna_id
            LEFT JOIN Region r ON co.region_id = r.region_id
            {whereClause}
            ORDER BY p.fecha_publicacion DESC
            OFFSET @Offset ROWS FETCH NEXT @ItemsPorPagina ROWS ONLY";

        using var command = new SqlCommand(dataSql, connection);
        command.Parameters.AddRange(parameters.ToArray());
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
        command.Parameters.Add(new SqlParameter("@ItemsPorPagina", SqlDbType.Int) { Value = itemsPorPagina });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapearPrenda(reader));
        }

        return (prendas, totalCount);
    }

    public async Task<List<CategoriaPrenda>> ObtenerCategorias()
    {
        var categorias = new List<CategoriaPrenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT categoria_id, nombre_categoria FROM CategoriaPrenda ORDER BY nombre_categoria";
        using var command = new SqlCommand(sql, connection);

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

    public async Task<List<string>> ObtenerTallas()
    {
        var tallas = new List<string>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT DISTINCT talla FROM Prenda WHERE estado_publicacion_id = 1 ORDER BY talla";
        using var command = new SqlCommand(sql, connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tallas.Add(reader.GetString(0));
        }

        return tallas;
    }

    public async Task<List<EstadoPrenda>> ObtenerEstadosPrenda()
    {
        var estados = new List<EstadoPrenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT estado_id, nombre_estado FROM EstadoPrenda ORDER BY estado_id";
        using var command = new SqlCommand(sql, connection);

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

    public async Task<List<Region>> ObtenerRegiones()
    {
        var regiones = new List<Region>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT region_id, nombre_region FROM Region ORDER BY nombre_region";
        using var command = new SqlCommand(sql, connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            regiones.Add(new Region
            {
                RegionId = reader.GetInt32(0),
                NombreRegion = reader.GetString(1)
            });
        }

        return regiones;
    }

    private static SqlParameter CloneParameter(SqlParameter original)
    {
        return new SqlParameter(original.ParameterName, original.SqlDbType, original.Size)
        {
            Value = original.Value
        };
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
