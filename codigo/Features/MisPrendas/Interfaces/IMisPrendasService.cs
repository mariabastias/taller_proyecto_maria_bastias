using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.MisPrendas.Interfaces;

public interface IMisPrendasService
{
    Task<List<Prenda>> ObtenerMisPrendas(int usuarioId);
    Task<Prenda?> ObtenerPrenda(int prendaId, int usuarioId);

    // RF-05: Verificar propuestas activas antes de editar
    Task<(bool TienePropuestas, int CantidadPropuestas)> VerificarPropuestasActivas(int prendaId);

    // RF-05: Actualizar prenda con historial de cambios
    Task<(bool Success, string? Error)> ActualizarPrenda(
        int prendaId,
        int usuarioId,
        string titulo,
        string descripcion,
        int categoriaId,
        string talla,
        int estadoPrendaId);

    // RF-06: Eliminacion logica con cambio de estado
    Task<(bool Success, string? Error)> EliminarPrenda(int prendaId, int usuarioId);

    Task<(bool Success, string? Error)> CambiarDisponibilidad(int prendaId, int usuarioId, bool disponible);
}
