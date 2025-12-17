namespace TruequeTextil.Shared.Models;

public class SeguimientoUsuario
{
    public int SeguimientoId { get; set; }
    public int UsuarioSeguidorId { get; set; }
    public int UsuarioSeguidoId { get; set; }
    public DateTime FechaSeguimiento { get; set; } = DateTime.Now;
    // Navigation properties
    public Usuario? UsuarioSeguidor { get; set; }
    public Usuario? UsuarioSeguido { get; set; }
}
