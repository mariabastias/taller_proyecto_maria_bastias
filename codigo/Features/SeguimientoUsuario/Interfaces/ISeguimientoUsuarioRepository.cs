namespace TruequeTextil.Features.SeguimientoUsuario.Interfaces;

public interface ISeguimientoUsuarioRepository
{
    Task<bool> SeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId);
    Task<bool> DejarDeSeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId);
    Task<bool> EstaSiguiendo(int usuarioSeguidorId, int usuarioSeguidoId);
    Task<List<int>> ObtenerSeguidores(int usuarioId);
    Task<List<int>> ObtenerSeguidos(int usuarioId);
    Task<int> ContarSeguidores(int usuarioId);
    Task<int> ContarSeguidos(int usuarioId);
}
