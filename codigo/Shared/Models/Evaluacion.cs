namespace TruequeTextil.Shared.Models;

// Tabla DimensionEvaluacion
public class DimensionEvaluacion
{
    public int DimensionId { get; set; }
    public string NombreDimension { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Peso { get; set; } = 1.00m; // Para calculo ponderado RF-14
}

// Dimensiones predefinidas:
// 1 = Comunicacion
// 2 = Estado de la prenda
// 3 = Puntualidad

// Tabla Evaluacion
public class Evaluacion
{
    public int EvaluacionId { get; set; }
    public int PropuestaId { get; set; }
    public int UsuarioEvaluadorId { get; set; }
    public int UsuarioEvaluadoId { get; set; }
    public int CalificacionGeneral { get; set; } // 1-5 estrellas
    public string? Comentario { get; set; }
    public DateTime FechaEvaluacion { get; set; } = DateTime.Now;

    // Navigation properties
    public PropuestaTrueque? Propuesta { get; set; }
    public Usuario? UsuarioEvaluador { get; set; }
    public Usuario? UsuarioEvaluado { get; set; }
    public List<EvaluacionDimensionDetalle> Dimensiones { get; set; } = new();

    // Compatibility aliases
    public int Id { get => EvaluacionId; set => EvaluacionId = value; }
    public int PropuestaTruequeId { get => PropuestaId; set => PropuestaId = value; }
    public int Puntuacion { get => CalificacionGeneral; set => CalificacionGeneral = value; }
    public DateTime FechaCreacion { get => FechaEvaluacion; set => FechaEvaluacion = value; }

    // For display compatibility
    public List<string> Aspectos => Dimensiones.Select(d => d.Dimension?.NombreDimension ?? string.Empty).ToList();
}

// Tabla EvaluacionDimension (evaluacion por dimension)
public class EvaluacionDimensionDetalle
{
    public int EvaluacionId { get; set; }
    public int DimensionId { get; set; }
    public int Calificacion { get; set; } // 1-5

    // Navigation
    public DimensionEvaluacion? Dimension { get; set; }
}

// Tabla HistorialReputacion
public class HistorialReputacion
{
    public int HistorialId { get; set; }
    public int UsuarioId { get; set; }
    public decimal? PuntuacionAnterior { get; set; }
    public decimal? PuntuacionNueva { get; set; }
    public string? MotivoCambio { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.Now;
}
