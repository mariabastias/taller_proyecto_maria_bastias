using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Notificaciones.Interfaces;

public interface INotificacionesRepository
{
    Task<List<Notificacion>> ObtenerNotificaciones(int usuarioId);
    Task<int> ContarNotificacionesNoLeidas(int usuarioId);
    Task<bool> MarcarComoLeida(int notificacionId);
    Task<bool> MarcarTodasComoLeidas(int usuarioId);
    Task<int> CrearNotificacion(Notificacion notificacion);
}
