namespace TruequeTextil.Features.PropuestasRecibidas.Interfaces;

/// <summary>
/// Repositorio para operaciones de expiración de propuestas de trueque (RF-11)
/// </summary>
public interface IExpiracionPropuestasRepository
{
    /// <summary>
    /// Marca como expiradas las propuestas pendientes que superaron los 7 días sin respuesta
    /// </summary>
    /// <returns>Número de propuestas marcadas como expiradas</returns>
    Task<int> MarcarPropuestasExpiradas();

    /// <summary>
    /// Obtiene información de las propuestas que fueron marcadas como expiradas
    /// para enviar notificaciones a los usuarios afectados
    /// </summary>
    /// <returns>Lista de tuplas con (PropuestaId, UsuarioProponenteId, UsuarioReceptorId, TituloPrenda)</returns>
    Task<List<(int PropuestaId, int UsuarioProponenteId, int UsuarioReceptorId, string TituloPrenda)>> ObtenerPropuestasRecienExpiradas();

    /// <summary>
    /// Obtiene las propuestas que están próximas a expirar (menos de 48 horas)
    /// </summary>
    /// <returns>Lista de tuplas con información de propuestas próximas a expirar</returns>
    Task<List<(int PropuestaId, int UsuarioReceptorId, string TituloPrenda, DateTime FechaExpiracion)>> ObtenerPropuestasProximasAExpirar();
}
