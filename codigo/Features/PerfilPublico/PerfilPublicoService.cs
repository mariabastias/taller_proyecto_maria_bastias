using TruequeTextil.Features.PerfilPublico.Interfaces;
using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.PerfilPublico;

public class PerfilPublicoService : IPerfilPublicoService
{
    private readonly IPerfilPublicoRepository _repository;
    private readonly ILogger<PerfilPublicoService> _logger;

    public PerfilPublicoService(
        IPerfilPublicoRepository repository,
        ILogger<PerfilPublicoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Usuario?> ObtenerPerfilPublico(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerPerfilPublico(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil publico del usuario {UsuarioId}", usuarioId);
            return null;
        }
    }

    public async Task<List<EvaluacionModel>> ObtenerValoraciones(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerValoraciones(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valoraciones del usuario {UsuarioId}", usuarioId);
            return new List<EvaluacionModel>();
        }
    }

    public async Task<List<Prenda>> ObtenerPrendasUsuario(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerPrendasUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas del usuario {UsuarioId}", usuarioId);
            return new List<Prenda>();
        }
    }
}
