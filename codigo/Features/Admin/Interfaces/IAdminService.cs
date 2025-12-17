using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Admin.Interfaces;

// DTO para estadisticas de reportes
public record EstadisticasReportesDTO(int Pendientes, int Revisados, int Resueltos, int Desestimados);

// DTO para estadisticas del dashboard
public record DashboardStatsDTO(int TotalUsuarios, int TotalPrendas, int TotalTrueques, int UsuariosActivos);

// DTO para informes generales
public record InformeGeneralDTO(
    int TotalUsuarios,
    int UsuariosNuevos,
    int TotalPrendas,
    int PrendasNuevas,
    int TruequesCompletados,
    int TruequesPendientes,
    int ReportesPendientes,
    int ReportesResueltos,
    decimal ReputacionPromedio,
    DateTime FechaGeneracion,
    DateTime? FechaInicio,
    DateTime? FechaFin
);

public interface IAdminService
{
    // RF-17: Gestion de reportes
    Task<List<Reporte>> ObtenerReportes(string? estado = null, string? tipo = null, int pagina = 1, int porPagina = 20);
    Task<(List<Reporte> Items, int Total, int TotalPaginas)> ObtenerReportesPaginados(string? estado, string? tipo, int pagina, int porPagina);
    Task<Reporte?> ObtenerReportePorId(int reporteId);
    Task<(bool Exito, string Mensaje)> CambiarEstadoReporte(int reporteId, string nuevoEstado, int adminId, string? comentario = null);
    Task<EstadisticasReportesDTO> ObtenerEstadisticasReportes();

    // RF-16/RF-17: Gestion de usuarios
    Task<(List<Usuario> Items, int Total, int TotalPaginas)> ObtenerUsuariosPaginados(string? busqueda, string? estado, int pagina, int porPagina);
    Task<Usuario?> ObtenerDetalleUsuario(int usuarioId);
    Task<(bool Exito, string Mensaje)> SuspenderUsuario(int usuarioId, int adminId, string motivo, int? diasSuspension = null);
    Task<(bool Exito, string Mensaje)> ReactivarUsuario(int usuarioId, int adminId);
    Task<(bool Exito, string Mensaje)> VerificarUsuario(int usuarioId, int adminId);
    Task<(bool Exito, string Mensaje)> CambiarRolUsuario(int usuarioId, int adminId, string nuevoRol);

    // RF-17: Gestion de prendas
    Task<(List<Prenda> Items, int Total, int TotalPaginas)> ObtenerPrendasPaginadas(string? busqueda, string? estado, string? categoria, int pagina, int porPagina);
    Task<(bool Exito, string Mensaje)> DesactivarPrenda(int prendaId, int adminId, string motivo);

    // RF-17: Informes y estadisticas
    Task<InformeGeneralDTO> GenerarInformeGeneral(DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<byte[]> ExportarInformeCSV(DateTime? fechaInicio = null, DateTime? fechaFin = null);

    // Dashboard
    Task<DashboardStatsDTO> ObtenerEstadisticasDashboard();
    Task<int> ObtenerPropuestasPendientes();

    // Verificacion de rol admin
    Task<bool> EsAdministrador(int usuarioId);

    // Acciones sobre reportes
    Task<(bool Exito, string Mensaje)> AprobarReporte(int reporteId, int adminId, string? comentario = null);
    Task<(bool Exito, string Mensaje)> RechazarReporte(int reporteId, int adminId, string? comentario = null);
}
