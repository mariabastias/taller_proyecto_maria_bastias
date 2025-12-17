namespace TruequeTextil.Shared.Models;

public class ConfiguracionInterfazModel
{
    public int ConfiguracionId { get; set; }
    public int UsuarioId { get; set; }
    public bool TemaOscuro { get; set; } = false;
    public bool NotificacionesSonido { get; set; } = true;
    public string? DensidadContenido { get; set; }
    public string? TamanioFuente { get; set; }
    public string? Idioma { get; set; }
    // Navigation
    public Usuario? Usuario { get; set; }
}
