using TruequeTextil.Shared.Models;

using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionModulo.Interfaces;

public interface IConfiguracionModuloRepository
{
    Task<ConfiguracionModuloModel?> ObtenerConfiguracionPorUsuarioYModulo(int usuarioId, string modulo);
    Task<List<ConfiguracionModuloModel>> ObtenerConfiguracionesPorUsuario(int usuarioId);
    Task<bool> CrearConfiguracion(ConfiguracionModuloModel configuracion);
    Task<bool> ActualizarConfiguracion(ConfiguracionModuloModel configuracion);
    Task<bool> EliminarConfiguracion(int usuarioId, string modulo);
    Task<bool> EliminarTodasConfiguracionesUsuario(int usuarioId);
}
