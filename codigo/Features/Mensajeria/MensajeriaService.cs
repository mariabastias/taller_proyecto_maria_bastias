using TruequeTextil.Features.Mensajeria.Interfaces;
using TruequeTextil.Features.Notificaciones.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;
using Microsoft.AspNetCore.SignalR;

namespace TruequeTextil.Features.Mensajeria;

public class MensajeriaService : IMensajeriaService
{
    private readonly IMensajeriaRepository _repository;
    private readonly INotificacionesRepository _notificacionesRepository;
    private readonly ILogger<MensajeriaService> _logger;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly IHubContext<ChatHub> _chatHub;

    public MensajeriaService(
        IMensajeriaRepository repository,
        INotificacionesRepository notificacionesRepository,
        ILogger<MensajeriaService> logger,
        CustomAuthenticationStateProvider authenticationStateProvider,
        IHubContext<ChatHub> chatHub)
    {
        _repository = repository;
        _notificacionesRepository = notificacionesRepository;
        _logger = logger;
        _authenticationStateProvider = authenticationStateProvider;
        _chatHub = chatHub;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId, int usuarioId)
    {
        try
        {
            // Verificar que el usuario puede ver los mensajes
            var puedeVer = await _repository.PuedeEnviarMensaje(propuestaId, usuarioId);
            if (!puedeVer)
            {
                _logger.LogWarning("Usuario {UsuarioId} intento acceder a mensajes de propuesta {PropuestaId} sin permiso",
                    usuarioId, propuestaId);
                return new List<MensajeNegociacion>();
            }

            // Marcar mensajes como leidos automaticamente
            await _repository.MarcarMensajesComoLeidos(propuestaId, usuarioId);

            return await _repository.ObtenerMensajes(propuestaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes de propuesta {PropuestaId}", propuestaId);
            return new List<MensajeNegociacion>();
        }
    }

    public async Task<(bool Exito, string Mensaje, int? MensajeId)> EnviarMensaje(int propuestaId, int usuarioId, string contenido)
    {
        try
        {
            // Validar contenido
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return (false, "El mensaje no puede estar vacio", null);
            }

            if (contenido.Length > 1000)
            {
                return (false, "El mensaje excede el limite de 1000 caracteres", null);
            }

            // Verificar permisos
            var puedeEnviar = await _repository.PuedeEnviarMensaje(propuestaId, usuarioId);
            if (!puedeEnviar)
            {
                return (false, "No puedes enviar mensajes a esta propuesta", null);
            }

            // Enviar mensaje
            var mensajeId = await _repository.EnviarMensaje(propuestaId, usuarioId, contenido.Trim());

            if (mensajeId > 0)
            {
                _logger.LogInformation("Mensaje {MensajeId} enviado por usuario {UsuarioId} en propuesta {PropuestaId}",
                    mensajeId, usuarioId, propuestaId);

                // Enviar via SignalR
                var mensaje = await _repository.ObtenerMensajePorId(mensajeId);
                if (mensaje != null)
                {
                    await _chatHub.Clients.Group($"chat-{propuestaId}").SendAsync("ReceiveMessage", mensaje);
                }

                // Crear notificacion para el otro usuario
                await CrearNotificacionMensaje(propuestaId, usuarioId);

                return (true, "Mensaje enviado", mensajeId);
            }

            return (false, "Error al enviar el mensaje", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje en propuesta {PropuestaId}", propuestaId);
            return (false, "Error al enviar el mensaje", null);
        }
    }

    private async Task CrearNotificacionMensaje(int propuestaId, int usuarioEmisorId)
    {
        try
        {
            // Obtener el otro usuario de la propuesta para notificarlo
            var conversaciones = await _repository.ObtenerConversaciones(usuarioEmisorId);
            var conversacion = conversaciones.FirstOrDefault(c => c.PropuestaId == propuestaId);

            if (conversacion != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = conversacion.OtroUsuarioId,
                    Tipo = TipoNotificacion.MensajeNuevo,  // Tipo 6: Mensaje nuevo
                    Titulo = "Nuevo mensaje",
                    Mensaje = $"Tienes un nuevo mensaje en la negociación",
                    ReferenciaId = propuestaId,
                    ReferenciaUrl = $"/mensajes/chat/{propuestaId}"
                };

                await _notificacionesRepository.CrearNotificacion(notificacion);
            }
        }
        catch (Exception ex)
        {
            // No fallar el envio del mensaje si la notificacion falla
            _logger.LogWarning(ex, "Error al crear notificacion de mensaje para propuesta {PropuestaId}", propuestaId);
        }
    }

    public async Task<bool> MarcarComoLeidos(int propuestaId, int usuarioId)
    {
        try
        {
            return await _repository.MarcarMensajesComoLeidos(propuestaId, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar mensajes como leidos en propuesta {PropuestaId}", propuestaId);
            return false;
        }
    }

    public async Task<int> ContarNoLeidos(int propuestaId, int usuarioId)
    {
        try
        {
            return await _repository.ContarMensajesNoLeidos(propuestaId, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar mensajes no leidos en propuesta {PropuestaId}", propuestaId);
            return 0;
        }
    }

    public async Task<int> ContarTotalNoLeidos(int usuarioId)
    {
        try
        {
            return await _repository.ContarTotalMensajesNoLeidos(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar total mensajes no leidos para usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }

    public async Task<List<ConversacionResumen>> ObtenerConversaciones(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerConversaciones(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conversaciones para usuario {UsuarioId}", usuarioId);
            return new List<ConversacionResumen>();
        }
    }

    public async Task<bool> PuedeEnviarMensaje(int propuestaId, int usuarioId)
    {
        try
        {
            return await _repository.PuedeEnviarMensaje(propuestaId, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permisos de mensaje para propuesta {PropuestaId}", propuestaId);
            return false;
        }
    }

    // Métodos adicionales para funcionalidad de Chat
    public async Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId, int usuarioId)
    {
        try
        {
            // Obtener propuesta directamente por ID (no depende de mensajes existentes)
            var conversacion = await _repository.ObtenerPropuestaPorId(propuestaId, usuarioId);

            if (conversacion == null) return null;

            return new PropuestaTrueque
            {
                PropuestaId = propuestaId,
                EstadoPropuestaId = GetEstadoId(conversacion.EstadoPropuesta),
                PrendaOfrecida = new Prenda
                {
                    PrendaId = conversacion.PrendaOfrecidaId,
                    Titulo = conversacion.TituloPrendaOfrecida,
                    UrlImagenPrincipal = conversacion.ImagenPrendaOfrecida,
                    Tipo = conversacion.TipoPrendaOfrecida
                },
                PrendaSolicitada = new Prenda
                {
                    PrendaId = conversacion.PrendaSolicitadaId,
                    Titulo = conversacion.TituloPrendaSolicitada,
                    UrlImagenPrincipal = conversacion.ImagenPrendaSolicitada,
                    Tipo = conversacion.TipoPrendaSolicitada
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propuesta {PropuestaId}", propuestaId);
            return null;
        }
    }

    private int GetEstadoId(string estado)
    {
        return estado.ToLower() switch
        {
            "pendiente" => 1,
            "aceptada" => 2,
            "rechazada" => 3,
            "completada" => 6,
            "cancelada" => 7,
            _ => 1
        };
    }

    public async Task AddSystemMessage(int propuestaId, string message)
    {
        // Crear mensaje del sistema (usuarioId = 0 para sistema)
        await EnviarMensaje(propuestaId, 0, $"[Sistema] {message}");
    }
}
