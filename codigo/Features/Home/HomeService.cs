using TruequeTextil.Features.Home.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Home;

public class HomeService : IHomeService
{
    private readonly IHomeRepository _repository;
    private readonly ILogger<HomeService> _logger;

    public HomeService(
        IHomeRepository repository,
        ILogger<HomeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Prenda>> ObtenerPrendasRecientes()
    {
        try
        {
            return await _repository.ObtenerPrendasRecientes();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas recientes");
            return new List<Prenda>();
        }
    }

    public async Task<List<Prenda>> ObtenerPrendasDestacadas()
    {
        try
        {
            return await _repository.ObtenerPrendasDestacadas();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas destacadas");
            return new List<Prenda>();
        }
    }

    public async Task<HomeStats> ObtenerEstadisticas()
    {
        try
        {
            var usuariosTask = _repository.ContarUsuariosActivos();
            var prendasTask = _repository.ContarPrendasDisponibles();
            var truequesTask = _repository.ContarTruequesCompletados();

            await Task.WhenAll(usuariosTask, prendasTask, truequesTask);

            return new HomeStats
            {
                UsuariosActivos = await usuariosTask,
                PrendasDisponibles = await prendasTask,
                TruequesCompletados = await truequesTask
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas del home");
            return new HomeStats();
        }
    }
}
