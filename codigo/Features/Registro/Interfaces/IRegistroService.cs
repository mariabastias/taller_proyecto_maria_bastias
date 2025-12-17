namespace TruequeTextil.Features.Registro.Interfaces;

public interface IRegistroService
{
    Task<(bool Success, string? Error)> RegistrarUsuario(
        string nombre,
        string apellido,
        string email,
        string password,
        string confirmPassword,
        int comunaId);

    bool ValidarFormatoEmail(string email);
    (bool Valid, List<string> Errors) ValidarPassword(string password, string confirmPassword);
}
