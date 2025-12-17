using TruequeTextil.Features.DetallePrenda.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePrenda;

public class DetallePrendaService : IDetallePrendaService
{
    private readonly IDetallePrendaRepository _repository;
    private readonly ILogger<DetallePrendaService> _logger;

    public DetallePrendaService(
        IDetallePrendaRepository repository,
        ILogger<DetallePrendaService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Prenda?> ObtenerDetallePrenda(int prendaId)
    {
        try
        {
            var prenda = await _repository.ObtenerPrendaPorId(prendaId);
            if (prenda != null)
            {
                prenda.Usuario = await _repository.ObtenerPropietario(prendaId);
            }
            return prenda;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de prenda {PrendaId}", prendaId);
            return null;
        }
    }


    public async Task<List<string>> ObtenerImagenesPrenda(int prendaId)
    {
        try
        {
            return await _repository.ObtenerImagenesPrenda(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagenes de prenda {PrendaId}", prendaId);
            return new List<string>();
        }
    }

    public async Task<Usuario?> ObtenerPropietario(int prendaId)
    {
        try
        {
            return await _repository.ObtenerPropietario(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener propietario de prenda {PrendaId}", prendaId);
            return null;
        }
    }

    public async Task RegistrarVista(int prendaId)
    {
        try
        {
            await _repository.IncrementarVistas(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar vista de prenda {PrendaId}", prendaId);
        }
    }

    public async Task<List<Prenda>> ObtenerPrendasSimilares(int prendaId)
    {
        try
        {
            var prenda = await _repository.ObtenerPrendaPorId(prendaId);
            if (prenda == null)
            {
                return new List<Prenda>();
            }

            return await _repository.ObtenerPrendasSimilares(prendaId, prenda.Tipo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas similares a {PrendaId}", prendaId);
            return new List<Prenda>();
        }
    }
}
