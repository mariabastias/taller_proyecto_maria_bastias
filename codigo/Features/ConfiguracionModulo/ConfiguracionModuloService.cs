using TruequeTextil.Features.ConfiguracionModulo.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionModulo;

public class ConfiguracionModuloService : IConfiguracionModuloService
{
    private readonly IConfiguracionModuloRepository _repository;
    private readonly ILogger<ConfiguracionModuloService> _logger;

    public ConfiguracionModuloService(IConfiguracionModuloRepository repository, ILogger<ConfiguracionModuloService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConfiguracionModuloModel?> ObtenerConfiguracionPorUsuarioYModulo(int usuarioId, string modulo)
    {
        if (usuarioId <= 0 || string.IsNullOrEmpty(modulo))
            return null;

        try
        {
            return await _repository.ObtenerConfiguracionPorUsuarioYModulo(usuarioId, modulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración de módulo {Modulo} para usuario {UsuarioId}", modulo, usuarioId);
            return null;
        }
    }

    public async Task<List<ConfiguracionModuloModel>> ObtenerConfiguracionesPorUsuario(int usuarioId)
    {
        if (usuarioId <= 0)
            return new List<ConfiguracionModuloModel>();

        try
        {
            return await _repository.ObtenerConfiguracionesPorUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuraciones de módulos para usuario {UsuarioId}", usuarioId);
            return new List<ConfiguracionModuloModel>();
        }
    }

    public async Task<(bool Success, string? Error)> GuardarConfiguracion(ConfiguracionModuloModel configuracion)
    {
        if (configuracion == null)
            return (false, "La configuración no puede ser nula");

        if (configuracion.UsuarioId <= 0)
            return (false, "ID de usuario inválido");

        if (string.IsNullOrEmpty(configuracion.Modulo))
            return (false, "El módulo no puede estar vacío");

        try
        {
            // Verificar si ya existe configuración
            var existente = await _repository.ObtenerConfiguracionPorUsuarioYModulo(configuracion.UsuarioId, configuracion.Modulo);
            bool success;

            if (existente == null)
            {
                // Crear nueva configuración
                success = await _repository.CrearConfiguracion(configuracion);
                if (success)
                {
                    _logger.LogInformation("Configuración de módulo {Modulo} creada para usuario {UsuarioId}", configuracion.Modulo, configuracion.UsuarioId);
                }
            }
            else
            {
                // Actualizar configuración existente
                success = await _repository.ActualizarConfiguracion(configuracion);
                if (success)
                {
                    _logger.LogInformation("Configuración de módulo {Modulo} actualizada para usuario {UsuarioId}", configuracion.Modulo, configuracion.UsuarioId);
                }
            }

            return success ? (true, null) : (false, "Error al guardar la configuración");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración de módulo {Modulo} para usuario {UsuarioId}", configuracion.Modulo, configuracion.UsuarioId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> EliminarConfiguracion(int usuarioId, string modulo)
    {
        if (usuarioId <= 0)
            return (false, "ID de usuario inválido");

        if (string.IsNullOrEmpty(modulo))
            return (false, "El módulo no puede estar vacío");

        try
        {
            var success = await _repository.EliminarConfiguracion(usuarioId, modulo);
            if (success)
            {
                _logger.LogInformation("Configuración de módulo {Modulo} eliminada para usuario {UsuarioId}", modulo, usuarioId);
                return (true, null);
            }
            else
            {
                return (false, "No se encontró configuración para eliminar");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar configuración de módulo {Modulo} para usuario {UsuarioId}", modulo, usuarioId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> EliminarTodasConfiguracionesUsuario(int usuarioId)
    {
        if (usuarioId <= 0)
            return (false, "ID de usuario inválido");

        try
        {
            var success = await _repository.EliminarTodasConfiguracionesUsuario(usuarioId);
            if (success)
            {
                _logger.LogInformation("Todas las configuraciones de módulos eliminadas para usuario {UsuarioId}", usuarioId);
                return (true, null);
            }
            else
            {
                return (false, "No se encontraron configuraciones para eliminar");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar todas las configuraciones de módulos para usuario {UsuarioId}", usuarioId);
            return (false, "Error interno del servidor");
        }
    }
}
