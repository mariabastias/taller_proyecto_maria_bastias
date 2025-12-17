using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PropuestasRecibidas.Interfaces;

public interface IPropuestasRecibidasService
{
    Task<Usuario?> GetCurrentUser();
    // Consultas
    Task<List<PropuestaTrueque>> ObtenerPropuestasRecibidas(int usuarioId);
    Task<List<PropuestaTrueque>> ObtenerPropuestasEnviadas(int usuarioId);
    Task<int> ContarPropuestasPendientes(int usuarioId);
    Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId);

    // RF-10: Aceptar propuesta (inicia negociacion)
    Task<(bool Exito, string Mensaje)> AceptarPropuesta(int propuestaId, int usuarioId, string? mensajeAceptacion = null);

    // RF-11: Rechazar propuesta
    Task<(bool Exito, string Mensaje)> RechazarPropuesta(int propuestaId, int usuarioId, string? motivoRechazo = null);

    // Cancelar propuesta propia
    Task<(bool Exito, string Mensaje)> CancelarPropuesta(int propuestaId, int usuarioId, string? motivoCancelacion = null);
}
