using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPerfil.Interfaces;

public class EditarPerfilDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public int ComunaId { get; set; }
    public string UrlFotoPerfil { get; set; } = string.Empty;
    public string Biografia { get; set; } = string.Empty;
}

public interface IEditarPerfilService
{
    Task<Usuario?> GetCurrentUser();
    Task<Usuario?> ObtenerUsuarioPorId(int usuarioId);
    Task<List<Region>> ObtenerRegiones();
    Task<List<Comuna>> ObtenerComunasPorRegion(int regionId);
    Task<(bool Success, string Message)> ActualizarPerfil(int usuarioId, EditarPerfilDto datos);
}
