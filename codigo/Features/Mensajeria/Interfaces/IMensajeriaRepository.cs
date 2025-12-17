using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Mensajeria.Interfaces;

public interface IMensajeriaRepository
{
    // Obtener mensajes de una propuesta
    Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId);

    // Enviar mensaje
    Task<int> EnviarMensaje(int propuestaId, int usuarioId, string mensaje);

    // Marcar mensajes como leidos
    Task<bool> MarcarMensajesComoLeidos(int propuestaId, int usuarioId);

    // Contar mensajes no leidos de una propuesta
    Task<int> ContarMensajesNoLeidos(int propuestaId, int usuarioId);

    // Contar total de mensajes no leidos del usuario (todas las propuestas)
    Task<int> ContarTotalMensajesNoLeidos(int usuarioId);

    // Obtener conversaciones del usuario (propuestas con mensajes)
    Task<List<ConversacionResumen>> ObtenerConversaciones(int usuarioId);

    // Verificar si usuario puede enviar mensajes a propuesta
    Task<bool> PuedeEnviarMensaje(int propuestaId, int usuarioId);

    // Obtener ultimo mensaje de una propuesta
    Task<MensajeNegociacion?> ObtenerUltimoMensaje(int propuestaId);

    // RF-12: Archivar conversacion cuando propuesta finaliza
    Task<bool> ArchivarConversacion(int propuestaId, string motivoCierre);

    // RF-12: Enviar mensaje de sistema (cierre de negociacion)
    Task<int> EnviarMensajeSistema(int propuestaId, string mensaje);

    // Obtener mensaje por ID
    Task<MensajeNegociacion?> ObtenerMensajePorId(int mensajeId);

    // Obtener propuesta por ID directamente (sin depender de mensajes existentes)
    Task<ConversacionResumen?> ObtenerPropuestaPorId(int propuestaId, int usuarioId);
}

// DTO para resumen de conversacion
public class ConversacionResumen
{
    public int PropuestaId { get; set; }
    public string TituloPrendaOfrecida { get; set; } = string.Empty;
    public string TituloPrendaSolicitada { get; set; } = string.Empty;
    public string ImagenPrendaOfrecida { get; set; } = string.Empty;
    public string ImagenPrendaSolicitada { get; set; } = string.Empty;
    public string TipoPrendaOfrecida { get; set; } = string.Empty;
    public string TipoPrendaSolicitada { get; set; } = string.Empty;
    public int PrendaOfrecidaId { get; set; }
    public int PrendaSolicitadaId { get; set; }
    public int OtroUsuarioId { get; set; }
    public string NombreOtroUsuario { get; set; } = string.Empty;
    public string FotoOtroUsuario { get; set; } = string.Empty;
    public string UltimoMensaje { get; set; } = string.Empty;
    public DateTime FechaUltimoMensaje { get; set; }
    public int MensajesNoLeidos { get; set; }
    public string EstadoPropuesta { get; set; } = string.Empty;
    public bool EsProponente { get; set; }
}
