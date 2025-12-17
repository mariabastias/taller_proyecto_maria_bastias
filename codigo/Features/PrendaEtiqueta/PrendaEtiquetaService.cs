using TruequeTextil.Features.PrendaEtiqueta.Interfaces;

namespace TruequeTextil.Features.PrendaEtiqueta;

public class PrendaEtiquetaService : IPrendaEtiquetaService
{
    private readonly IPrendaEtiquetaRepository _repository;
    private readonly ILogger<PrendaEtiquetaService> _logger;

    public PrendaEtiquetaService(IPrendaEtiquetaRepository repository, ILogger<PrendaEtiquetaService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> AgregarEtiquetaAPrenda(int prendaId, int etiquetaId)
    {
        if (prendaId <= 0 || etiquetaId <= 0)
            return (false, "IDs inválidos");

        // Verificar si ya tiene la etiqueta
        if (await _repository.PrendaTieneEtiqueta(prendaId, etiquetaId))
            return (false, "La prenda ya tiene esta etiqueta");

        try
        {
            var success = await _repository.AgregarEtiquetaAPrenda(prendaId, etiquetaId);
            if (success)
            {
                _logger.LogInformation("Etiqueta {EtiquetaId} agregada a prenda {PrendaId}", etiquetaId, prendaId);
                return (true, null);
            }
            else
            {
                return (false, "Error al agregar la etiqueta");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar etiqueta {EtiquetaId} a prenda {PrendaId}", etiquetaId, prendaId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> RemoverEtiquetaDePrenda(int prendaId, int etiquetaId)
    {
        if (prendaId <= 0 || etiquetaId <= 0)
            return (false, "IDs inválidos");

        // Verificar si tiene la etiqueta
        if (!await _repository.PrendaTieneEtiqueta(prendaId, etiquetaId))
            return (false, "La prenda no tiene esta etiqueta");

        try
        {
            var success = await _repository.RemoverEtiquetaDePrenda(prendaId, etiquetaId);
            if (success)
            {
                _logger.LogInformation("Etiqueta {EtiquetaId} removida de prenda {PrendaId}", etiquetaId, prendaId);
                return (true, null);
            }
            else
            {
                return (false, "Error al remover la etiqueta");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover etiqueta {EtiquetaId} de prenda {PrendaId}", etiquetaId, prendaId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<List<int>> ObtenerEtiquetasDePrenda(int prendaId)
    {
        if (prendaId <= 0)
            return new List<int>();

        try
        {
            return await _repository.ObtenerEtiquetasDePrenda(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener etiquetas de prenda {PrendaId}", prendaId);
            return new List<int>();
        }
    }

    public async Task<List<int>> ObtenerPrendasConEtiqueta(int etiquetaId)
    {
        if (etiquetaId <= 0)
            return new List<int>();

        try
        {
            return await _repository.ObtenerPrendasConEtiqueta(etiquetaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas con etiqueta {EtiquetaId}", etiquetaId);
            return new List<int>();
        }
    }

    public async Task<bool> PrendaTieneEtiqueta(int prendaId, int etiquetaId)
    {
        if (prendaId <= 0 || etiquetaId <= 0)
            return false;

        try
        {
            return await _repository.PrendaTieneEtiqueta(prendaId, etiquetaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar etiqueta {EtiquetaId} en prenda {PrendaId}", etiquetaId, prendaId);
            return false;
        }
    }

    public async Task<int> ContarEtiquetasDePrenda(int prendaId)
    {
        if (prendaId <= 0)
            return 0;

        try
        {
            return await _repository.ContarEtiquetasDePrenda(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar etiquetas de prenda {PrendaId}", prendaId);
            return 0;
        }
    }

    public async Task<int> ContarPrendasConEtiqueta(int etiquetaId)
    {
        if (etiquetaId <= 0)
            return 0;

        try
        {
            return await _repository.ContarPrendasConEtiqueta(etiquetaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar prendas con etiqueta {EtiquetaId}", etiquetaId);
            return 0;
        }
    }

    public async Task<(bool Success, string? Error)> GestionarEtiquetasDePrenda(int prendaId, List<int> etiquetasIds)
    {
        if (prendaId <= 0)
            return (false, "ID de prenda inválido");

        if (etiquetasIds == null)
            etiquetasIds = new List<int>();

        try
        {
            // Obtener etiquetas actuales
            var etiquetasActuales = await _repository.ObtenerEtiquetasDePrenda(prendaId);

            // Determinar qué agregar y qué remover
            var etiquetasAAgregar = etiquetasIds.Except(etiquetasActuales).ToList();
            var etiquetasARemover = etiquetasActuales.Except(etiquetasIds).ToList();

            // Agregar nuevas etiquetas
            foreach (var etiquetaId in etiquetasAAgregar)
            {
                await _repository.AgregarEtiquetaAPrenda(prendaId, etiquetaId);
            }

            // Remover etiquetas no deseadas
            foreach (var etiquetaId in etiquetasARemover)
            {
                await _repository.RemoverEtiquetaDePrenda(prendaId, etiquetaId);
            }

            _logger.LogInformation("Etiquetas gestionadas para prenda {PrendaId}: +{Agregadas} -{Removidas}",
                prendaId, etiquetasAAgregar.Count, etiquetasARemover.Count);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al gestionar etiquetas de prenda {PrendaId}", prendaId);
            return (false, "Error interno del servidor");
        }
    }
}
