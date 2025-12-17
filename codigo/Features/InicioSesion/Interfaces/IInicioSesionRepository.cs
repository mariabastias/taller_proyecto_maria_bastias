using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.InicioSesion.Interfaces;

public interface IInicioSesionRepository
{
    Task<Usuario?> ObtenerUsuarioPorEmail(string email);
    Task<string?> ObtenerEstadoUsuario(int usuarioId);
    Task ActualizarUltimoLogin(int usuarioId);
    Task<bool> OnboardingCompletado(int usuarioId);
    Task<bool> PerfilCompleto(int usuarioId);
}
