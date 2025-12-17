using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Repositories.Interfaces;

public interface IPrendaSharedRepository
{
    Task<List<Prenda>> GetPrendasAsync();
    Task<List<Prenda>> GetPrendasDisponiblesAsync();
    Task<Prenda?> GetPrendaByIdAsync(int id);
    Task<List<Prenda>> GetPrendasByUsuarioIdAsync(int usuarioId);
    Task<int> CreatePrendaAsync(Prenda prenda);
    Task<bool> UpdatePrendaAsync(Prenda prenda);
    Task<bool> DeletePrendaAsync(int id);
    Task<bool> CambiarDisponibilidadAsync(int id, bool disponible);
    Task<bool> IncrementarVistasAsync(int id);
}
