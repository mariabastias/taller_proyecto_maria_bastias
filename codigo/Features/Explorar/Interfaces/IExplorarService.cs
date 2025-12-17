using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Explorar.Interfaces;

// DTO para resultado paginado
public record PaginacionResultado<T>(
    List<T> Items,
    int TotalCount,
    int PaginaActual,
    int TotalPaginas,
    bool TienePaginaAnterior,
    bool TienePaginaSiguiente
);

public interface IExplorarService
{
    // RF-07: Busqueda con paginacion (RNF-01: 20 items/pagina, respuesta < 2 seg)
    Task<PaginacionResultado<Prenda>> BuscarPrendas(
        string? busqueda = null,
        int? categoriaId = null,
        string? talla = null,
        int? estadoPrendaId = null,
        int? regionId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pagina = 1);

    Task<List<CategoriaPrenda>> ObtenerCategorias();
    Task<List<string>> ObtenerTallas();
    Task<List<EstadoPrenda>> ObtenerEstadosPrenda();
    Task<List<Region>> ObtenerRegiones();
}
