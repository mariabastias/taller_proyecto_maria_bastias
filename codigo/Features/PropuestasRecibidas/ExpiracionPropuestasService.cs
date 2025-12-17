using TruequeTextil.Features.PropuestasRecibidas.Interfaces;
using TruequeTextil.Features.Notificaciones.Interfaces;

namespace TruequeTextil.Features.PropuestasRecibidas;

/// <summary>
/// Servicio para gestionar la expiración automática de propuestas de trueque (RF-11)
/// </summary>
public class ExpiracionPropuestasService : IExpiracionPropuestasService
{
    private readonly IExpiracionPropuestasRepository _repository;
    private readonly INotificacionesService _notificacionesService;
    private readonly ILogger<ExpiracionPropuestasService> _logger;

    public ExpiracionPropuestasService(
        IExpiracionPropuestasRepository repository,
        INotificacionesService notificacionesService,
        ILogger<ExpiracionPropuestasService> logger)
    {
        _repository = repository;
        _notificacionesService = notificacionesService;
        _logger = logger;
    }

    /// <summary>
    /// RF-11: Procesa propuestas expiradas y notifica a los usuarios afectados
    /// </summary>
    public async Task<int> ProcesarPropuestasExpiradas()
    {
        try
        {
            // 1. Marcar propuestas como expiradas
            var propuestasExpiradas = await _repository.MarcarPropuestasExpiradas();

            if (propuestasExpiradas == 0)
            {
                _logger.LogInformation("No hay propuestas expiradas para procesar");
                return 0;
            }

            _logger.LogInformation("Se marcaron {Cantidad} propuestas como expiradas", propuestasExpiradas);

            // 2. Obtener información de las propuestas expiradas para notificar
            var propuestasInfo = await _repository.ObtenerPropuestasRecienExpiradas();

            // 3. Enviar notificaciones a usuarios afectados
            foreach (var (propuestaId, proponenteId, receptorId, tituloPrenda) in propuestasInfo)
            {
                // Notificar al proponente
                await _notificacionesService.EnviarNotificacion(
                    proponenteId,
                    "Propuesta expirada",
                    $"Tu propuesta de trueque por \"{tituloPrenda}\" ha expirado por falta de respuesta.",
                    "propuesta_expirada",
                    propuestaId);

                // Notificar al receptor
                await _notificacionesService.EnviarNotificacion(
                    receptorId,
                    "Propuesta expirada",
                    $"La propuesta de trueque por tu prenda \"{tituloPrenda}\" ha expirado.",
                    "propuesta_expirada",
                    propuestaId);

                _logger.LogInformation(
                    "Notificaciones enviadas por expiración de propuesta {PropuestaId} a usuarios {Proponente} y {Receptor}",
                    propuestaId, proponenteId, receptorId);
            }

            return propuestasExpiradas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar propuestas expiradas");
            throw;
        }
    }

    /// <summary>
    /// RF-11: Obtiene propuestas próximas a expirar y envía recordatorio al receptor
    /// </summary>
    public async Task<List<int>> ObtenerPropuestasProximasAExpirar()
    {
        try
        {
            var propuestasProximas = await _repository.ObtenerPropuestasProximasAExpirar();

            foreach (var (propuestaId, receptorId, tituloPrenda, fechaExpiracion) in propuestasProximas)
            {
                var horasRestantes = (int)(fechaExpiracion - DateTime.Now).TotalHours;

                // Enviar recordatorio al receptor (solo si está dentro de las próximas 48 horas)
                if (horasRestantes <= 48 && horasRestantes > 0)
                {
                    await _notificacionesService.EnviarNotificacion(
                        receptorId,
                        "Propuesta próxima a expirar",
                        $"La propuesta de trueque por tu prenda \"{tituloPrenda}\" expirará en {horasRestantes} horas. ¡No olvides responder!",
                        "propuesta_proxima_expirar",
                        propuestaId);

                    _logger.LogInformation(
                        "Recordatorio enviado al usuario {UsuarioId} sobre propuesta {PropuestaId} que expira en {Horas} horas",
                        receptorId, propuestaId, horasRestantes);
                }
            }

            return propuestasProximas.Select(p => p.PropuestaId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propuestas próximas a expirar");
            return new List<int>();
        }
    }
}
