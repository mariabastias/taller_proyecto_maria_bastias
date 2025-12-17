using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPrenda.Interfaces;

public interface IEditarPrendaService
{
    Task<Usuario?> GetCurrentUser();
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<(bool Success, string? Error)> ActualizarPrenda(
        int prendaId,
        int usuarioId,
        string titulo,
        string descripcion,
        int categoriaId,
        string talla,
        int estadoPrendaId,
        List<string> imagenes);

    Task<List<CategoriaPrenda>> ObtenerCategorias();
    Task<List<EstadoPrenda>> ObtenerEstadosPrenda();
    Task<List<string>> ObtenerTallas();
}