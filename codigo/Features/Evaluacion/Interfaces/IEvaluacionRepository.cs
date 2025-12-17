using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.Evaluacion.Interfaces;

public interface IEvaluacionRepository
{
    // RF-13: Crear evaluacion con dimensiones
    Task<int> CrearEvaluacion(EvaluacionModel evaluacion, List<EvaluacionDimensionDetalle> dimensiones);
    Task<bool> ExisteEvaluacion(int propuestaId, int usuarioEvaluadorId);
    Task<PropuestaTrueque?> ObtenerPropuesta(int propuestaId);

    // RF-14: Calculo ponderado de reputacion
    Task<bool> ActualizarReputacionUsuario(int usuarioId);
    Task<List<DimensionEvaluacion>> ObtenerDimensiones();

    // Consultas
    Task<List<EvaluacionModel>> ObtenerEvaluacionesUsuario(int usuarioId);
    Task<EvaluacionModel?> ObtenerEvaluacion(int propuestaId, int usuarioEvaluadorId);
    Task<(decimal Promedio, int Total)> ObtenerEstadisticasUsuario(int usuarioId);
    Task<Usuario?> ObtenerUsuarioAEvaluar(int propuestaId, int usuarioEvaluadorId);
}
