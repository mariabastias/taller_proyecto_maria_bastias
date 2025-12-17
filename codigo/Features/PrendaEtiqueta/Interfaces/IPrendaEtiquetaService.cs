using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.PrendaEtiqueta.Interfaces;

public interface IPrendaEtiquetaService
{
    Task<(bool Success, string? Error)> AgregarEtiquetaAPrenda(int prendaId, int etiquetaId);
    Task<(bool Success, string? Error)> RemoverEtiquetaDePrenda(int prendaId, int etiquetaId);
    Task<List<int>> ObtenerEtiquetasDePrenda(int prendaId);
    Task<List<int>> ObtenerPrendasConEtiqueta(int etiquetaId);
    Task<bool> PrendaTieneEtiqueta(int prendaId, int etiquetaId);
    Task<int> ContarEtiquetasDePrenda(int prendaId);
    Task<int> ContarPrendasConEtiqueta(int etiquetaId);
    Task<(bool Success, string? Error)> GestionarEtiquetasDePrenda(int prendaId, List<int> etiquetasIds);
}
