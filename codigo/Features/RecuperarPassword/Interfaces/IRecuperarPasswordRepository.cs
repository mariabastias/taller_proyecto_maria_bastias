namespace TruequeTextil.Features.RecuperarPassword.Interfaces;

public interface IRecuperarPasswordRepository
{
    Task<int?> ObtenerUsuarioIdPorEmail(string email);
    Task GuardarTokenRecuperacion(int usuarioId, string token, DateTime expiracion);
    Task<int?> ValidarTokenRecuperacion(string token);
    Task ActualizarPassword(int usuarioId, string passwordHash);
}
