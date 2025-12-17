using TruequeTextil.Features.Admin.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Features.Admin;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repository;
    private readonly IUsuarioSharedRepository _usuarioRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAdminRepository repository,
        IUsuarioSharedRepository usuarioRepository,
        ILogger<AdminService> logger)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    // RF-17: Verificar rol admin
    public async Task<bool> EsAdministrador(int usuarioId)
    {
        try
        {
            var usuario = await _usuarioRepository.ObtenerUsuarioPorId(usuarioId);
            return usuario?.Rol == "admin" || usuario?.Rol == "administrador";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar rol admin para usuario {UsuarioId}", usuarioId);
            return false;
        }
    }

    // RF-17: Gestion de reportes
    public async Task<List<Reporte>> ObtenerReportes(string? estado = null, string? tipo = null, int pagina = 1, int porPagina = 20)
    {
        try
        {
            return await _repository.ObtenerReportes(estado, tipo, pagina, porPagina);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes");
            return new List<Reporte>();
        }
    }

    public async Task<(List<Reporte> Items, int Total, int TotalPaginas)> ObtenerReportesPaginados(string? estado, string? tipo, int pagina, int porPagina)
    {
        try
        {
            var items = await _repository.ObtenerReportes(estado, tipo, pagina, porPagina);
            var total = await _repository.ContarReportes(estado, tipo);
            var totalPaginas = (int)Math.Ceiling((double)total / porPagina);

            return (items, total, totalPaginas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes paginados");
            return (new List<Reporte>(), 0, 0);
        }
    }

    public async Task<Reporte?> ObtenerReportePorId(int reporteId)
    {
        try
        {
            return await _repository.ObtenerReportePorId(reporteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte {ReporteId}", reporteId);
            return null;
        }
    }

    public async Task<(bool Exito, string Mensaje)> CambiarEstadoReporte(int reporteId, string nuevoEstado, int adminId, string? comentario = null)
    {
        try
        {
            // Validar estado
            var estadosValidos = new[] { "pendiente", "revisado", "resuelto", "desestimado" };
            if (!estadosValidos.Contains(nuevoEstado.ToLower()))
            {
                return (false, "Estado no valido");
            }

            var resultado = await _repository.ActualizarEstadoReporte(reporteId, nuevoEstado.ToLower(), adminId, comentario);

            if (resultado)
            {
                _logger.LogInformation("Admin {AdminId} cambio estado de reporte {ReporteId} a {NuevoEstado}",
                    adminId, reporteId, nuevoEstado);
                return (true, $"Reporte marcado como {nuevoEstado}");
            }

            return (false, "No se pudo actualizar el reporte");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de reporte {ReporteId}", reporteId);
            return (false, "Error al procesar la accion");
        }
    }

    public async Task<EstadisticasReportesDTO> ObtenerEstadisticasReportes()
    {
        try
        {
            var (pendientes, revisados, resueltos, desestimados) = await _repository.ObtenerEstadisticasReportes();
            return new EstadisticasReportesDTO(pendientes, revisados, resueltos, desestimados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadisticas de reportes");
            return new EstadisticasReportesDTO(0, 0, 0, 0);
        }
    }

    // RF-16/RF-17: Gestion de usuarios
    public async Task<(List<Usuario> Items, int Total, int TotalPaginas)> ObtenerUsuariosPaginados(string? busqueda, string? estado, int pagina, int porPagina)
    {
        try
        {
            var items = await _repository.ObtenerUsuarios(busqueda, estado, pagina, porPagina);
            var total = await _repository.ContarUsuarios(busqueda, estado);
            var totalPaginas = (int)Math.Ceiling((double)total / porPagina);

            return (items, total, totalPaginas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios paginados");
            return (new List<Usuario>(), 0, 0);
        }
    }

    public async Task<(bool Exito, string Mensaje)> SuspenderUsuario(int usuarioId, int adminId, string motivo, int? diasSuspension = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return (false, "Debe proporcionar un motivo para la suspension");
            }

            var resultado = await _repository.SuspenderUsuario(usuarioId, adminId, motivo, diasSuspension);

            if (resultado)
            {
                _logger.LogInformation("Admin {AdminId} suspendio usuario {UsuarioId}: {Motivo}",
                    adminId, usuarioId, motivo);
                return (true, "Usuario suspendido exitosamente");
            }

            return (false, "No se pudo suspender al usuario");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al suspender usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar la suspension");
        }
    }

    public async Task<(bool Exito, string Mensaje)> ReactivarUsuario(int usuarioId, int adminId)
    {
        try
        {
            var resultado = await _repository.ReactivarUsuario(usuarioId, adminId);

            if (resultado)
            {
                _logger.LogInformation("Admin {AdminId} reactivo usuario {UsuarioId}", adminId, usuarioId);
                return (true, "Usuario reactivado exitosamente");
            }

            return (false, "No se pudo reactivar al usuario");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reactivar usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar la reactivacion");
        }
    }

    public async Task<(bool Exito, string Mensaje)> VerificarUsuario(int usuarioId, int adminId)
    {
        try
        {
            var resultado = await _repository.VerificarUsuario(usuarioId, adminId);

            if (resultado)
            {
                _logger.LogInformation("Admin {AdminId} verifico usuario {UsuarioId}", adminId, usuarioId);
                return (true, "Usuario verificado exitosamente");
            }

            return (false, "No se pudo verificar al usuario");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar la verificacion");
        }
    }

    // RF-17: Gestion de prendas
    public async Task<(List<Prenda> Items, int Total, int TotalPaginas)> ObtenerPrendasPaginadas(string? busqueda, string? estado, string? categoria, int pagina, int porPagina)
    {
        try
        {
            var items = await _repository.ObtenerPrendas(busqueda, estado, categoria, pagina, porPagina);
            var total = await _repository.ContarPrendas(busqueda, estado, categoria);
            var totalPaginas = (int)Math.Ceiling((double)total / porPagina);

            return (items, total, totalPaginas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas paginadas");
            return (new List<Prenda>(), 0, 0);
        }
    }

    public async Task<(bool Exito, string Mensaje)> DesactivarPrenda(int prendaId, int adminId, string motivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return (false, "Debe proporcionar un motivo para desactivar la prenda");
            }

            var resultado = await _repository.DesactivarPrenda(prendaId, adminId, motivo);

            if (resultado)
            {
                _logger.LogInformation("Admin {AdminId} desactivo prenda {PrendaId}: {Motivo}",
                    adminId, prendaId, motivo);
                return (true, "Prenda desactivada exitosamente");
            }

            return (false, "No se pudo desactivar la prenda");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar prenda {PrendaId}", prendaId);
            return (false, "Error al procesar la desactivacion");
        }
    }

    // RF-17: Informes y estadisticas
    public async Task<InformeGeneralDTO> GenerarInformeGeneral(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var (totalUsuarios, totalPrendas, totalTrueques, usuariosActivos) = await _repository.ObtenerEstadisticasGenerales();
            var (usuariosNuevos, prendasNuevas, truequesCompletados, truequesPendientes, reportesResueltos, reputacionPromedio) =
                await _repository.ObtenerEstadisticasPeriodo(fechaInicio, fechaFin);
            var estadisticasReportes = await _repository.ObtenerEstadisticasReportes();

            return new InformeGeneralDTO(
                TotalUsuarios: totalUsuarios,
                UsuariosNuevos: usuariosNuevos,
                TotalPrendas: totalPrendas,
                PrendasNuevas: prendasNuevas,
                TruequesCompletados: truequesCompletados,
                TruequesPendientes: truequesPendientes,
                ReportesPendientes: estadisticasReportes.Pendientes,
                ReportesResueltos: reportesResueltos,
                ReputacionPromedio: reputacionPromedio,
                FechaGeneracion: DateTime.Now,
                FechaInicio: fechaInicio,
                FechaFin: fechaFin
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar informe general");
            return new InformeGeneralDTO(0, 0, 0, 0, 0, 0, 0, 0, 0, DateTime.Now, fechaInicio, fechaFin);
        }
    }

    public async Task<byte[]> ExportarInformeCSV(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var informe = await GenerarInformeGeneral(fechaInicio, fechaFin);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Informe General - Trueque Textil Digital");
            csv.AppendLine($"Fecha de generacion,{informe.FechaGeneracion:dd/MM/yyyy HH:mm}");

            if (informe.FechaInicio.HasValue && informe.FechaFin.HasValue)
            {
                csv.AppendLine($"Periodo,{informe.FechaInicio:dd/MM/yyyy} - {informe.FechaFin:dd/MM/yyyy}");
            }

            csv.AppendLine();
            csv.AppendLine("Metrica,Valor");
            csv.AppendLine($"Total Usuarios,{informe.TotalUsuarios}");
            csv.AppendLine($"Usuarios Nuevos (periodo),{informe.UsuariosNuevos}");
            csv.AppendLine($"Total Prendas,{informe.TotalPrendas}");
            csv.AppendLine($"Prendas Nuevas (periodo),{informe.PrendasNuevas}");
            csv.AppendLine($"Trueques Completados,{informe.TruequesCompletados}");
            csv.AppendLine($"Trueques Pendientes,{informe.TruequesPendientes}");
            csv.AppendLine($"Reportes Pendientes,{informe.ReportesPendientes}");
            csv.AppendLine($"Reportes Resueltos (periodo),{informe.ReportesResueltos}");
            csv.AppendLine($"Reputacion Promedio,{informe.ReputacionPromedio:F2}");

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar informe CSV");
            return Array.Empty<byte>();
        }
    }

    // Dashboard
    public async Task<DashboardStatsDTO> ObtenerEstadisticasDashboard()
    {
        try
        {
            var (totalUsuarios, totalPrendas, totalTrueques, usuariosActivos) = await _repository.ObtenerEstadisticasGenerales();
            return new DashboardStatsDTO(totalUsuarios, totalPrendas, totalTrueques, usuariosActivos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadisticas del dashboard");
            return new DashboardStatsDTO(0, 0, 0, 0);
        }
    }

    public async Task<int> ObtenerPropuestasPendientes()
    {
        try
        {
            return await _repository.ObtenerPropuestasPendientes();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propuestas pendientes");
            return 0;
        }
    }

    // RF-17: Obtener detalle completo de usuario
    public async Task<Usuario?> ObtenerDetalleUsuario(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerDetalleUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de usuario {UsuarioId}", usuarioId);
            return null;
        }
    }

    // RF-17: Cambiar rol de usuario
    public async Task<(bool Exito, string Mensaje)> CambiarRolUsuario(int usuarioId, int adminId, string nuevoRol)
    {
        try
        {
            // Validar rol
            var rolesValidos = new[] { "usuario", "admin", "administrador" };
            if (!rolesValidos.Contains(nuevoRol.ToLower()))
            {
                return (false, "Rol no valido");
            }

            var resultado = await _repository.CambiarRolUsuario(usuarioId, adminId, nuevoRol.ToLower());

            if (resultado)
            {
                _logger.LogInformation("Admin {AdminId} cambio rol de usuario {UsuarioId} a {NuevoRol}",
                    adminId, usuarioId, nuevoRol);
                return (true, $"Rol cambiado a {nuevoRol} exitosamente");
            }

            return (false, "No se pudo cambiar el rol del usuario");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar rol de usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar el cambio de rol");
        }
    }

    // RF-17: Aprobar reporte con acciones automaticas
    public async Task<(bool Exito, string Mensaje)> AprobarReporte(int reporteId, int adminId, string? comentario = null)
    {
        try
        {
            // Cambiar estado a resuelto
            var (exito, mensaje) = await CambiarEstadoReporte(reporteId, "resuelto", adminId, comentario);

            if (!exito)
            {
                return (false, mensaje);
            }

            // Ejecutar accion automatica
            var accionEjecutada = await _repository.EjecutarAccionReporte(reporteId, adminId, "aprobar");

            if (accionEjecutada)
            {
                _logger.LogInformation("Admin {AdminId} aprobo reporte {ReporteId} y ejecuto acciones automaticas",
                    adminId, reporteId);
                return (true, "Reporte aprobado y acciones ejecutadas exitosamente");
            }

            return (true, "Reporte aprobado pero no se pudieron ejecutar acciones automaticas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar reporte {ReporteId}", reporteId);
            return (false, "Error al procesar la aprobacion");
        }
    }

    // RF-17: Rechazar reporte
    public async Task<(bool Exito, string Mensaje)> RechazarReporte(int reporteId, int adminId, string? comentario = null)
    {
        try
        {
            var (exito, mensaje) = await CambiarEstadoReporte(reporteId, "desestimado", adminId, comentario);

            if (exito)
            {
                _logger.LogInformation("Admin {AdminId} rechazo reporte {ReporteId}", adminId, reporteId);
                return (true, "Reporte rechazado exitosamente");
            }

            return (false, mensaje);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar reporte {ReporteId}", reporteId);
            return (false, "Error al procesar el rechazo");
        }
    }
}
