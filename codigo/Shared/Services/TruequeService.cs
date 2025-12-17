using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Services;

public class TruequeService
{
    private readonly ITruequeSharedRepository _truequeRepository;
    private readonly IUsuarioSharedRepository _usuarioRepository;

    public TruequeService(ITruequeSharedRepository truequeRepository, IUsuarioSharedRepository usuarioRepository)
    {
        _truequeRepository = truequeRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasAsync()
    {
        return await _truequeRepository.GetPropuestasAsync();
    }

    public async Task<PropuestaTrueque?> GetPropuestaByIdAsync(int id)
    {
        return await _truequeRepository.GetPropuestaByIdAsync(id);
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasByUsuarioProponenteIdAsync(int usuarioId)
    {
        return await _truequeRepository.GetPropuestasByUsuarioProponenteIdAsync(usuarioId);
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasByUsuarioReceptorIdAsync(int usuarioId)
    {
        return await _truequeRepository.GetPropuestasByUsuarioReceptorIdAsync(usuarioId);
    }

    public async Task<List<PropuestaTrueque>> GetPropuestasByPrendaIdAsync(int prendaId)
    {
        return await _truequeRepository.GetPropuestasByPrendaIdAsync(prendaId);
    }

    public async Task<PropuestaTrueque> CreatePropuestaAsync(PropuestaTrueque propuesta)
    {
        var propuestaId = await _truequeRepository.CreatePropuestaAsync(propuesta);
        propuesta.Id = propuestaId;
        return propuesta;
    }

    public async Task<bool> AceptarPropuestaAsync(int id, string? mensaje = null)
    {
        return await _truequeRepository.AceptarPropuestaAsync(id, mensaje);
    }

    public async Task<bool> RechazarPropuestaAsync(int id, string motivo)
    {
        return await _truequeRepository.RechazarPropuestaAsync(id, motivo);
    }

    public async Task<bool> CompletarPropuestaAsync(int id, UsuarioService usuarioService)
    {
        var success = await _truequeRepository.CompletarPropuestaAsync(id);

        if (success)
        {
            // Get the propuesta to increment trueques for both users
            var propuesta = await _truequeRepository.GetPropuestaByIdAsync(id);
            if (propuesta != null)
            {
                await usuarioService.IncrementarTruequesConcretadosAsync(propuesta.UsuarioProponenteId);
                await usuarioService.IncrementarTruequesConcretadosAsync(propuesta.UsuarioReceptorId);
            }
        }

        return success;
    }

    public async Task<bool> CancelarPropuestaAsync(int id, string motivo)
    {
        return await _truequeRepository.CancelarPropuestaAsync(id, motivo);
    }
}
