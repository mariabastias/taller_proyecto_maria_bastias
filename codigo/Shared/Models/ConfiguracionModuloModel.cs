namespace TruequeTextil.Shared.Models;

public class ConfiguracionModuloModel
{
    public int ConfigModuloId { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string? ConfiguracionJson { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    // Navigation
    public Usuario? Usuario { get; set; }
}
