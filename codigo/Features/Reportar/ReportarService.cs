using TruequeTextil.Features.Reportar.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Reportar;

public class ReportarService : IReportarService
{
    private readonly IReportarRepository _repository;
    private readonly ILogger<ReportarService> _logger;

    public ReportarService(
        IReportarRepository repository,
        ILogger<ReportarService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool Exito, string Mensaje)> ReportarUsuario(int usuarioReportadorId, int usuarioReportadoId, string motivo, string? descripcion = null)
    {
        try
        {
            // Validar que no se reporte a sí mismo
            if (usuarioReportadorId == usuarioReportadoId)
            {
                return (false, "No puedes reportarte a ti mismo");
            }

            // Validar que el usuario reportado exista
            var usuarioReportado = await _repository.ObtenerUsuarioPorId(usuarioReportadoId);
            if (usuarioReportado == null)
            {
                return (false, "El usuario que intentas reportar no existe");
            }

            // Verificar si ya existe un reporte activo
            var existeReporte = await _repository.ExisteReporteActivo(usuarioReportadorId, usuarioReportadoId, null);
            if (existeReporte)
            {
                return (false, "Ya tienes un reporte activo para este usuario. Espera a que sea revisado.");
            }

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return (false, "Debes seleccionar un motivo para el reporte");
            }

            var reporte = new Reporte
            {
                Tipo = TipoReporte.Usuario,
                Motivo = motivo,
                Descripcion = descripcion?.Trim() ?? string.Empty,
                UsuarioReportanteId = usuarioReportadorId,
                UsuarioReportadoId = usuarioReportadoId,
                FechaCreacion = DateTime.Now,
                Estado = EstadoReporte.Pendiente
            };

            var reporteId = await _repository.CrearReporte(reporte);

            if (reporteId > 0)
            {
                _logger.LogInformation("Reporte {ReporteId} de usuario creado: usuario {Reportador} reportó a {Reportado} por {Motivo}",
                    reporteId, usuarioReportadorId, usuarioReportadoId, motivo);
                return (true, "Reporte enviado exitosamente. Nuestro equipo lo revisará pronto.");
            }

            return (false, "No se pudo crear el reporte. Intenta nuevamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reporte de usuario {UsuarioReportadoId} por usuario {UsuarioReportadorId}",
                usuarioReportadoId, usuarioReportadorId);
            return (false, "Ocurrió un error al procesar el reporte. Intenta nuevamente.");
        }
    }

    public async Task<(bool Exito, string Mensaje)> ReportarPrenda(int usuarioReportadorId, int prendaId, string motivo, string? descripcion = null)
    {
        try
        {
            // Validar que la prenda exista
            var prenda = await _repository.ObtenerPrendaPorId(prendaId);
            if (prenda == null)
            {
                return (false, "La prenda que intentas reportar no existe");
            }

            // Validar que no reporte su propia prenda
            if (prenda.UsuarioId == usuarioReportadorId)
            {
                return (false, "No puedes reportar tu propia prenda");
            }

            // Verificar si ya existe un reporte activo
            var existeReporte = await _repository.ExisteReporteActivo(usuarioReportadorId, null, prendaId);
            if (existeReporte)
            {
                return (false, "Ya tienes un reporte activo para esta prenda. Espera a que sea revisado.");
            }

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return (false, "Debes seleccionar un motivo para el reporte");
            }

            var reporte = new Reporte
            {
                Tipo = TipoReporte.Prenda,
                Motivo = motivo,
                Descripcion = descripcion?.Trim() ?? string.Empty,
                UsuarioReportanteId = usuarioReportadorId,
                PrendaReportadaId = prendaId,
                FechaCreacion = DateTime.Now,
                Estado = EstadoReporte.Pendiente
            };

            var reporteId = await _repository.CrearReporte(reporte);

            if (reporteId > 0)
            {
                _logger.LogInformation("Reporte {ReporteId} de prenda creado: usuario {Reportador} reportó prenda {PrendaId} por {Motivo}",
                    reporteId, usuarioReportadorId, prendaId, motivo);
                return (true, "Reporte enviado exitosamente. Nuestro equipo lo revisará pronto.");
            }

            return (false, "No se pudo crear el reporte. Intenta nuevamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reporte de prenda {PrendaId} por usuario {UsuarioReportadorId}",
                prendaId, usuarioReportadorId);
            return (false, "Ocurrió un error al procesar el reporte. Intenta nuevamente.");
        }
    }

    public async Task<List<CategoriaReporteDTO>> ObtenerCategoriasReporte(TipoReporte tipo)
    {
        return await _repository.ObtenerCategoriasReporte(tipo);
    }

    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        return await _repository.ObtenerUsuarioPorId(usuarioId);
    }

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        return await _repository.ObtenerPrendaPorId(prendaId);
    }

    public async Task<List<Reporte>> ObtenerMisReportes(int usuarioId)
    {
        return await _repository.ObtenerMisReportes(usuarioId);
    }

    public async Task<bool> PuedeReportar(int usuarioReportadorId, TipoReporte tipo, int elementoId)
    {
        try
        {
            if (tipo == TipoReporte.Usuario)
            {
                // No puede reportarse a sí mismo
                if (usuarioReportadorId == elementoId)
                    return false;

                // Verificar si el usuario existe
                var usuario = await _repository.ObtenerUsuarioPorId(elementoId);
                if (usuario == null)
                    return false;

                // Verificar si ya hay reporte activo
                return !await _repository.ExisteReporteActivo(usuarioReportadorId, elementoId, null);
            }
            else if (tipo == TipoReporte.Prenda)
            {
                // Verificar si la prenda existe
                var prenda = await _repository.ObtenerPrendaPorId(elementoId);
                if (prenda == null)
                    return false;

                // No puede reportar su propia prenda
                if (prenda.UsuarioId == usuarioReportadorId)
                    return false;

                // Verificar si ya hay reporte activo
                return !await _repository.ExisteReporteActivo(usuarioReportadorId, null, elementoId);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si usuario {UsuarioId} puede reportar {Tipo} {ElementoId}",
                usuarioReportadorId, tipo, elementoId);
            return false;
        }
    }
}
