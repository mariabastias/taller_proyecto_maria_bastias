using Microsoft.AspNetCore.SignalR;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Mensajeria;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinChat(int propuestaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{propuestaId}");
        _logger.LogInformation("User {ConnectionId} joined chat {PropuestaId}", Context.ConnectionId, propuestaId);
    }

    public async Task LeaveChat(int propuestaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{propuestaId}");
        _logger.LogInformation("User {ConnectionId} left chat {PropuestaId}", Context.ConnectionId, propuestaId);
    }

    public async Task SendMessage(int propuestaId, MensajeNegociacion mensaje)
    {
        await Clients.Group($"chat-{propuestaId}").SendAsync("ReceiveMessage", mensaje);
        _logger.LogInformation("Message sent in chat {PropuestaId} by user {UsuarioId}", propuestaId, mensaje.UsuarioId);
    }

    public async Task TypingIndicator(int propuestaId, int usuarioId, bool isTyping)
    {
        await Clients.GroupExcept($"chat-{propuestaId}", Context.ConnectionId).SendAsync("UserTyping", usuarioId, isTyping);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}