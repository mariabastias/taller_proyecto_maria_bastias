using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePropuesta.Interfaces;

public interface IDetallePropuestaRepository
{
    // Propuesta
    Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId);
    Task<bool> AceptarPropuesta(int propuestaId, string? mensajeAceptacion = null);
    Task<bool> RechazarPropuesta(int propuestaId, string? motivoRechazo = null);
    Task<bool> CancelarPropuesta(int propuestaId, string? motivoCancelacion = null);
    Task<bool> CompletarTrueque(int propuestaId);
    Task<bool> ActualizarDisponibilidadPrendas(int prendaOfrecidaId, int prendaSolicitadaId, bool disponible);

    // RF-12: Mensajes de negociacion
    Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId);
    Task<int> EnviarMensaje(int propuestaId, int usuarioId, string mensaje);
    Task<bool> MarcarMensajesComoLeidos(int propuestaId, int usuarioId);
    Task<int> ContarMensajesNoLeidos(int propuestaId, int usuarioId);
}
