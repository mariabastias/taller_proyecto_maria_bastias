using TruequeTextil.Features.Explorar.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Explorar;

public class ExplorarService : IExplorarService
{
    private readonly IExplorarRepository _repository;
    private readonly ILogger<ExplorarService> _logger;

    private const int ITEMS_POR_PAGINA = 20; // RNF-01

    public ExplorarService(
        IExplorarRepository repository,
        ILogger<ExplorarService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // RF-07: Busqueda con paginacion (RNF-01: 20 items/pagina) + filtro por fecha
    public async Task<PaginacionResultado<Prenda>> BuscarPrendas(
        string? busqueda = null,
        int? categoriaId = null,
        string? talla = null,
        int? estadoPrendaId = null,
        int? regionId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pagina = 1)
    {
        try
        {
            if (pagina < 1) pagina = 1;

            var (prendas, totalCount) = await _repository.ObtenerPrendasDisponibles(
                busqueda, categoriaId, talla, estadoPrendaId, regionId, fechaDesde, fechaHasta, pagina, ITEMS_POR_PAGINA);

            var totalPaginas = (int)Math.Ceiling((double)totalCount / ITEMS_POR_PAGINA);

            return new PaginacionResultado<Prenda>(
                Items: prendas,
                TotalCount: totalCount,
                PaginaActual: pagina,
                TotalPaginas: totalPaginas,
                TienePaginaAnterior: pagina > 1,
                TienePaginaSiguiente: pagina < totalPaginas
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar prendas con filtros: busqueda={Busqueda}, categoriaId={CategoriaId}, talla={Talla}, estadoPrendaId={EstadoPrendaId}, regionId={RegionId}, fechaDesde={FechaDesde}, fechaHasta={FechaHasta}, pagina={Pagina}",
                busqueda, categoriaId, talla, estadoPrendaId, regionId, fechaDesde, fechaHasta, pagina);

            return new PaginacionResultado<Prenda>(
                Items: new List<Prenda>(),
                TotalCount: 0,
                PaginaActual: 1,
                TotalPaginas: 0,
                TienePaginaAnterior: false,
                TienePaginaSiguiente: false
            );
        }
    }

    public async Task<List<CategoriaPrenda>> ObtenerCategorias()
    {
        try
        {
            return await _repository.ObtenerCategorias();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorias");
            return GetCategoriasFallback();
        }
    }

    public async Task<List<string>> ObtenerTallas()
    {
        try
        {
            var tallas = await _repository.ObtenerTallas();
            if (tallas.Count == 0)
            {
                return GetTallasFallback();
            }
            return tallas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tallas");
            return GetTallasFallback();
        }
    }

    public async Task<List<EstadoPrenda>> ObtenerEstadosPrenda()
    {
        try
        {
            return await _repository.ObtenerEstadosPrenda();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estados de prenda");
            return GetEstadosFallback();
        }
    }

    public async Task<List<Region>> ObtenerRegiones()
    {
        try
        {
            return await _repository.ObtenerRegiones();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener regiones");
            return new List<Region>();
        }
    }

    private List<CategoriaPrenda> GetCategoriasFallback()
    {
        return new List<CategoriaPrenda>
        {
            new() { CategoriaId = 1, NombreCategoria = "Camisa" },
            new() { CategoriaId = 2, NombreCategoria = "Camiseta" },
            new() { CategoriaId = 3, NombreCategoria = "Pantalon" },
            new() { CategoriaId = 4, NombreCategoria = "Vestido" },
            new() { CategoriaId = 5, NombreCategoria = "Falda" },
            new() { CategoriaId = 6, NombreCategoria = "Chaqueta" },
            new() { CategoriaId = 7, NombreCategoria = "Sueter" },
            new() { CategoriaId = 8, NombreCategoria = "Abrigo" },
            new() { CategoriaId = 9, NombreCategoria = "Calzado" },
            new() { CategoriaId = 10, NombreCategoria = "Accesorio" }
        };
    }

    private List<string> GetTallasFallback()
    {
        return new List<string> { "XS", "S", "M", "L", "XL", "XXL", "36", "37", "38", "39", "40", "41", "42", "43", "44" };
    }

    private List<EstadoPrenda> GetEstadosFallback()
    {
        return new List<EstadoPrenda>
        {
            new() { EstadoId = 1, NombreEstado = "Nuevo" },
            new() { EstadoId = 2, NombreEstado = "Como nuevo" },
            new() { EstadoId = 3, NombreEstado = "Muy bueno" },
            new() { EstadoId = 4, NombreEstado = "Bueno" },
            new() { EstadoId = 5, NombreEstado = "Aceptable" }
        };
    }
}
