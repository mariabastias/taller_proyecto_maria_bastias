using TruequeTextil.Shared.Models;

using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionInterfaz.Interfaces;

public interface IConfiguracionInterfazService
{
    Task<ConfiguracionInterfazModel?> ObtenerConfiguracionPorUsuario(int usuarioId);
    Task<(bool Success, string? Error)> CrearConfiguracion(ConfiguracionInterfazModel configuracion);
    Task<(bool Success, string? Error)> ActualizarConfiguracion(ConfiguracionInterfazModel configuracion);
    Task<(bool Success, string? Error)> EliminarConfiguracion(int usuarioId);
    Task<(bool Success, string? Error)> GuardarConfiguracion(ConfiguracionInterfazModel configuracion);
}
