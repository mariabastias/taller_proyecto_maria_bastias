using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Notificaciones.Interfaces;

public interface INotificacionesService
{
    Task<List<Notificacion>> ObtenerNotificaciones(int usuarioId);
    Task<int> ContarNotificacionesNoLeidas(int usuarioId);
    Task MarcarComoLeida(int notificacionId);
    Task MarcarTodasComoLeidas(int usuarioId);
    Task EnviarNotificacion(int usuarioId, string titulo, string mensaje, string tipo, int? referenciaId = null);
}
