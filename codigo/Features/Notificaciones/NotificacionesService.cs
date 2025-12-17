using Microsoft.AspNetCore.SignalR;
using TruequeTextil.Features.Notificaciones.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Notificaciones;

public class NotificacionesService : INotificacionesService
{
    private readonly INotificacionesRepository _repository;
    private readonly IHubContext<NotificacionesHub> _hubContext;
    private readonly ILogger<NotificacionesService> _logger;

    public NotificacionesService(
        INotificacionesRepository repository,
        IHubContext<NotificacionesHub> hubContext,
        ILogger<NotificacionesService> logger)
    {
        _repository = repository;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<List<Notificacion>> ObtenerNotificaciones(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerNotificaciones(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones del usuario {UsuarioId}", usuarioId);
            return new List<Notificacion>();
        }
    }

    public async Task<int> ContarNotificacionesNoLeidas(int usuarioId)
    {
        try
        {
            return await _repository.ContarNotificacionesNoLeidas(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar notificaciones no leídas del usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }

    public async Task MarcarComoLeida(int notificacionId)
    {
        try
        {
            await _repository.MarcarComoLeida(notificacionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación {NotificacionId} como leída", notificacionId);
        }
    }

    public async Task MarcarTodasComoLeidas(int usuarioId)
    {
        try
        {
            await _repository.MarcarTodasComoLeidas(usuarioId);
            _logger.LogInformation("Todas las notificaciones marcadas como leídas para usuario {UsuarioId}", usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas del usuario {UsuarioId}", usuarioId);
        }
    }

    public async Task EnviarNotificacion(int usuarioId, string titulo, string mensaje, string tipo, int? referenciaId = null)
    {
        try
        {
            // Mapeo de strings a tipos válidos en BD
            // Tipos en tabla TipoNotificacion: 1-8
            var tipoNotificacion = tipo.ToLower() switch
            {
                "propuesta" or "nueva_propuesta" or "nuevapropuesta" => TipoNotificacion.NuevaPropuesta,
                "aceptacion" or "propuesta_aceptada" or "propuestaaceptada" or "aceptada" => TipoNotificacion.PropuestaAceptada,
                "seguidor" or "nuevo_seguidor" or "nuevoseguidor" => TipoNotificacion.NuevoSeguidor,
                "busqueda" or "busqueda_encontrada" or "busquedaencontrada" => TipoNotificacion.BusquedaEncontrada,
                "rechazo" or "rechazada" or "propuesta_rechazada" or "propuestarechazada" => TipoNotificacion.PropuestaRechazada,
                "mensaje" or "mensaje_nuevo" or "mensajenuevo" or "chat" => TipoNotificacion.MensajeNuevo,
                "evaluacion" or "evaluacion_recibida" or "evaluacionrecibida" or "review" => TipoNotificacion.EvaluacionRecibida,
                "sistema" or "system" or "info" => TipoNotificacion.Sistema,
                _ => TipoNotificacion.Sistema  // Fallback a Sistema para tipos desconocidos
            };

            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                Tipo = tipoNotificacion,
                Titulo = titulo,
                Mensaje = mensaje,
                ReferenciaId = referenciaId,
                Fecha = DateTime.Now
            };

            var notificacionId = await _repository.CrearNotificacion(notificacion);

            // RF-10: Enviar notificacion en tiempo real via SignalR
            var notificacionRealTime = new NotificacionRealTime
            {
                NotificacionId = notificacionId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                ReferenciaId = referenciaId,
                Fecha = DateTime.Now
            };

            await _hubContext.Clients.Group($"user_{usuarioId}")
                .SendAsync("RecibirNotificacion", notificacionRealTime);

            _logger.LogInformation("Notificacion enviada a usuario {UsuarioId}: {Titulo} (tiempo real)", usuarioId, titulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificacion a usuario {UsuarioId}", usuarioId);
        }
    }

    // RF-10: Enviar actualizacion de contador de notificaciones
    public async Task ActualizarContadorNotificaciones(int usuarioId)
    {
        try
        {
            var count = await _repository.ContarNotificacionesNoLeidas(usuarioId);
            await _hubContext.Clients.Group($"user_{usuarioId}")
                .SendAsync("ActualizarContador", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contador de notificaciones para usuario {UsuarioId}", usuarioId);
        }
    }
}
