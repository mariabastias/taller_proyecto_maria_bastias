namespace TruequeTextil.Shared.Models;

// Tabla CategoriaReporte
public class CategoriaReporte
{
    public int CategoriaReporteId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

// Tabla Reporte (RF-15)
public class Reporte
{
    public int ReporteId { get; set; }
    public int UsuarioReportadorId { get; set; }
    public int? UsuarioReportadoId { get; set; }
    public int? PrendaReportadaId { get; set; }
    public int CategoriaReporteId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? EvidenciaUrl { get; set; }
    public string EstadoReporteStr { get; set; } = "pendiente"; // pendiente, en_revision, resuelto, descartado
    public DateTime FechaReporte { get; set; } = DateTime.Now;
    public DateTime? FechaResolucion { get; set; }
    public int? AdministradorId { get; set; }
    public string? ComentarioResolucion { get; set; }

    // Navigation properties
    public Usuario? UsuarioReportador { get; set; }
    public Usuario? UsuarioReportado { get; set; }
    public Prenda? PrendaReportada { get; set; }
    public CategoriaReporte? Categoria { get; set; }
    public Usuario? Administrador { get; set; }

    // Backing fields for compatibility
    private string? _motivo;
    private EstadoReporte? _estado;

    // Compatibility aliases
    public int Id { get => ReporteId; set => ReporteId = value; }
    public int UsuarioReportanteId { get => UsuarioReportadorId; set => UsuarioReportadorId = value; }
    public Usuario? UsuarioReportante { get => UsuarioReportador; set => UsuarioReportador = value; }

    public TipoReporte Tipo { get; set; } = TipoReporte.Prenda;

    public string Motivo
    {
        get => _motivo ?? Categoria?.NombreCategoria ?? string.Empty;
        set => _motivo = value;
    }

    public DateTime FechaCreacion { get => FechaReporte; set => FechaReporte = value; }

    public EstadoReporte Estado
    {
        get => _estado ?? EstadoReporteStr switch
        {
            "pendiente" => EstadoReporte.Pendiente,
            "en_revision" => EstadoReporte.Revisado,
            "resuelto" => EstadoReporte.Resuelto,
            "descartado" => EstadoReporte.Desestimado,
            _ => EstadoReporte.Pendiente
        };
        set => _estado = value;
    }
}

public enum TipoReporte
{
    Usuario,
    Prenda,
    Propuesta
}

// Keep old EstadoReporte name for backwards compatibility
public enum EstadoReporte
{
    Pendiente,
    Revisado,
    Resuelto,
    Desestimado
}
