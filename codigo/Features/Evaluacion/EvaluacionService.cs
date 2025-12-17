using TruequeTextil.Features.Evaluacion.Interfaces;
using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.Evaluacion;

public class EvaluacionService : IEvaluacionService
{
    private readonly IEvaluacionRepository _repository;
    private readonly ILogger<EvaluacionService> _logger;

    public EvaluacionService(
        IEvaluacionRepository repository,
        ILogger<EvaluacionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // RF-13: Crear evaluacion con dimensiones
    public async Task<(bool Exito, string Mensaje)> CrearEvaluacion(int propuestaId, int usuarioEvaluadorId, EvaluacionDTO evaluacionDTO)
    {
        try
        {
            // Validar calificacion general
            if (evaluacionDTO.CalificacionGeneral < 1 || evaluacionDTO.CalificacionGeneral > 5)
            {
                return (false, "La calificacion general debe estar entre 1 y 5");
            }

            // Validar calificaciones por dimension
            foreach (var (dimensionId, calificacion) in evaluacionDTO.CalificacionesPorDimension)
            {
                if (calificacion < 1 || calificacion > 5)
                {
                    return (false, "Las calificaciones por dimension deben estar entre 1 y 5");
                }
            }

            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null)
            {
                return (false, "Propuesta no encontrada");
            }

            // Solo trueques completados (estado_propuesta_id = 6)
            if (propuesta.EstadoPropuestaId != 6)
            {
                return (false, "Solo se pueden evaluar trueques completados");
            }

            // Verificar participacion
            if (propuesta.UsuarioProponenteId != usuarioEvaluadorId && propuesta.UsuarioReceptorId != usuarioEvaluadorId)
            {
                return (false, "No participaste en este trueque");
            }

            // Verificar que no haya evaluado antes
            var existeEvaluacion = await _repository.ExisteEvaluacion(propuestaId, usuarioEvaluadorId);
            if (existeEvaluacion)
            {
                return (false, "Ya has evaluado este trueque");
            }

            // Determinar usuario evaluado (la contraparte)
            var usuarioEvaluadoId = propuesta.UsuarioProponenteId == usuarioEvaluadorId
                ? propuesta.UsuarioReceptorId
                : propuesta.UsuarioProponenteId;

            // Crear evaluacion
            var evaluacion = new EvaluacionModel
            {
                PropuestaId = propuestaId,
                UsuarioEvaluadorId = usuarioEvaluadorId,
                UsuarioEvaluadoId = usuarioEvaluadoId,
                CalificacionGeneral = evaluacionDTO.CalificacionGeneral,
                Comentario = evaluacionDTO.Comentario,
                FechaEvaluacion = DateTime.Now
            };

            // RF-13: Crear detalles por dimension
            var dimensiones = evaluacionDTO.CalificacionesPorDimension
                .Select(kvp => new EvaluacionDimensionDetalle
                {
                    DimensionId = kvp.Key,
                    Calificacion = kvp.Value
                })
                .ToList();

            var evaluacionId = await _repository.CrearEvaluacion(evaluacion, dimensiones);

            if (evaluacionId > 0)
            {
                // RF-14: Actualizar reputacion con calculo ponderado
                await _repository.ActualizarReputacionUsuario(usuarioEvaluadoId);

                _logger.LogInformation(
                    "Evaluacion {EvaluacionId} creada: usuario {EvaluadorId} evaluo a {EvaluadoId} con {Calificacion} estrellas y {NumDimensiones} dimensiones",
                    evaluacionId, usuarioEvaluadorId, usuarioEvaluadoId, evaluacionDTO.CalificacionGeneral, dimensiones.Count);

                return (true, "Evaluacion enviada exitosamente. Gracias por tu opinion.");
            }

            return (false, "No se pudo crear la evaluacion");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear evaluacion para propuesta {PropuestaId}", propuestaId);
            return (false, "Error al procesar la evaluacion");
        }
    }

    public async Task<bool> PuedeEvaluar(int propuestaId, int usuarioId)
    {
        try
        {
            var propuesta = await _repository.ObtenerPropuesta(propuestaId);

            if (propuesta == null || propuesta.EstadoPropuestaId != 6) // Completada
            {
                return false;
            }

            if (propuesta.UsuarioProponenteId != usuarioId && propuesta.UsuarioReceptorId != usuarioId)
            {
                return false;
            }

            var existeEvaluacion = await _repository.ExisteEvaluacion(propuestaId, usuarioId);
            return !existeEvaluacion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si usuario {UsuarioId} puede evaluar propuesta {PropuestaId}",
                usuarioId, propuestaId);
            return false;
        }
    }

    public async Task<List<DimensionEvaluacion>> ObtenerDimensiones()
    {
        try
        {
            return await _repository.ObtenerDimensiones();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dimensiones de evaluacion");
            // Retornar lista vacia - NO inventar IDs que no existen en DimensionEvaluacion
            // ya que causaria FK violation al insertar en EvaluacionDimension
            return new List<DimensionEvaluacion>();
        }
    }

    public async Task<List<EvaluacionModel>> ObtenerEvaluacionesRecibidas(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerEvaluacionesUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener evaluaciones del usuario {UsuarioId}", usuarioId);
            return new List<EvaluacionModel>();
        }
    }

    public async Task<(decimal Promedio, int Total)> ObtenerEstadisticasUsuario(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerEstadisticasUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadisticas del usuario {UsuarioId}", usuarioId);
            return (0, 0);
        }
    }

    public async Task<EvaluacionModel?> ObtenerMiEvaluacion(int propuestaId, int usuarioId)
    {
        try
        {
            return await _repository.ObtenerEvaluacion(propuestaId, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener evaluacion del usuario {UsuarioId} para propuesta {PropuestaId}",
                usuarioId, propuestaId);
            return null;
        }
    }

    public async Task<Usuario?> ObtenerUsuarioAEvaluar(int propuestaId, int usuarioEvaluadorId)
    {
        try
        {
            return await _repository.ObtenerUsuarioAEvaluar(propuestaId, usuarioEvaluadorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario a evaluar para propuesta {PropuestaId}",
                propuestaId);
            return null;
        }
    }
}
