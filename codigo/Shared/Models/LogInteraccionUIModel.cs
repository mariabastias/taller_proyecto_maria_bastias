namespace TruequeTextil.Shared.Models;

public class LogInteraccionUIModel
{
    public int LogId { get; set; }
    public int? UsuarioId { get; set; }
    public string ElementoUi { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public DateTime? FechaInteraccion { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    // Navigation
    public Usuario? Usuario { get; set; }
}
