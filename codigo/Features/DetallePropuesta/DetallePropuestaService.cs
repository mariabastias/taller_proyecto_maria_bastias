using TruequeTextil.Features.DetallePropuesta.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePropuesta;

public class DetallePropuestaService : IDetallePropuestaService
{
    private readonly IDetallePropuestaRepository _repository;
    private readonly ILogger<DetallePropuestaService> _logger;

    public DetallePropuestaService(
        IDetallePropuestaRepository repository,
        ILogger<DetallePropuestaService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PropuestaTrueque?> ObtenerDetallePropuesta(int propuestaId, int usuarioId)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return null;
            }

            // Verificar que el usuario es parte de la propuesta
            if (propuesta.UsuarioProponenteId != usuarioId && propuesta.UsuarioReceptorId != usuarioId)
            {
                _logger.LogWarning("Usuario {UsuarioId} intento acceder a propuesta {PropuestaId} sin autorizacion",
                    usuarioId, propuestaId);
                return null;
            }

            return propuesta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de propuesta {PropuestaId}", propuestaId);
            return null;
        }
    }

    public async Task<(bool Exito, string Mensaje)> AceptarPropuesta(int propuestaId, int usuarioId, string? mensajeAceptacion = null)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return (false, "Propuesta no encontrada");
            }

            if (propuesta.UsuarioReceptorId != usuarioId)
            {
                return (false, "Solo el receptor puede aceptar la propuesta");
            }

            if (propuesta.EstadoPropuestaId != 1)
            {
                return (false, "La propuesta ya no esta pendiente");
            }

            var resultado = await _repository.AceptarPropuesta(propuestaId, mensajeAceptacion);

            if (resultado)
            {
                _logger.LogInformation("Propuesta {PropuestaId} aceptada por usuario {UsuarioId}", propuestaId, usuarioId);
                return (true, "Propuesta aceptada exitosamente");
            }

            return (false, "No se pudo aceptar la propuesta");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aceptar propuesta {PropuestaId}", propuestaId);
            return (false, "Error al procesar la aceptacion");
        }
    }

    public async Task<(bool Exito, string Mensaje)> RechazarPropuesta(int propuestaId, int usuarioId, string? motivoRechazo = null)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return (false, "Propuesta no encontrada");
            }

            if (propuesta.UsuarioReceptorId != usuarioId)
            {
                return (false, "Solo el receptor puede rechazar la propuesta");
            }

            if (propuesta.EstadoPropuestaId != 1)
            {
                return (false, "La propuesta ya no esta pendiente");
            }

            var resultado = await _repository.RechazarPropuesta(propuestaId, motivoRechazo);

            if (resultado)
            {
                _logger.LogInformation("Propuesta {PropuestaId} rechazada por usuario {UsuarioId}", propuestaId, usuarioId);
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

    public async Task<(bool Exito, string Mensaje)> CancelarPropuesta(int propuestaId, int usuarioId, string? motivoCancelacion = null)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return (false, "Propuesta no encontrada");
            }

            if (propuesta.UsuarioProponenteId != usuarioId)
            {
                return (false, "Solo el proponente puede cancelar la propuesta");
            }

            if (propuesta.EstadoPropuestaId != 1 && propuesta.EstadoPropuestaId != 2)
            {
                return (false, "La propuesta no puede ser cancelada en su estado actual");
            }

            var resultado = await _repository.CancelarPropuesta(propuestaId, motivoCancelacion);

            if (resultado)
            {
                _logger.LogInformation("Propuesta {PropuestaId} cancelada por usuario {UsuarioId}", propuestaId, usuarioId);
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

    public async Task<(bool Exito, string Mensaje)> CompletarTrueque(int propuestaId, int usuarioId)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return (false, "Propuesta no encontrada");
            }

            if (propuesta.UsuarioProponenteId != usuarioId && propuesta.UsuarioReceptorId != usuarioId)
            {
                return (false, "No tienes permiso para completar este trueque");
            }

            if (propuesta.EstadoPropuestaId != 2)
            {
                return (false, "El trueque debe estar aceptado para completarse");
            }

            var resultado = await _repository.CompletarTrueque(propuestaId);

            if (resultado)
            {
                _logger.LogInformation("Trueque {PropuestaId} completado por usuario {UsuarioId}", propuestaId, usuarioId);
                return (true, "Trueque completado exitosamente. Ahora pueden evaluarse mutuamente.");
            }

            return (false, "No se pudo completar el trueque");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar trueque {PropuestaId}", propuestaId);
            return (false, "Error al procesar la finalizacion del trueque");
        }
    }

    // RF-12: Obtener mensajes de negociacion
    public async Task<List<MensajeNegociacion>> ObtenerMensajes(int propuestaId, int usuarioId)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return new List<MensajeNegociacion>();
            }

            // Verificar autorizacion
            if (propuesta.UsuarioProponenteId != usuarioId && propuesta.UsuarioReceptorId != usuarioId)
            {
                return new List<MensajeNegociacion>();
            }

            // Marcar mensajes como leidos
            await _repository.MarcarMensajesComoLeidos(propuestaId, usuarioId);

            return await _repository.ObtenerMensajes(propuestaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes de propuesta {PropuestaId}", propuestaId);
            return new List<MensajeNegociacion>();
        }
    }

    // RF-12: Enviar mensaje
    public async Task<(bool Exito, string Mensaje)> EnviarMensaje(int propuestaId, int usuarioId, string mensaje)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return (false, "El mensaje no puede estar vacio");
            }

            if (mensaje.Length > 1000)
            {
                return (false, "El mensaje excede el limite de 1000 caracteres");
            }

            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return (false, "Propuesta no encontrada");
            }

            // Solo participantes pueden enviar mensajes
            if (propuesta.UsuarioProponenteId != usuarioId && propuesta.UsuarioReceptorId != usuarioId)
            {
                return (false, "No tienes permiso para enviar mensajes en esta propuesta");
            }

            // Solo se puede chatear si la propuesta esta aceptada (en negociacion)
            if (propuesta.EstadoPropuestaId != 2)
            {
                return (false, "Solo se puede enviar mensajes en propuestas aceptadas");
            }

            var mensajeId = await _repository.EnviarMensaje(propuestaId, usuarioId, mensaje);

            if (mensajeId > 0)
            {
                _logger.LogInformation("Mensaje {MensajeId} enviado en propuesta {PropuestaId} por usuario {UsuarioId}",
                    mensajeId, propuestaId, usuarioId);
                return (true, "Mensaje enviado");
            }

            return (false, "No se pudo enviar el mensaje");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje en propuesta {PropuestaId}", propuestaId);
            return (false, "Error al enviar el mensaje");
        }
    }

    // RF-12: Contar mensajes no leidos
    public async Task<int> ContarMensajesNoLeidos(int propuestaId, int usuarioId)
    {
        try
        {
            return await _repository.ContarMensajesNoLeidos(propuestaId, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar mensajes no leidos en propuesta {PropuestaId}", propuestaId);
            return 0;
        }
    }
}
