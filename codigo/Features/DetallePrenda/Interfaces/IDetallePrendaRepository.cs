using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePrenda.Interfaces;

public interface IDetallePrendaRepository
{
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<List<string>> ObtenerImagenesPrenda(int prendaId);
    Task<Usuario?> ObtenerPropietario(int prendaId);
    Task IncrementarVistas(int prendaId);
    Task<List<Prenda>> ObtenerPrendasSimilares(int prendaId, string tipo, int limite = 4);
}
