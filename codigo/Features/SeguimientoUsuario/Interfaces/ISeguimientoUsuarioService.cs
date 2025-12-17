namespace TruequeTextil.Features.SeguimientoUsuario.Interfaces;

public interface ISeguimientoUsuarioService
{
    Task<(bool Success, string? Error)> SeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId);
    Task<(bool Success, string? Error)> DejarDeSeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId);
    Task<bool> EstaSiguiendo(int usuarioSeguidorId, int usuarioSeguidoId);
    Task<List<int>> ObtenerSeguidores(int usuarioId);
    Task<List<int>> ObtenerSeguidos(int usuarioId);
    Task<int> ContarSeguidores(int usuarioId);
    Task<int> ContarSeguidos(int usuarioId);
}
