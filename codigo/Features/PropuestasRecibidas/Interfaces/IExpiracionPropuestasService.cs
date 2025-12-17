namespace TruequeTextil.Features.PropuestasRecibidas.Interfaces;

/// <summary>
/// Servicio para gestionar la expiración automática de propuestas de trueque (RF-11)
/// </summary>
public interface IExpiracionPropuestasService
{
    /// <summary>
    /// Marca como expiradas las propuestas pendientes que superaron los 7 días sin respuesta
    /// y notifica a los usuarios afectados
    /// </summary>
    /// <returns>Número de propuestas marcadas como expiradas</returns>
    Task<int> ProcesarPropuestasExpiradas();

    /// <summary>
    /// Obtiene las propuestas que están próximas a expirar (menos de 48 horas)
    /// para enviar notificaciones de recordatorio
    /// </summary>
    /// <returns>Lista de propuesta IDs próximas a expirar</returns>
    Task<List<int>> ObtenerPropuestasProximasAExpirar();
}
