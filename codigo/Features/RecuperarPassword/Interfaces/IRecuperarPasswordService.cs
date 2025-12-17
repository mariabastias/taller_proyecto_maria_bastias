namespace TruequeTextil.Features.RecuperarPassword.Interfaces;

public interface IRecuperarPasswordService
{
    Task<(bool Success, string? Token)> EnviarEmailRecuperacion(string email);
    Task<int?> ValidarTokenRecuperacion(string token);
    Task<bool> RestablecerPassword(string token, string nuevaPassword);
}
