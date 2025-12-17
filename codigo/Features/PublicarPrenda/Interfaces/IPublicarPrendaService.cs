using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PublicarPrenda.Interfaces;

public interface IPublicarPrendaService
{
    Task<Usuario?> GetCurrentUser();
    Task<(bool Success, int PrendaId, string? Error)> PublicarPrenda(
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
