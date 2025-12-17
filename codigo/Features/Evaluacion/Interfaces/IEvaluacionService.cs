using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.Evaluacion.Interfaces;

// DTO para evaluacion con dimensiones
public record EvaluacionDTO(
    int CalificacionGeneral,
    string? Comentario,
    Dictionary<int, int> CalificacionesPorDimension // DimensionId -> Calificacion (1-5)
);

public interface IEvaluacionService
{
    // RF-13: Crear evaluacion con dimensiones
    Task<(bool Exito, string Mensaje)> CrearEvaluacion(int propuestaId, int usuarioEvaluadorId, EvaluacionDTO evaluacion);

    Task<bool> PuedeEvaluar(int propuestaId, int usuarioId);
    Task<List<DimensionEvaluacion>> ObtenerDimensiones();

    // Consultas
    Task<List<EvaluacionModel>> ObtenerEvaluacionesRecibidas(int usuarioId);
    Task<(decimal Promedio, int Total)> ObtenerEstadisticasUsuario(int usuarioId);
    Task<EvaluacionModel?> ObtenerMiEvaluacion(int propuestaId, int usuarioId);
    Task<Usuario?> ObtenerUsuarioAEvaluar(int propuestaId, int usuarioEvaluadorId);
}
