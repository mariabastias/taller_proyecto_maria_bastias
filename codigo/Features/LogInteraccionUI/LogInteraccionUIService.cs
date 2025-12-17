using TruequeTextil.Features.LogInteraccionUI.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.LogInteraccionUI;

public class LogInteraccionUIService : ILogInteraccionUIService
{
    private readonly ILogInteraccionUIRepository _repository;
    private readonly ILogger<LogInteraccionUIService> _logger;

    public LogInteraccionUIService(ILogInteraccionUIRepository repository, ILogger<LogInteraccionUIService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> RegistrarInteraccion(LogInteraccionUIModel interaccion)
    {
        if (interaccion == null)
            return (false, "La interacción no puede ser nula");

        if (string.IsNullOrEmpty(interaccion.ElementoUi))
            return (false, "El elemento UI no puede estar vacío");

        if (string.IsNullOrEmpty(interaccion.Accion))
            return (false, "La acción no puede estar vacía");

        try
        {
            var success = await _repository.RegistrarInteraccion(interaccion);
            if (success)
            {
                _logger.LogInformation("Interacción registrada: {ElementoUi} - {Accion} - Usuario: {UsuarioId}",
                    interaccion.ElementoUi, interaccion.Accion, interaccion.UsuarioId);
                return (true, null);
            }
            else
            {
                return (false, "Error al registrar la interacción");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar interacción: {ElementoUi} - {Accion}",
                interaccion.ElementoUi, interaccion.Accion);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<List<LogInteraccionUIModel>> ObtenerInteraccionesPorUsuario(int? usuarioId, int limite = 100)
    {
        if (limite <= 0 || limite > 1000)
            limite = 100;

        try
        {
            return await _repository.ObtenerInteraccionesPorUsuario(usuarioId, limite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener interacciones por usuario {UsuarioId}", usuarioId);
            return new List<LogInteraccionUIModel>();
        }
    }

    public async Task<List<LogInteraccionUIModel>> ObtenerInteraccionesPorElemento(string elementoUi, int limite = 100)
    {
        if (string.IsNullOrEmpty(elementoUi))
            return new List<LogInteraccionUIModel>();

        if (limite <= 0 || limite > 1000)
            limite = 100;

        try
        {
            return await _repository.ObtenerInteraccionesPorElemento(elementoUi, limite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener interacciones por elemento {ElementoUi}", elementoUi);
            return new List<LogInteraccionUIModel>();
        }
    }

    public async Task<List<LogInteraccionUIModel>> ObtenerInteraccionesRecientes(int limite = 50)
    {
        if (limite <= 0 || limite > 1000)
            limite = 50;

        try
        {
            return await _repository.ObtenerInteraccionesRecientes(limite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener interacciones recientes");
            return new List<LogInteraccionUIModel>();
        }
    }

    public async Task<int> ContarInteraccionesPorUsuario(int? usuarioId, DateTime? desde = null, DateTime? hasta = null)
    {
        try
        {
            return await _repository.ContarInteraccionesPorUsuario(usuarioId, desde, hasta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar interacciones por usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }
}
