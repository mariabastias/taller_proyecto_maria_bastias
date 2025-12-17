using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.MisPrendas.Interfaces;

public interface IMisPrendasRepository
{
    Task<List<Prenda>> ObtenerPrendasUsuario(int usuarioId);
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<bool> ActualizarPrenda(Prenda prenda);
    Task<bool> EliminarPrenda(int prendaId); // Eliminacion logica (RF-06)
    Task<bool> CambiarDisponibilidad(int prendaId, int estadoPublicacionId);
    Task<int> ContarPropuestasActivas(int prendaId);
    Task<bool> RegistrarHistorial(HistorialPrenda historial); // RF-05
    Task<List<int>> ObtenerUsuariosConPrendaEnFavoritos(int prendaId); // RF-06: Para notificacion
}
