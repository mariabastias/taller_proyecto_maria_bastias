using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.InicioSesion.Interfaces;

public interface IInicioSesionService
{
    Task<(bool Success, string? Error, Usuario? Usuario, bool RequiresAdminVerification)> IniciarSesion(string email, string password);
    Task Logout();
    Task<bool> IsSessionExpired();
    Task<Usuario?> GetCurrentUser();
    Task<string?> ObtenerPaginaRedireccionPostLogin(int usuarioId);
    Task UpdateActivity();
}
