using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ProponerTrueque.Interfaces;

public interface IProponerTruequeRepository
{
    Task<int> CrearPropuesta(PropuestaTrueque propuesta, int prendaOfrecidaId, int prendaSolicitadaId);
    Task<List<Prenda>> ObtenerPrendasDisponiblesUsuario(int usuarioId);
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<int> ContarPropuestasActivasPorPrenda(int prendaId);
    Task<bool> ExistePropuestaActiva(int prendaOfrecidaId, int prendaSolicitadaId);
}
