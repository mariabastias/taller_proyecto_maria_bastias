using TruequeTextil.Features.Favoritos.Interfaces;
using TruequeTextil.Features.Explorar.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Favoritos;

/// <summary>
/// Service for favorites business logic (RF-08)
/// </summary>
public class FavoritosService : IFavoritosService
{
    private readonly IFavoritosRepository _repository;
    private readonly ILogger<FavoritosService> _logger;

    private const int ITEMS_POR_PAGINA = 20; // RNF-01

    public FavoritosService(
        IFavoritosRepository repository,
        ILogger<FavoritosService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaginacionResultado<Prenda>> ObtenerFavoritos(int usuarioId, int pagina = 1)
    {
        try
        {
            if (pagina < 1) pagina = 1;

            var (prendas, totalCount) = await _repository.ObtenerFavoritos(usuarioId, pagina, ITEMS_POR_PAGINA);

            var totalPaginas = (int)Math.Ceiling((double)totalCount / ITEMS_POR_PAGINA);

            return new PaginacionResultado<Prenda>(
                Items: prendas,
                TotalCount: totalCount,
                PaginaActual: pagina,
                TotalPaginas: totalPaginas,
                TienePaginaAnterior: pagina > 1,
                TienePaginaSiguiente: pagina < totalPaginas
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener favoritos para usuario {UsuarioId}, pagina {Pagina}",
                usuarioId, pagina);

            return new PaginacionResultado<Prenda>(
                Items: new List<Prenda>(),
                TotalCount: 0,
                PaginaActual: 1,
                TotalPaginas: 0,
                TienePaginaAnterior: false,
                TienePaginaSiguiente: false
            );
        }
    }

    public async Task<bool> EsFavorito(int usuarioId, int prendaId)
    {
        try
        {
            return await _repository.EsFavorito(usuarioId, prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar favorito: usuario {UsuarioId}, prenda {PrendaId}",
                usuarioId, prendaId);
            return false;
        }
    }

    public async Task<HashSet<int>> ObtenerIdsFavoritos(int usuarioId)
    {
        try
        {
            var ids = await _repository.ObtenerIdsFavoritos(usuarioId);
            return new HashSet<int>(ids);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener IDs de favoritos para usuario {UsuarioId}", usuarioId);
            return new HashSet<int>();
        }
    }

    public async Task<bool> ToggleFavorito(int usuarioId, int prendaId)
    {
        try
        {
            var esFavorito = await _repository.EsFavorito(usuarioId, prendaId);

            if (esFavorito)
            {
                await _repository.QuitarFavorito(usuarioId, prendaId);
                return false; // Ya no es favorito
            }
            else
            {
                await _repository.AgregarFavorito(usuarioId, prendaId);
                return true; // Ahora es favorito
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al toggle favorito: usuario {UsuarioId}, prenda {PrendaId}",
                usuarioId, prendaId);
            throw;
        }
    }

    public async Task<bool> AgregarFavorito(int usuarioId, int prendaId)
    {
        try
        {
            return await _repository.AgregarFavorito(usuarioId, prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar favorito: usuario {UsuarioId}, prenda {PrendaId}",
                usuarioId, prendaId);
            return false;
        }
    }

    public async Task<bool> QuitarFavorito(int usuarioId, int prendaId)
    {
        try
        {
            return await _repository.QuitarFavorito(usuarioId, prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al quitar favorito: usuario {UsuarioId}, prenda {PrendaId}",
                usuarioId, prendaId);
            return false;
        }
    }

    public async Task<int> ContarFavoritos(int usuarioId)
    {
        try
        {
            return await _repository.ContarFavoritos(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar favoritos para usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }
}
