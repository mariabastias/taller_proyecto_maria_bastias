using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.CompletarPerfil.Interfaces;

public interface ICompletarPerfilService
{
    Task<Usuario?> GetCurrentUser();
    Task<bool> PerfilCompleto(int usuarioId);
    Task<bool> CompletarPerfil(int usuarioId, string urlFotoPerfil, string biografia);
    Task<int> ObtenerProgresoPerfil(int usuarioId);
}
