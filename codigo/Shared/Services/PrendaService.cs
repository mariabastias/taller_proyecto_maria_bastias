using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Services;

public class PrendaService
{
    private readonly IPrendaSharedRepository _prendaRepository;

    public PrendaService(IPrendaSharedRepository prendaRepository)
    {
        _prendaRepository = prendaRepository;
    }

    public async Task<List<Prenda>> GetPrendasAsync()
    {
        return await _prendaRepository.GetPrendasAsync();
    }

    public async Task<List<Prenda>> GetPrendasDisponiblesAsync()
    {
        return await _prendaRepository.GetPrendasDisponiblesAsync();
    }

    public async Task<Prenda?> GetPrendaByIdAsync(int id)
    {
        return await _prendaRepository.GetPrendaByIdAsync(id);
    }

    public async Task<List<Prenda>> GetPrendasByUsuarioIdAsync(int usuarioId)
    {
        return await _prendaRepository.GetPrendasByUsuarioIdAsync(usuarioId);
    }

    public async Task<Prenda> CreatePrendaAsync(Prenda prenda)
    {
        var prendaId = await _prendaRepository.CreatePrendaAsync(prenda);
        prenda.Id = prendaId;
        return prenda;
    }

    public async Task<bool> UpdatePrendaAsync(Prenda prenda)
    {
        return await _prendaRepository.UpdatePrendaAsync(prenda);
    }

    public async Task<bool> DeletePrendaAsync(int id)
    {
        return await _prendaRepository.DeletePrendaAsync(id);
    }

    public async Task<bool> CambiarDisponibilidadAsync(int id, bool disponible)
    {
        return await _prendaRepository.CambiarDisponibilidadAsync(id, disponible);
    }

    public async Task<bool> IncrementarVistasAsync(int id)
    {
        return await _prendaRepository.IncrementarVistasAsync(id);
    }
}
