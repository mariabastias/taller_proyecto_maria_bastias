using TruequeTextil.Features.PropuestasRecibidas.Interfaces;
using TruequeTextil.Features.Notificaciones.Interfaces;
using TruequeTextil.Features.Mensajeria.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;

namespace TruequeTextil.Features.PropuestasRecibidas;

public class PropuestasRecibidasService : IPropuestasRecibidasService
{
    private readonly IPropuestasRecibidasRepository _repository;
    private readonly INotificacionesService _notificacionesService;
    private readonly IMensajeriaRepository _mensajeriaRepository;
    private readonly ILogger<PropuestasRecibidasService> _logger;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    public PropuestasRecibidasService(
        IPropuestasRecibidasRepository repository,
        INotificacionesService notificacionesService,
        IMensajeriaRepository mensajeriaRepository,
        ILogger<PropuestasRecibidasService> logger,
        CustomAuthenticationStateProvider authenticationStateProvider)
    {
        _repository = repository;
        _notificacionesService = notificacionesService;
        _mensajeriaRepository = mensajeriaRepository;
        _logger = logger;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<List<PropuestaTrueque>> ObtenerPropuestasRecibidas(int usuarioId)
    {
        try
        {
            // Marcar propuestas expiradas antes de listar
            await _repository.MarcarPropuestasExpiradas();
            return await _repository.ObtenerPropuestasRecibidas(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propuestas recibidas del usuario {UsuarioId}", usuarioId);
            return new List<PropuestaTrueque>();
        }
    }

    public async Task<List<PropuestaTrueque>> ObtenerPropuestasEnviadas(int usuarioId)
    {
        try
        {
            // Marcar propuestas expiradas antes de listar
            await _repository.MarcarPropuestasExpiradas();
            return await _repository.ObtenerPropuestasEnviadas(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propuestas enviadas del usuario {UsuarioId}", usuarioId);
            return new List<PropuestaTrueque>();
        }
    }

    public async Task<int> ContarPropuestasPendientes(int usuarioId)
    {
        try
        {
            return await _repository.ContarPropuestasPendientes(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar propuestas pendientes del usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }

    public async Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId)
    {
        try
        {
            return await _repository.ObtenerPropuestaPorId(propuestaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propuesta {PropuestaId}", propuestaId);
            return null;
        }
    }

    // RF-10: Aceptar propuesta (inicia negociacion)
    // RF-11: Notificar a ambos usuarios y cambiar estado de AMBAS prendas a "en negociacion"
    public async Task<(bool Exito, string Mensaje)> AceptarPropuesta(int propuestaId, int usuarioId, string? mensajeAceptacion = null)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuestaPorId(propuestaId);

            if (propuesta == null)
            {
                return (false, "La propuesta no existe");
            }

            // Solo el receptor puede aceptar (dueno de la prenda solicitada)
            if (propuesta.PrendaSolicitada?.UsuarioId != usuarioId)
            {
                return (false, "No tienes permiso para aceptar esta propuesta");
            }

            // Solo se puede aceptar si esta pendiente
            if (propuesta.EstadoPropuestaId != 1)
            {
                return (false, "Esta propuesta ya no esta pendiente");
            }

            // Verificar si no ha expirado
            if (propuesta.FechaExpiracion.HasValue && propuesta.FechaExpiracion < DateTime.Now)
            {
                await _repository.MarcarPropuestasExpiradas();
                return (false, "Esta propuesta ha expirado");
            }

            var resultado = await _repository.AceptarPropuesta(propuestaId, mensajeAceptacion);

            if (resultado)
            {
                // RF-11: Cambiar estado de AMBAS prendas a "en negociacion"
                await _repository.CambiarEstadoPrendasANegociacion(
                    propuesta.PrendaOfrecidaId,
                    propuesta.PrendaSolicitadaId);

                // RF-11: Notificar al proponente que su propuesta fue aceptada
                await _notificacionesService.EnviarNotificacion(
                    propuesta.UsuarioProponenteId,
                    "Propuesta aceptada",
                    $"Tu propuesta de trueque por \"{propuesta.PrendaSolicitada?.TituloPublicacion}\" ha sido aceptada.",
                    "propuesta_aceptada",
                    propuestaId);

                // RF-11: Notificar al receptor (confirmacion de su accion)
                await _notificacionesService.EnviarNotificacion(
                    usuarioId,
                    "Trueque iniciado",
                    $"Has aceptado la propuesta de trueque por \"{propuesta.PrendaOfrecida?.TituloPublicacion}\". Ahora pueden coordinar los detalles.",
                    "trueque_iniciado",
                    propuestaId);

                _logger.LogInformation("Propuesta {PropuestaId} aceptada por usuario {UsuarioId}. Prendas {PrendaOfrecida} y {PrendaSolicitada} en negociacion.",
                    propuestaId, usuarioId, propuesta.PrendaOfrecidaId, propuesta.PrendaSolicitadaId);

                return (true, "Propuesta aceptada. Ahora pueden coordinar los detalles del trueque.");
            }

            return (false, "No se pudo aceptar la propuesta");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aceptar propuesta {PropuestaId}", propuestaId);
            return (false, "Error al procesar la aceptacion");
        }
    }

    // RF-11: Rechazar propuesta
    // RF-12: Archivar conversacion al rechazar
    public async Task<(bool Exito, string Mensaje)> RechazarPropuesta(int propuestaId, int usuarioId, string? motivoRechazo = null)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuestaPorId(propuestaId);

            if (propuesta == null)
            {
                return (false, "La propuesta no existe");
            }

            // Solo el receptor puede rechazar
            if (propuesta.PrendaSolicitada?.UsuarioId != usuarioId)
            {
                return (false, "No tienes permiso para rechazar esta propuesta");
            }

            // Solo se puede rechazar si esta pendiente
            if (propuesta.EstadoPropuestaId != 1)
            {
                return (false, "Esta propuesta ya no esta pendiente");
            }

            var resultado = await _repository.RechazarPropuesta(propuestaId, motivoRechazo);

            if (resultado)
            {
                // RF-12: Archivar conversacion con mensaje de cierre
                await _mensajeriaRepository.ArchivarConversacion(propuestaId,
                    "La propuesta ha sido rechazada. Esta conversacion ha sido archivada.");

                // Notificar al proponente
                await _notificacionesService.EnviarNotificacion(
                    propuesta.UsuarioProponenteId,
                    "Propuesta rechazada",
                    $"Tu propuesta de trueque por \"{propuesta.PrendaSolicitada?.TituloPublicacion}\" ha sido rechazada.",
                    "propuesta_rechazada",
                    propuestaId);

                _logger.LogInformation("Propuesta {PropuestaId} rechazada por usuario {UsuarioId}. Conversacion archivada.", propuestaId, usuarioId);
                return (true, "Propuesta rechazada");
            }

            return (false, "No se pudo rechazar la propuesta");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar propuesta {PropuestaId}", propuestaId);
            return (false, "Error al procesar el rechazo");
        }
    }

    // Cancelar propuesta propia
    // RF-12: Archivar conversacion al cancelar
    public async Task<(bool Exito, string Mensaje)> CancelarPropuesta(int propuestaId, int usuarioId, string? motivoCancelacion = null)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuestaPorId(propuestaId);

            if (propuesta == null)
            {
                return (false, "La propuesta no existe");
            }

            // Solo el proponente puede cancelar
            if (propuesta.UsuarioProponenteId != usuarioId)
            {
                return (false, "No tienes permiso para cancelar esta propuesta");
            }

            // Solo se puede cancelar si esta pendiente
            if (propuesta.EstadoPropuestaId != 1)
            {
                return (false, "Esta propuesta ya no esta pendiente");
            }

            var resultado = await _repository.CancelarPropuesta(propuestaId, motivoCancelacion);

            if (resultado)
            {
                // RF-12: Archivar conversacion con mensaje de cierre
                await _mensajeriaRepository.ArchivarConversacion(propuestaId,
                    "La propuesta ha sido cancelada por el proponente. Esta conversacion ha sido archivada.");

                // Notificar al receptor
                await _notificacionesService.EnviarNotificacion(
                    propuesta.PrendaSolicitada!.UsuarioId,
                    "Propuesta cancelada",
                    $"La propuesta de trueque por \"{propuesta.PrendaSolicitada?.TituloPublicacion}\" ha sido cancelada.",
                    "propuesta_cancelada",
                    propuestaId);

                _logger.LogInformation("Propuesta {PropuestaId} cancelada por usuario {UsuarioId}. Conversacion archivada.", propuestaId, usuarioId);
                return (true, "Propuesta cancelada");
            }

            return (false, "No se pudo cancelar la propuesta");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar propuesta {PropuestaId}", propuestaId);
            return (false, "Error al procesar la cancelacion");
        }
    }
}
