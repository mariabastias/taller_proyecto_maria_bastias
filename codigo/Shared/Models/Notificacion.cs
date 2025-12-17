namespace TruequeTextil.Shared.Models;

// Tabla TipoNotificacion
public class TipoNotificacionEntity
{
    public int TipoNotificacionId { get; set; }
    public string NombreTipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

// Tabla Notificacion
public class Notificacion
{
    public int NotificacionId { get; set; }
    public int UsuarioId { get; set; }
    public int TipoNotificacionId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Enlace { get; set; }
    public bool Leida { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaEnvio { get; set; }
    public string MetodoEnvio { get; set; } = "push";

    // Navigation properties
    public Usuario? Usuario { get; set; }
    public TipoNotificacionEntity? TipoNotificacionNav { get; set; }

    // Backing fields for UI helpers
    private TipoNotificacion? _tipo;
    private string? _iconoNombre;
    private string? _iconoColor;
    private string? _bgColor;

    // Compatibility aliases
    public int Id { get => NotificacionId; set => NotificacionId = value; }
    public DateTime Fecha { get => FechaCreacion; set => FechaCreacion = value; }
    public string? ReferenciaUrl { get => Enlace; set => Enlace = value; }
    public int? ReferenciaId { get; set; }

    public TipoNotificacion Tipo
    {
        // El enum ahora está alineado 1:1 con los IDs de la BD
        get => _tipo ?? (TipoNotificacion)TipoNotificacionId;
        set { _tipo = value; TipoNotificacionId = (int)value; }
    }

    // UI helpers with setters - Alineados con tipos reales en BD
    public string IconoNombre
    {
        get => _iconoNombre ?? TipoNotificacionId switch
        {
            1 => "bi-send",           // Nueva propuesta
            2 => "bi-check-circle",   // Propuesta aceptada
            3 => "bi-person-plus",    // Nuevo seguidor
            4 => "bi-search",         // Búsqueda encontrada
            5 => "bi-x-circle",       // Propuesta rechazada
            6 => "bi-chat-dots",      // Mensaje nuevo
            7 => "bi-star",           // Evaluación recibida
            8 => "bi-info-circle",    // Sistema
            _ => "bi-bell"
        };
        set => _iconoNombre = value;
    }

    public string IconoColor
    {
        get => _iconoColor ?? TipoNotificacionId switch
        {
            1 => "text-primary",      // Nueva propuesta
            2 => "text-success",      // Propuesta aceptada
            3 => "text-info",         // Nuevo seguidor
            4 => "text-warning",      // Búsqueda encontrada
            5 => "text-danger",       // Propuesta rechazada
            6 => "text-primary",      // Mensaje nuevo
            7 => "text-warning",      // Evaluación recibida
            8 => "text-secondary",    // Sistema
            _ => "text-stone-600"
        };
        set => _iconoColor = value;
    }

    public string BgColor
    {
        get => _bgColor ?? (Leida ? "bg-white" : "bg-amber-50");
        set => _bgColor = value;
    }

    public string FechaFormateada => FechaCreacion.ToString("dd/MM/yyyy HH:mm");
}

// Tabla PreferenciaNotificacion
public class PreferenciaNotificacion
{
    public int PreferenciaId { get; set; }
    public int UsuarioId { get; set; }
    public bool NotifNuevasPropuestas { get; set; } = true;
    public bool NotifRespuestasPropuestas { get; set; } = true;
    public bool NotifTruequesCompletados { get; set; } = true;
    public bool NotifNuevosSeguidores { get; set; } = true;
    public bool NotifActividadSeguidos { get; set; } = true;
    public bool NotifPorEmail { get; set; } = true;
    public bool NotifPorPush { get; set; } = true;
    public DateTime? SilenciadoHasta { get; set; }
}

// Enum alineado con tabla TipoNotificacion en BD
// IMPORTANTE: Los valores deben coincidir con tipo_notificacion_id en la BD
public enum TipoNotificacion
{
    NuevaPropuesta = 1,       // BD: "Nueva propuesta"
    PropuestaAceptada = 2,    // BD: "Propuesta aceptada"
    NuevoSeguidor = 3,        // BD: "Nuevo seguidor"
    BusquedaEncontrada = 4,   // BD: "Búsqueda encontrada"
    PropuestaRechazada = 5,   // BD: "Propuesta rechazada"
    MensajeNuevo = 6,         // BD: "Mensaje nuevo"
    EvaluacionRecibida = 7,   // BD: "Evaluación recibida"
    Sistema = 8               // BD: "Sistema"
}
