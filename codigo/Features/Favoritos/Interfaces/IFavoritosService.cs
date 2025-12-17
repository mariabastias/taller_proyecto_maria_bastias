using TruequeTextil.Shared.Models;
using TruequeTextil.Features.Explorar.Interfaces;

namespace TruequeTextil.Features.Favoritos.Interfaces;

/// <summary>
/// Service interface for favorites business logic (RF-08)
/// </summary>
public interface IFavoritosService
{
    /// <summary>
    /// Gets paginated list of user's favorite items
    /// </summary>
    Task<PaginacionResultado<Prenda>> ObtenerFavoritos(int usuarioId, int pagina = 1);

    /// <summary>
    /// Checks if a specific item is in user's favorites
    /// </summary>
    Task<bool> EsFavorito(int usuarioId, int prendaId);

    /// <summary>
    /// Gets all favorite item IDs for a user (for efficient bulk checking)
    /// </summary>
    Task<HashSet<int>> ObtenerIdsFavoritos(int usuarioId);

    /// <summary>
    /// Toggles favorite status - adds if not favorite, removes if favorite
    /// </summary>
    Task<bool> ToggleFavorito(int usuarioId, int prendaId);

    /// <summary>
    /// Adds an item to user's favorites
    /// </summary>
    Task<bool> AgregarFavorito(int usuarioId, int prendaId);

    /// <summary>
    /// Removes an item from user's favorites
    /// </summary>
    Task<bool> QuitarFavorito(int usuarioId, int prendaId);

    /// <summary>
    /// Gets the count of favorites for a user
    /// </summary>
    Task<int> ContarFavoritos(int usuarioId);
}
