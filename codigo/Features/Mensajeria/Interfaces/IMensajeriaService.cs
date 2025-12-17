using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Mensajeria.Interfaces;

public interface IMensajeriaService
{
    Task<Usuario?> GetCurrentUser();
    // Obtener mensajes de una propuesta
    Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId, int usuarioId);

    // Enviar mensaje con validaciones
    Task<(bool Exito, string Mensaje, int? MensajeId)> EnviarMensaje(int propuestaId, int usuarioId, string contenido);

    // Marcar mensajes como leidos
    Task<bool> MarcarComoLeidos(int propuestaId, int usuarioId);

    // Contar mensajes no leidos de propuesta
    Task<int> ContarNoLeidos(int propuestaId, int usuarioId);

    // Contar total no leidos del usuario
    Task<int> ContarTotalNoLeidos(int usuarioId);

    // Obtener conversaciones del usuario
    Task<List<ConversacionResumen>> ObtenerConversaciones(int usuarioId);

    // Verificar si puede chatear
    Task<bool> PuedeEnviarMensaje(int propuestaId, int usuarioId);

    // Métodos adicionales para funcionalidad de Chat
    Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId, int usuarioId);
    Task AddSystemMessage(int propuestaId, string message);
}
