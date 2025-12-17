using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Favoritos.Interfaces;

/// <summary>
/// Repository interface for favorites data access (RF-08)
/// </summary>
public interface IFavoritosRepository
{
    /// <summary>
    /// Gets paginated list of favorite items for a user
    /// </summary>
    Task<(List<Prenda> Prendas, int TotalCount)> ObtenerFavoritos(int usuarioId, int pagina, int itemsPorPagina);

    /// <summary>
    /// Checks if a specific item is in user's favorites
    /// </summary>
    Task<bool> EsFavorito(int usuarioId, int prendaId);

    /// <summary>
    /// Gets all favorite item IDs for a user (for bulk checking)
    /// </summary>
    Task<List<int>> ObtenerIdsFavoritos(int usuarioId);

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
