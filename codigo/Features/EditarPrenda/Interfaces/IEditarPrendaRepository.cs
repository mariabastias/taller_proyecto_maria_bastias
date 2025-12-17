using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPrenda.Interfaces;

public interface IEditarPrendaRepository
{
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<bool> ActualizarPrenda(Prenda prenda);
    Task<bool> EliminarImagenesPrenda(int prendaId);
    Task<bool> AgregarImagenPrenda(int prendaId, string imageUrl, int orden, bool esPrincipal);
    Task<List<CategoriaPrenda>> ObtenerCategorias();
    Task<List<EstadoPrenda>> ObtenerEstadosPrenda();
}