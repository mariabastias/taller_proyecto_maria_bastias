namespace TruequeTextil.Shared.Models;

public class PrendaEtiqueta
{
    public int PrendaId { get; set; }
    public int EtiquetaId { get; set; }
    // Navigation
    public Prenda? Prenda { get; set; }
    public Etiqueta? Etiqueta { get; set; }
}
