namespace TruequeTextil.Shared.Models;

public class Region
{
    public int RegionId { get; set; }
    public string NombreRegion { get; set; } = string.Empty;
    public string CodigoRegion { get; set; } = string.Empty;
    public ICollection<Comuna> Comunas { get; set; } = new List<Comuna>();
}

public class Comuna
{
    public int ComunaId { get; set; }
    public int RegionId { get; set; }
    public string NombreComuna { get; set; } = string.Empty;
    public string CodigoComuna { get; set; } = string.Empty;
    public Region Region { get; set; } = null!;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

public class Genero
{
    public int GeneroId { get; set; }
    public string NombreGenero { get; set; } = string.Empty;
}

public class Usuario
{
    public int UsuarioId { get; set; }
    public string CorreoElectronico { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NumeroTelefono { get; set; } = string.Empty;
    public int ComunaId { get; set; }
    public string UrlFotoPerfil { get; set; } = string.Empty;
    public string Biografia { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public int? GeneroId { get; set; }
    public string PreferenciasContacto { get; set; } = string.Empty;
    public decimal ReputacionPromedio { get; set; } = 0.00m;
    public bool CuentaVerificada { get; set; } = false;
    public DateTime? FechaVerificacion { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? FechaUltimoLogin { get; set; }
    public string EstadoUsuario { get; set; } = "activo";
    public string Rol { get; set; } = "usuario";

    // Missing properties from database schema
    public string? TokenVerificacion { get; set; }
    public DateTime? TokenVerificacionExpiracion { get; set; }
    public string? TokenRecuperacion { get; set; }
    public DateTime? TokenRecuperacionExpiracion { get; set; }
    public bool? OnboardingCompletado { get; set; }
    public string? TotpSecret { get; set; }

    // Navigation properties
    public Comuna Comuna { get; set; } = null!;
    public Genero? Genero { get; set; }

    // Computed properties
    public string Iniciales => !string.IsNullOrEmpty(Nombre)
        ? string.Join("", Nombre.Split(' ')
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n[0]))
        : "";

    public double Reputacion => (double)ReputacionPromedio;
    public bool CuentaActiva => EstadoUsuario == "activo";

    // Compatibility properties needed by the codebase
    public string Telefono
    {
        get => NumeroTelefono;
        set => NumeroTelefono = value;
    }
    public int TruequesConcretados { get; set; } = 0;
    public Estadisticas Estadisticas { get; set; } = new Estadisticas();
}

public class SesionAdministrador
{
    public int SesionId { get; set; }
    public int AdministradorId { get; set; }
    public string TokenSesion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime FechaUltimaActividad { get; set; } = DateTime.Now;
    public DateTime? FechaExpiracion { get; set; }
    public bool Activa { get; set; } = true;
    public Usuario Administrador { get; set; } = null!;
}

public class Estadisticas
{
    public int PrendasPublicadas { get; set; }
    public int Valoraciones { get; set; }
}


