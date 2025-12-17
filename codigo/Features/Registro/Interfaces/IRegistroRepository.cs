namespace TruequeTextil.Features.Registro.Interfaces;

public interface IRegistroRepository
{
    Task<bool> EmailDisponible(string email);
    Task<int> CrearUsuario(string nombre, string apellido, string email, string passwordHash, int comunaId);
    Task GuardarTokenVerificacion(int usuarioId, string token, DateTime expiracion);
    Task<int?> ObtenerUsuarioIdPorEmail(string email);
}
