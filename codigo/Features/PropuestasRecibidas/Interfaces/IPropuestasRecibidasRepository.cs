using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PropuestasRecibidas.Interfaces;

public interface IPropuestasRecibidasRepository
{
    // Consultas
    Task<List<PropuestaTrueque>> ObtenerPropuestasRecibidas(int usuarioId);
    Task<List<PropuestaTrueque>> ObtenerPropuestasEnviadas(int usuarioId);
    Task<int> ContarPropuestasPendientes(int usuarioId);
    Task<PropuestaTrueque?> ObtenerPropuestaPorId(int propuestaId);

    // RF-10: Aceptar propuesta
    Task<bool> AceptarPropuesta(int propuestaId, string? mensajeAceptacion);

    // RF-11: Cambiar estado de ambas prendas a "en negociacion"
    Task<bool> CambiarEstadoPrendasANegociacion(int prendaOfrecidaId, int prendaSolicitadaId);

    // RF-11: Rechazar propuesta
    Task<bool> RechazarPropuesta(int propuestaId, string? motivoRechazo);

    // Cancelar propuesta (por el proponente)
    Task<bool> CancelarPropuesta(int propuestaId, string? motivoCancelacion);

    // Marcar expiradas (propuestas > 7 dias sin respuesta)
    Task<int> MarcarPropuestasExpiradas();
}
