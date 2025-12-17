using TruequeTextil.Features.ConfiguracionInterfaz.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionInterfaz;

public class ConfiguracionInterfazService : IConfiguracionInterfazService
{
    private readonly IConfiguracionInterfazRepository _repository;
    private readonly ILogger<ConfiguracionInterfazService> _logger;

    public ConfiguracionInterfazService(IConfiguracionInterfazRepository repository, ILogger<ConfiguracionInterfazService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConfiguracionInterfazModel?> ObtenerConfiguracionPorUsuario(int usuarioId)
    {
        if (usuarioId <= 0)
            return null;

        try
        {
            return await _repository.ObtenerConfiguracionPorUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración de interfaz para usuario {UsuarioId}", usuarioId);
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> CrearConfiguracion(ConfiguracionInterfazModel configuracion)
    {
        if (configuracion == null)
            return (false, "La configuración no puede ser nula");

        if (configuracion.UsuarioId <= 0)
            return (false, "ID de usuario inválido");

        // Verificar si ya existe configuración para este usuario
        var existente = await _repository.ObtenerConfiguracionPorUsuario(configuracion.UsuarioId);
        if (existente != null)
            return (false, "Ya existe una configuración para este usuario");

        try
        {
            var success = await _repository.CrearConfiguracion(configuracion);
            if (success)
            {
                _logger.LogInformation("Configuración de interfaz creada para usuario {UsuarioId}", configuracion.UsuarioId);
                return (true, null);
            }
            else
            {
                return (false, "Error al crear la configuración");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear configuración de interfaz para usuario {UsuarioId}", configuracion.UsuarioId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> ActualizarConfiguracion(ConfiguracionInterfazModel configuracion)
    {
        if (configuracion == null)
            return (false, "La configuración no puede ser nula");

        if (configuracion.UsuarioId <= 0)
            return (false, "ID de usuario inválido");

        // Verificar si existe configuración para este usuario
        var existente = await _repository.ObtenerConfiguracionPorUsuario(configuracion.UsuarioId);
        if (existente == null)
            return (false, "No existe configuración para este usuario");

        try
        {
            var success = await _repository.ActualizarConfiguracion(configuracion);
            if (success)
            {
                _logger.LogInformation("Configuración de interfaz actualizada para usuario {UsuarioId}", configuracion.UsuarioId);
                return (true, null);
            }
            else
            {
                return (false, "Error al actualizar la configuración");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración de interfaz para usuario {UsuarioId}", configuracion.UsuarioId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> EliminarConfiguracion(int usuarioId)
    {
        if (usuarioId <= 0)
            return (false, "ID de usuario inválido");

        try
        {
            var success = await _repository.EliminarConfiguracion(usuarioId);
            if (success)
            {
                _logger.LogInformation("Configuración de interfaz eliminada para usuario {UsuarioId}", usuarioId);
                return (true, null);
            }
            else
            {
                return (false, "No se encontró configuración para eliminar");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar configuración de interfaz para usuario {UsuarioId}", usuarioId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> GuardarConfiguracion(ConfiguracionInterfazModel configuracion)
    {
        if (configuracion == null)
            return (false, "La configuración no puede ser nula");

        if (configuracion.UsuarioId <= 0)
            return (false, "ID de usuario inválido");

        // Verificar si ya existe configuración
        var existente = await _repository.ObtenerConfiguracionPorUsuario(configuracion.UsuarioId);
        if (existente == null)
        {
            // Crear nueva configuración
            return await CrearConfiguracion(configuracion);
        }
        else
        {
            // Actualizar configuración existente
            return await ActualizarConfiguracion(configuracion);
        }
    }
}
