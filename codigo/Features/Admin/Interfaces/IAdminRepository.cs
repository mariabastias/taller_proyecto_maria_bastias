using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Admin.Interfaces;

public interface IAdminRepository
{
    // RF-17: Gestion de reportes
    Task<List<Reporte>> ObtenerReportes(string? estado = null, string? tipo = null, int pagina = 1, int porPagina = 20);
    Task<int> ContarReportes(string? estado = null, string? tipo = null);
    Task<Reporte?> ObtenerReportePorId(int reporteId);
    Task<bool> ActualizarEstadoReporte(int reporteId, string nuevoEstado, int adminId, string? comentario = null);
    Task<(int Pendientes, int Revisados, int Resueltos, int Desestimados)> ObtenerEstadisticasReportes();

    // RF-16/RF-17: Gestion de usuarios
    Task<List<Usuario>> ObtenerUsuarios(string? busqueda = null, string? estado = null, int pagina = 1, int porPagina = 20);
    Task<int> ContarUsuarios(string? busqueda = null, string? estado = null);
    Task<Usuario?> ObtenerDetalleUsuario(int usuarioId);
    Task<bool> SuspenderUsuario(int usuarioId, int adminId, string motivo, int? diasSuspension = null);
    Task<bool> ReactivarUsuario(int usuarioId, int adminId);
    Task<bool> VerificarUsuario(int usuarioId, int adminId);
    Task<bool> CambiarRolUsuario(int usuarioId, int adminId, string nuevoRol);

    // RF-17: Gestion de prendas
    Task<List<Prenda>> ObtenerPrendasReportadas(int pagina = 1, int porPagina = 20);
    Task<List<Prenda>> ObtenerPrendas(string? busqueda = null, string? estado = null, string? categoria = null, int pagina = 1, int porPagina = 20);
    Task<int> ContarPrendas(string? busqueda = null, string? estado = null, string? categoria = null);
    Task<bool> DesactivarPrenda(int prendaId, int adminId, string motivo);

    // RF-17: Informes
    Task<(int UsuariosNuevos, int PrendasNuevas, int TruequesCompletados, int TruequesPendientes, int ReportesResueltos, decimal ReputacionPromedio)> ObtenerEstadisticasPeriodo(DateTime? fechaInicio, DateTime? fechaFin);

    // Dashboard
    Task<(int TotalUsuarios, int TotalPrendas, int TotalTrueques, int UsuariosActivos)> ObtenerEstadisticasGenerales();
    Task<int> ObtenerPropuestasPendientes();

    // Acciones sobre reportes
    Task<bool> EjecutarAccionReporte(int reporteId, int adminId, string tipoAccion);
}
