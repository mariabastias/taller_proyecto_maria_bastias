using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Repositories.Interfaces;

public interface IReporteSharedRepository
{
    Task<List<Reporte>> GetReportesAsync();
    Task<Reporte?> GetReporteByIdAsync(int id);
    Task<List<Reporte>> GetReportesByUsuarioReportanteIdAsync(int usuarioId);
    Task<List<Reporte>> GetReportesByUsuarioReportadoIdAsync(int usuarioId);
    Task<List<Reporte>> GetReportesByEstadoAsync(string estado);
    Task<int> CreateReporteAsync(Reporte reporte);
    Task<bool> ActualizarEstadoAsync(int id, string estado);
    Task<bool> DeleteReporteAsync(int id);
}
