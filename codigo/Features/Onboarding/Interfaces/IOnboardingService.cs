using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Onboarding.Interfaces;

public interface IOnboardingService
{
    Task<Usuario?> GetCurrentUser();
    Task<bool> OnboardingCompletado(int usuarioId);
    Task<bool> PerfilCompleto(int usuarioId);
    Task MarcarOnboardingCompletado(int usuarioId);
}
