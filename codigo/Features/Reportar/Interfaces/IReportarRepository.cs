using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Reportar.Interfaces;

public interface IReportarRepository
{
    Task<int> CrearReporte(Reporte reporte);
    Task<bool> ExisteReporteActivo(int usuarioReportadorId, int? usuarioReportadoId, int? prendaId);
    Task<List<CategoriaReporteDTO>> ObtenerCategoriasReporte(TipoReporte tipo);
    Task<Usuario?> ObtenerUsuarioPorId(int usuarioId);
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<List<Reporte>> ObtenerMisReportes(int usuarioId);
}

// DTO para categorías de reporte
public record CategoriaReporteDTO(
    int CategoriaId,
    string Nombre,
    string? Descripcion,
    TipoReporte TipoAplicable
);
