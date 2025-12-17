namespace TruequeTextil.Shared.Models;

// Tabla EstadoPropuesta
public class EstadoPropuestaEntity
{
    public int EstadoPropuestaId { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
}

// Estados predefinidos (RF-11)
// 1 = Pendiente
// 2 = Aceptada (en negociacion)
// 3 = Rechazada
// 4 = Contraoferta
// 5 = Expirada
// 6 = Completada
// 7 = Cancelada

// Tabla PropuestaTrueque
public class PropuestaTrueque
{
    public int PropuestaId { get; set; }
    public int UsuarioProponenteId { get; set; }
    public string? MensajeAcompanante { get; set; }
    public int EstadoPropuestaId { get; set; } = 1; // Default: Pendiente
    public int Prioridad { get; set; } = 1;
    public bool EsContraoferta { get; set; } = false;
    public DateTime FechaPropuesta { get; set; } = DateTime.Now;
    public DateTime? FechaRespuesta { get; set; }
    public DateTime? FechaExpiracion { get; set; }

    // Navigation properties
    public Usuario? UsuarioProponente { get; set; }
    public EstadoPropuestaEntity? EstadoPropuestaNav { get; set; }
    public List<DetallePropuesta> Detalles { get; set; } = new();
    public List<MensajeNegociacion> Mensajes { get; set; } = new();

    // Backing fields for compatibility
    private Prenda? _prendaOfrecida;
    private Prenda? _prendaSolicitada;
    private int? _prendaOfrecidaId;
    private int? _prendaSolicitadaId;
    private int? _usuarioReceptorId;
    private Usuario? _usuarioReceptor;
    private EstadoPropuesta? _estado;

    // Computed from detalles with setters for compatibility
    public Prenda? PrendaOfrecida
    {
        get => _prendaOfrecida ?? Detalles.FirstOrDefault(d => d.TipoParticipacion == "ofrecida")?.Prenda;
        set => _prendaOfrecida = value;
    }

    public Prenda? PrendaSolicitada
    {
        get => _prendaSolicitada ?? Detalles.FirstOrDefault(d => d.TipoParticipacion == "solicitada")?.Prenda;
        set => _prendaSolicitada = value;
    }

    public int PrendaOfrecidaId
    {
        get => _prendaOfrecidaId ?? Detalles.FirstOrDefault(d => d.TipoParticipacion == "ofrecida")?.PrendaId ?? 0;
        set => _prendaOfrecidaId = value;
    }

    public int PrendaSolicitadaId
    {
        get => _prendaSolicitadaId ?? Detalles.FirstOrDefault(d => d.TipoParticipacion == "solicitada")?.PrendaId ?? 0;
        set => _prendaSolicitadaId = value;
    }

    // Usuario receptor es el dueno de la prenda solicitada
    public int UsuarioReceptorId
    {
        get => _usuarioReceptorId ?? PrendaSolicitada?.UsuarioId ?? 0;
        set => _usuarioReceptorId = value;
    }

    public Usuario? UsuarioReceptor
    {
        get => _usuarioReceptor ?? PrendaSolicitada?.Usuario;
        set => _usuarioReceptor = value;
    }

    // Compatibility aliases
    public int Id { get => PropuestaId; set => PropuestaId = value; }
    public string Mensaje { get => MensajeAcompanante ?? string.Empty; set => MensajeAcompanante = value; }
    public DateTime FechaCreacion { get => FechaPropuesta; set => FechaPropuesta = value; }

    public EstadoPropuesta Estado
    {
        get => _estado ?? (EstadoPropuesta)Math.Max(0, EstadoPropuestaId - 1);
        set => _estado = value;
    }

    public bool Nuevo { get; set; } = true;

    // Additional compatibility fields
    public DateTime? UltimaActualizacion { get; set; }
    public DateTime? FechaAceptacion { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public DateTime? FechaCancelacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? MensajeAceptacion { get; set; }
}

// Tabla DetallePropuesta
public class DetallePropuesta
{
    public int DetalleId { get; set; }
    public int PropuestaId { get; set; }
    public int PrendaId { get; set; }
    public string TipoParticipacion { get; set; } = string.Empty; // 'solicitada' o 'ofrecida'
    public DateTime FechaAgregado { get; set; } = DateTime.Now;

    // Navigation
    public Prenda? Prenda { get; set; }
}

// Tabla MensajeNegociacion (RF-12)
public class MensajeNegociacion
{
    public int MensajeId { get; set; }
    public int PropuestaId { get; set; }
    public int UsuarioId { get; set; }
    public string MensajeTexto { get; set; } = string.Empty;
    public string TipoMensaje { get; set; } = "texto";
    public DateTime FechaEnvio { get; set; } = DateTime.Now;
    public bool Leido { get; set; } = false;

    // Navigation
    public Usuario? Usuario { get; set; }
}

// Enum for compatibility
public enum EstadoPropuesta
{
    Pendiente = 0,
    Aceptada = 1,
    Rechazada = 2,
    Contraoferta = 3,
    Expirada = 4,
    Completada = 5,
    Cancelada = 6
}
