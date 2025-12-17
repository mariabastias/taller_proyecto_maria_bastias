using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Explorar.Interfaces;

public interface IExplorarRepository
{
    // RF-07: Busqueda paginada con filtros (RNF-01: 20 items/pagina)
    Task<(List<Prenda> Prendas, int TotalCount)> ObtenerPrendasDisponibles(
        string? busqueda = null,
        int? categoriaId = null,
        string? talla = null,
        int? estadoPrendaId = null,
        int? regionId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pagina = 1,
        int itemsPorPagina = 20);

    Task<List<CategoriaPrenda>> ObtenerCategorias();
    Task<List<string>> ObtenerTallas();
    Task<List<EstadoPrenda>> ObtenerEstadosPrenda();
    Task<List<Region>> ObtenerRegiones();
}
