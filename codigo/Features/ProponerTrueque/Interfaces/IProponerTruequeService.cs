using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ProponerTrueque.Interfaces;

public interface IProponerTruequeService
{
    // RF-09: Crear propuesta con limite de 3 activas por prenda
    Task<(bool Exito, string Mensaje, int PropuestaId)> CrearPropuesta(
        int usuarioProponenteId, int prendaOfrecidaId, int prendaSolicitadaId, string? mensaje = null);

    Task<List<Prenda>> ObtenerMisPrendasDisponibles(int usuarioId);
    Task<Prenda?> ObtenerPrendaDestino(int prendaId);

    // RF-09: Verificar propuestas activas para una prenda
    Task<(int Activas, int Maximo)> ObtenerEstadoPropuestasPrenda(int prendaId);
}
