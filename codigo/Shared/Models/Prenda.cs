namespace TruequeTextil.Shared.Models;

// Tabla CategoriaPrenda
public class CategoriaPrenda
{
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
}

// Tabla EstadoPrenda (estado fisico)
public class EstadoPrenda
{
    public int EstadoId { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
}

// Tabla EstadoPublicacion (disponible, inactivo, en negociacion)
public class EstadoPublicacion
{
    public int EstadoPublicacionId { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
}

// Tabla Prenda
public class Prenda
{
    public int PrendaId { get; set; }
    public int UsuarioId { get; set; }
    public string TituloPublicacion { get; set; } = string.Empty;
    public string DescripcionPublicacion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string Talla { get; set; } = string.Empty;
    public int EstadoPrendaId { get; set; }
    public int EstadoPublicacionId { get; set; } = 1; // Default: disponible
    public string? UrlImagenPrincipal { get; set; }
    public DateTime FechaPublicacion { get; set; } = DateTime.Now;
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    // Navigation properties
    public Usuario? Usuario { get; set; }
    public CategoriaPrenda? Categoria { get; set; }
    public EstadoPrenda? EstadoPrendaNav { get; set; }
    public EstadoPublicacion? EstadoPublicacionNav { get; set; }
    public List<ImagenPrenda> Imagenes { get; set; } = new();

    // Computed for display
    public int PropuestasActivas { get; set; }

    // Backing fields for compatibility
    private string? _ubicacion;
    private string? _tipo;
    private string? _estado;
    private string? _imagen;
    private bool? _disponible;

    // Compatibility aliases with setters
    public int Id { get => PrendaId; set => PrendaId = value; }
    public string Titulo { get => TituloPublicacion; set => TituloPublicacion = value; }
    public string Descripcion { get => DescripcionPublicacion; set => DescripcionPublicacion = value; }

    public string Tipo
    {
        get => _tipo ?? Categoria?.NombreCategoria ?? string.Empty;
        set => _tipo = value;
    }

    public string Estado
    {
        get => _estado ?? EstadoPrendaNav?.NombreEstado ?? string.Empty;
        set => _estado = value;
    }

    public string Ubicacion
    {
        get => _ubicacion ?? Usuario?.Comuna?.NombreComuna ?? string.Empty;
        set => _ubicacion = value;
    }

    public string ImagenPrincipal => UrlImagenPrincipal
                                      ?? Imagenes.FirstOrDefault(i => i.EsPrincipal)?.ImagenUrl
                                      ?? Imagenes.FirstOrDefault()?.ImagenUrl
                                      ?? _imagen
                                      ?? string.Empty;

    public string Imagen
    {
        get => ImagenPrincipal;
        set => _imagen = value;
    }

    public bool Disponible
    {
        get => _disponible ?? EstadoPublicacionId == 1;
        set => _disponible = value;
    }

    public int Vistas { get; set; }
}

// Tabla ImagenPrenda
public class ImagenPrenda
{
    public int ImagenId { get; set; }
    public int PrendaId { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int Orden { get; set; } = 1;
    public bool EsPrincipal { get; set; } = false;
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}

// Tabla Etiqueta
public class Etiqueta
{
    public int EtiquetaId { get; set; }
    public string NombreEtiqueta { get; set; } = string.Empty;
}

// Tabla HistorialPrenda (RF-05)
public class HistorialPrenda
{
    public int HistorialId { get; set; }
    public int PrendaId { get; set; }
    public string CampoModificado { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public int UsuarioModificadorId { get; set; }
    public DateTime FechaModificacion { get; set; } = DateTime.Now;
}

// Tabla Favorito (RF-08)
public class Favorito
{
    public int FavoritoId { get; set; }
    public int UsuarioId { get; set; }
    public int PrendaId { get; set; }
    public DateTime FechaAgregado { get; set; } = DateTime.Now;
}

// Tabla BusquedaGuardada
public class BusquedaGuardada
{
    public int BusquedaId { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreBusqueda { get; set; }
    public string? CriteriosBusqueda { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaUltimaNotificacion { get; set; }
}
