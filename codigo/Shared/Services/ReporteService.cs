using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Services;

public class ReporteService
{
    private readonly IReporteSharedRepository _reporteRepository;

    public ReporteService(IReporteSharedRepository reporteRepository)
    {
        _reporteRepository = reporteRepository;
    }

    public async Task<List<Reporte>> GetReportesAsync()
    {
        return await _reporteRepository.GetReportesAsync();
    }

    public async Task<Reporte?> GetReporteByIdAsync(int id)
    {
        return await _reporteRepository.GetReporteByIdAsync(id);
    }

    public async Task<List<Reporte>> GetReportesByUsuarioReportanteIdAsync(int usuarioId)
    {
        return await _reporteRepository.GetReportesByUsuarioReportanteIdAsync(usuarioId);
    }

    public async Task<List<Reporte>> GetReportesByUsuarioReportadoIdAsync(int usuarioId)
    {
        return await _reporteRepository.GetReportesByUsuarioReportadoIdAsync(usuarioId);
    }

    public async Task<List<Reporte>> GetReportesByEstadoAsync(EstadoReporte estado)
    {
        var estadoStr = estado switch
        {
            EstadoReporte.Pendiente => "pendiente",
            EstadoReporte.Revisado => "en_revision",
            EstadoReporte.Resuelto => "resuelto",
            EstadoReporte.Desestimado => "descartado",
            _ => "pendiente"
        };
        return await _reporteRepository.GetReportesByEstadoAsync(estadoStr);
    }

    public async Task<List<Reporte>> GetReportesByTipoAsync(TipoReporte tipo)
    {
        var reportes = await _reporteRepository.GetReportesAsync();
        return reportes.Where(r => r.Tipo == tipo).ToList();
    }

    public async Task<Reporte> CreateReporteAsync(Reporte reporte)
    {
        var reporteId = await _reporteRepository.CreateReporteAsync(reporte);
        reporte.Id = reporteId;
        return reporte;
    }

    public async Task<bool> ActualizarEstadoAsync(int id, EstadoReporte estado)
    {
        var estadoStr = estado switch
        {
            EstadoReporte.Pendiente => "pendiente",
            EstadoReporte.Revisado => "en_revision",
            EstadoReporte.Resuelto => "resuelto",
            EstadoReporte.Desestimado => "descartado",
            _ => "pendiente"
        };
        return await _reporteRepository.ActualizarEstadoAsync(id, estadoStr);
    }

    public async Task<bool> DeleteReporteAsync(int id)
    {
        return await _reporteRepository.DeleteReporteAsync(id);
    }

    // Métodos para crear reportes específicos
    public async Task<Reporte> CrearReporteUsuarioAsync(int usuarioReportanteId, int usuarioReportadoId, string motivo, string descripcion)
    {
        var reporte = new Reporte
        {
            UsuarioReportanteId = usuarioReportanteId,
            UsuarioReportadoId = usuarioReportadoId,
            Tipo = TipoReporte.Usuario,
            Motivo = motivo,
            Descripcion = descripcion,
            CategoriaReporteId = 1 // Default category
        };

        return await CreateReporteAsync(reporte);
    }

    public async Task<Reporte> CrearReportePrendaAsync(int usuarioReportanteId, int prendaId, string motivo, string descripcion)
    {
        var reporte = new Reporte
        {
            UsuarioReportanteId = usuarioReportanteId,
            Tipo = TipoReporte.Prenda,
            Motivo = motivo,
            Descripcion = descripcion,
            PrendaReportadaId = prendaId,
            CategoriaReporteId = 1 // Default category
        };

        return await CreateReporteAsync(reporte);
    }

    public async Task<Reporte> CrearReporteTruequeAsync(int usuarioReportanteId, int usuarioReportadoId, int propuestaId, string motivo, string descripcion)
    {
        var reporte = new Reporte
        {
            UsuarioReportanteId = usuarioReportanteId,
            UsuarioReportadoId = usuarioReportadoId,
            Tipo = TipoReporte.Propuesta,
            Motivo = motivo,
            Descripcion = descripcion,
            CategoriaReporteId = 1 // Default category
        };

        return await CreateReporteAsync(reporte);
    }
}
