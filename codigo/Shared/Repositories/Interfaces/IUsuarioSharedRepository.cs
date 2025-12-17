using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Repositories.Interfaces;

public interface IUsuarioSharedRepository
{
    Task<Usuario?> ObtenerUsuarioPorId(int usuarioId);
}
