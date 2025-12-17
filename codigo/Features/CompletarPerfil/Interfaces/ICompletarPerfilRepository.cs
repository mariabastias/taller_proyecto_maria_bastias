using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.CompletarPerfil.Interfaces;

public interface ICompletarPerfilRepository
{
    Task<bool> PerfilCompleto(int usuarioId);
    Task ActualizarPerfil(int usuarioId, string urlFotoPerfil, string biografia);
    Task<Usuario?> ObtenerUsuarioPorId(int usuarioId);
}
