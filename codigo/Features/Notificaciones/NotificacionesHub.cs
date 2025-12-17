using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TruequeTextil.Features.Notificaciones;

// RF-10: Hub de SignalR para notificaciones en tiempo real
public class NotificacionesHub : Hub
{
    private readonly ILogger<NotificacionesHub> _logger;

    public NotificacionesHub(ILogger<NotificacionesHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Agregar usuario a su grupo personal para notificaciones dirigidas
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("Usuario {UserId} conectado a notificaciones. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("Usuario {UserId} desconectado de notificaciones", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Metodo para suscribirse a un grupo de conversacion (para mensajeria)
    public async Task JoinConversation(int propuestaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversacion_{propuestaId}");
        _logger.LogInformation("Usuario unido a conversacion {PropuestaId}", propuestaId);
    }

    public async Task LeaveConversation(int propuestaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversacion_{propuestaId}");
        _logger.LogInformation("Usuario salio de conversacion {PropuestaId}", propuestaId);
    }
}

// DTO para notificaciones en tiempo real
public class NotificacionRealTime
{
    public int NotificacionId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int? ReferenciaId { get; set; }
    public DateTime Fecha { get; set; }
}
