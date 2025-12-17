using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.PerfilPublico.Interfaces;

public interface IPerfilPublicoRepository
{
    Task<Usuario?> ObtenerPerfilPublico(int usuarioId);
    Task<List<EvaluacionModel>> ObtenerValoraciones(int usuarioId);
    Task<List<Prenda>> ObtenerPrendasUsuario(int usuarioId);
}
