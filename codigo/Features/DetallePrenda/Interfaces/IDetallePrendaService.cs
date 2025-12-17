using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePrenda.Interfaces;

public interface IDetallePrendaService
{
    Task<Prenda?> ObtenerDetallePrenda(int prendaId);
    Task<List<string>> ObtenerImagenesPrenda(int prendaId);
    Task<Usuario?> ObtenerPropietario(int prendaId);
    Task RegistrarVista(int prendaId);
    Task<List<Prenda>> ObtenerPrendasSimilares(int prendaId);
}
