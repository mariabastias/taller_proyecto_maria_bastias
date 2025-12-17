using TruequeTextil.Features.Onboarding.Interfaces;
using TruequeTextil.Shared.Services;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Onboarding;

public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _repository;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        IOnboardingRepository repository,
        CustomAuthenticationStateProvider authenticationStateProvider,
        ILogger<OnboardingService> logger)
    {
        _repository = repository;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<bool> OnboardingCompletado(int usuarioId)
    {
        return await _repository.OnboardingCompletado(usuarioId);
    }

    public async Task<bool> PerfilCompleto(int usuarioId)
    {
        return await _repository.PerfilCompleto(usuarioId);
    }

    public async Task MarcarOnboardingCompletado(int usuarioId)
    {
        await _repository.MarcarOnboardingCompletado(usuarioId);
        _logger.LogInformation("Onboarding completado para usuario ID: {UsuarioId}", usuarioId);
    }
}
