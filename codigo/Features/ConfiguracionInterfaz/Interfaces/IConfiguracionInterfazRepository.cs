using TruequeTextil.Shared.Models;

using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionInterfaz.Interfaces;

public interface IConfiguracionInterfazRepository
{
    Task<ConfiguracionInterfazModel?> ObtenerConfiguracionPorUsuario(int usuarioId);
    Task<bool> CrearConfiguracion(ConfiguracionInterfazModel configuracion);
    Task<bool> ActualizarConfiguracion(ConfiguracionInterfazModel configuracion);
    Task<bool> EliminarConfiguracion(int usuarioId);
}
