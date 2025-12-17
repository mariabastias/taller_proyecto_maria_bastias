using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Reportar.Interfaces;

public interface IReportarService
{
    Task<(bool Exito, string Mensaje)> ReportarUsuario(int usuarioReportadorId, int usuarioReportadoId, string motivo, string? descripcion = null);
    Task<(bool Exito, string Mensaje)> ReportarPrenda(int usuarioReportadorId, int prendaId, string motivo, string? descripcion = null);
    Task<List<CategoriaReporteDTO>> ObtenerCategoriasReporte(TipoReporte tipo);
    Task<Usuario?> ObtenerUsuarioPorId(int usuarioId);
    Task<Prenda?> ObtenerPrendaPorId(int prendaId);
    Task<List<Reporte>> ObtenerMisReportes(int usuarioId);
    Task<bool> PuedeReportar(int usuarioReportadorId, TipoReporte tipo, int elementoId);
}
