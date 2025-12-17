using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class EvaluacionService
{
    // En una aplicación real, esto sería un servicio que se comunica con una API
    private readonly List<Evaluacion> _evaluaciones = new List<Evaluacion>
        {
            new Evaluacion
            {
                Id = 1,
                PropuestaTruequeId = 4,
                UsuarioEvaluadorId = 1,
                UsuarioEvaluadoId = 2,
                Puntuacion = 5,
                Comentario = "Excelente experiencia. La prenda estaba en perfecto estado y el intercambio fue muy fácil.",
                FechaCreacion = DateTime.Now.AddDays(-24)
            },
            new Evaluacion
            {
                Id = 2,
                PropuestaTruequeId = 4,
                UsuarioEvaluadorId = 2,
                UsuarioEvaluadoId = 1,
                Puntuacion = 4,
                Comentario = "Buena experiencia en general. La prenda estaba como se describió y la persona fue puntual.",
                FechaCreacion = DateTime.Now.AddDays(-24)
            },
            new Evaluacion
            {
                Id = 3,
                PropuestaTruequeId = 2,
                UsuarioEvaluadorId = 2,
                UsuarioEvaluadoId = 1,
                Puntuacion = 3,
                Comentario = "La prenda estaba en buen estado, pero la persona llegó un poco tarde al intercambio.",
                FechaCreacion = DateTime.Now.AddDays(-7)
            }
        };

    public Task<List<Evaluacion>> GetEvaluacionesAsync()
    {
        return Task.FromResult(_evaluaciones);
    }

    public Task<Evaluacion?> GetEvaluacionByIdAsync(int id)
    {
        return Task.FromResult(_evaluaciones.FirstOrDefault(e => e.Id == id));
    }

    public Task<List<Evaluacion>> GetEvaluacionesByUsuarioEvaluadoIdAsync(int usuarioId)
    {
        return Task.FromResult(_evaluaciones.Where(e => e.UsuarioEvaluadoId == usuarioId).ToList());
    }

    public Task<List<Evaluacion>> GetEvaluacionesByUsuarioEvaluadorIdAsync(int usuarioId)
    {
        return Task.FromResult(_evaluaciones.Where(e => e.UsuarioEvaluadorId == usuarioId).ToList());
    }

    public Task<List<Evaluacion>> GetEvaluacionesByPropuestaTruequeIdAsync(int propuestaId)
    {
        return Task.FromResult(_evaluaciones.Where(e => e.PropuestaTruequeId == propuestaId).ToList());
    }

    public Task<double> GetPromedioEvaluacionesByUsuarioIdAsync(int usuarioId)
    {
        var evaluaciones = _evaluaciones.Where(e => e.UsuarioEvaluadoId == usuarioId).ToList();
        if (!evaluaciones.Any())
        {
            return Task.FromResult(0.0);
        }

        return Task.FromResult(evaluaciones.Average(e => e.Puntuacion));
    }

    public async Task<Evaluacion> CreateEvaluacionAsync(Evaluacion evaluacion)
    {
        // Simular un retraso de creación
        await Task.Delay(500);

        // Verificar si ya existe una evaluación para este usuario y propuesta
        var existente = _evaluaciones.FirstOrDefault(e =>
            e.PropuestaTruequeId == evaluacion.PropuestaTruequeId &&
            e.UsuarioEvaluadorId == evaluacion.UsuarioEvaluadorId);

        if (existente != null)
        {
            // Actualizar la evaluación existente
            existente.Puntuacion = evaluacion.Puntuacion;
            existente.Comentario = evaluacion.Comentario;
            return existente;
        }

        // Asignar un ID y agregar la evaluación a la lista
        evaluacion.Id = _evaluaciones.Max(e => e.Id) + 1;
        evaluacion.FechaCreacion = DateTime.Now;
        _evaluaciones.Add(evaluacion);

        return evaluacion;
    }

    public async Task<bool> UpdateEvaluacionAsync(Evaluacion evaluacion)
    {
        // Simular un retraso
        await Task.Delay(500);

        var index = _evaluaciones.FindIndex(e => e.Id == evaluacion.Id);
        if (index == -1)
        {
            return false;
        }

        _evaluaciones[index] = evaluacion;
        return true;
    }

    public async Task<bool> DeleteEvaluacionAsync(int id)
    {
        // Simular un retraso
        await Task.Delay(500);

        var index = _evaluaciones.FindIndex(e => e.Id == id);
        if (index == -1)
        {
            return false;
        }

        _evaluaciones.RemoveAt(index);
        return true;
    }

    // Método para verificar si un usuario ya ha evaluado una propuesta de trueque
    public Task<bool> UsuarioYaEvaluoPropuestaAsync(int usuarioId, int propuestaId)
    {
        return Task.FromResult(_evaluaciones.Any(e =>
            e.PropuestaTruequeId == propuestaId &&
            e.UsuarioEvaluadorId == usuarioId));
    }

    // Método para obtener estadísticas de evaluaciones de un usuario
    public async Task<Dictionary<int, int>> GetEstadisticasEvaluacionesUsuarioAsync(int usuarioId)
    {
        var evaluaciones = _evaluaciones.Where(e => e.UsuarioEvaluadoId == usuarioId).ToList();

        var estadisticas = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 }
            };

        foreach (var evaluacion in evaluaciones)
        {
            if (estadisticas.ContainsKey(evaluacion.Puntuacion))
            {
                estadisticas[evaluacion.Puntuacion]++;
            }
        }

        return estadisticas;
    }
}
