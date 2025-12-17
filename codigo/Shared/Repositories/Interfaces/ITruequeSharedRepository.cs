using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Repositories.Interfaces;

public interface ITruequeSharedRepository
{
    Task<List<PropuestaTrueque>> GetPropuestasAsync();
    Task<PropuestaTrueque?> GetPropuestaByIdAsync(int id);
    Task<List<PropuestaTrueque>> GetPropuestasByUsuarioProponenteIdAsync(int usuarioId);
    Task<List<PropuestaTrueque>> GetPropuestasByUsuarioReceptorIdAsync(int usuarioId);
    Task<List<PropuestaTrueque>> GetPropuestasByPrendaIdAsync(int prendaId);
    Task<int> CreatePropuestaAsync(PropuestaTrueque propuesta);
    Task<bool> AceptarPropuestaAsync(int id, string? mensaje = null);
    Task<bool> RechazarPropuestaAsync(int id, string motivo);
    Task<bool> CompletarPropuestaAsync(int id);
    Task<bool> CancelarPropuestaAsync(int id, string motivo);
}
