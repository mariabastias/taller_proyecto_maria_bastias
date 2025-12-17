namespace TruequeTextil.Features.Onboarding.Interfaces;

public interface IOnboardingRepository
{
    Task<bool> OnboardingCompletado(int usuarioId);
    Task<bool> PerfilCompleto(int usuarioId);
    Task MarcarOnboardingCompletado(int usuarioId);
}
