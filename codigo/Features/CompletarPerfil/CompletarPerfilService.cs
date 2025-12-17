using TruequeTextil.Features.CompletarPerfil.Interfaces;
using TruequeTextil.Shared.Services;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.CompletarPerfil;

public class CompletarPerfilService : ICompletarPerfilService
{
    private readonly ICompletarPerfilRepository _repository;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<CompletarPerfilService> _logger;

    public CompletarPerfilService(
        ICompletarPerfilRepository repository,
        CustomAuthenticationStateProvider authenticationStateProvider,
        ILogger<CompletarPerfilService> logger)
    {
        _repository = repository;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<bool> PerfilCompleto(int usuarioId)
    {
        return await _repository.PerfilCompleto(usuarioId);
    }

    public async Task<bool> CompletarPerfil(int usuarioId, string urlFotoPerfil, string biografia)
    {
        // Validar campos obligatorios segun RF-03
        if (string.IsNullOrWhiteSpace(urlFotoPerfil))
        {
            _logger.LogWarning("Intento de completar perfil sin foto para usuario {UsuarioId}", usuarioId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(biografia) || biografia.Length < 10)
        {
            _logger.LogWarning("Intento de completar perfil con biografia insuficiente para usuario {UsuarioId}", usuarioId);
            return false;
        }

        try
        {
            await _repository.ActualizarPerfil(usuarioId, urlFotoPerfil, biografia);
            _logger.LogInformation("Perfil completado exitosamente para usuario {UsuarioId}", usuarioId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar perfil del usuario {UsuarioId}", usuarioId);
            return false;
        }
    }

    public async Task<int> ObtenerProgresoPerfil(int usuarioId)
    {
        try
        {
            var usuario = await _repository.ObtenerUsuarioPorId(usuarioId);
            if (usuario == null) return 0;

            int progreso = 0;
            int totalCampos = 2; // Foto y biografia son los obligatorios

            if (!string.IsNullOrWhiteSpace(usuario.UrlFotoPerfil)) progreso++;
            if (!string.IsNullOrWhiteSpace(usuario.Biografia) && usuario.Biografia.Length >= 10) progreso++;

            return (progreso * 100) / totalCampos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular progreso de perfil del usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }
}
