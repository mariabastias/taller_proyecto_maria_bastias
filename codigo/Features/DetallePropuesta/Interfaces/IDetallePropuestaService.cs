using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePropuesta.Interfaces;

public interface IDetallePropuestaService
{
    // Propuesta
    Task<PropuestaTrueque?> ObtenerDetallePropuesta(int propuestaId, int usuarioId);
    Task<(bool Exito, string Mensaje)> AceptarPropuesta(int propuestaId, int usuarioId, string? mensajeAceptacion = null);
    Task<(bool Exito, string Mensaje)> RechazarPropuesta(int propuestaId, int usuarioId, string? motivoRechazo = null);
    Task<(bool Exito, string Mensaje)> CancelarPropuesta(int propuestaId, int usuarioId, string? motivoCancelacion = null);
    Task<(bool Exito, string Mensaje)> CompletarTrueque(int propuestaId, int usuarioId);

    // RF-12: Mensajes de negociacion
    Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId, int usuarioId);
    Task<(bool Exito, string Mensaje)> EnviarMensaje(int propuestaId, int usuarioId, string mensaje);
    Task<int> ContarMensajesNoLeidos(int propuestaId, int usuarioId);
}
