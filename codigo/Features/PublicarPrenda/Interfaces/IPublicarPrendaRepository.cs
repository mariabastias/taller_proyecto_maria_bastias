using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PublicarPrenda.Interfaces;

public interface IPublicarPrendaRepository
{
    Task<int> CrearPrenda(Prenda prenda);
    Task<bool> AgregarImagenPrenda(int prendaId, string imageUrl, int orden, bool esPrincipal);
    Task<bool> ActualizarImagenPrincipal(int prendaId, string imageUrl);
    Task<List<CategoriaPrenda>> ObtenerCategorias();
    Task<List<EstadoPrenda>> ObtenerEstadosPrenda();
}
