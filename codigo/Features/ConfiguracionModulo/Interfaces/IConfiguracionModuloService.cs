using TruequeTextil.Shared.Models;

using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionModulo.Interfaces;

public interface IConfiguracionModuloService
{
    Task<ConfiguracionModuloModel?> ObtenerConfiguracionPorUsuarioYModulo(int usuarioId, string modulo);
    Task<List<ConfiguracionModuloModel>> ObtenerConfiguracionesPorUsuario(int usuarioId);
    Task<(bool Success, string? Error)> GuardarConfiguracion(ConfiguracionModuloModel configuracion);
    Task<(bool Success, string? Error)> EliminarConfiguracion(int usuarioId, string modulo);
    Task<(bool Success, string? Error)> EliminarTodasConfiguracionesUsuario(int usuarioId);
}
