using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class NotificacionService
{
    // Datos de ejemplo - usando todos los tipos válidos de BD
    // BD TipoNotificacion: 1=Nueva propuesta, 2=Propuesta aceptada, 3=Nuevo seguidor, 4=Búsqueda encontrada,
    //                      5=Propuesta rechazada, 6=Mensaje nuevo, 7=Evaluación recibida, 8=Sistema
    private readonly List<Notificacion> _notificaciones = new List<Notificacion>
        {
            new Notificacion
            {
                Id = 1,
                UsuarioId = 1,
                Tipo = TipoNotificacion.NuevaPropuesta,
                Titulo = "Nueva propuesta de trueque",
                Mensaje = "Has recibido una nueva propuesta de trueque para tu prenda 'Zapatillas deportivas blancas'",
                Leida = false,
                Fecha = DateTime.Now.AddHours(-2),
                ReferenciaId = 1,
                ReferenciaUrl = "/detalle-propuesta/1"
            },
            new Notificacion
            {
                Id = 2,
                UsuarioId = 2,
                Tipo = TipoNotificacion.PropuestaAceptada,
                Titulo = "Propuesta aceptada",
                Mensaje = "Tu propuesta de trueque para 'Pantalón cargo negro' ha sido aceptada",
                Leida = true,
                Fecha = DateTime.Now.AddDays(-8),
                ReferenciaId = 2,
                ReferenciaUrl = "/detalle-propuesta/2"
            },
            new Notificacion
            {
                Id = 3,
                UsuarioId = 1,
                Tipo = TipoNotificacion.PropuestaRechazada,
                Titulo = "Propuesta rechazada",
                Mensaje = "Tu propuesta de trueque para 'Suéter tejido a mano' ha sido rechazada",
                Leida = true,
                Fecha = DateTime.Now.AddDays(-14),
                ReferenciaId = 3,
                ReferenciaUrl = "/detalle-propuesta/3"
            },
            new Notificacion
            {
                Id = 4,
                UsuarioId = 1,
                Tipo = TipoNotificacion.PropuestaAceptada,
                Titulo = "Trueque completado",
                Mensaje = "El trueque de 'Zapatillas deportivas blancas' por 'Camisa de lino beige' ha sido completado",
                Leida = true,
                Fecha = DateTime.Now.AddDays(-25),
                ReferenciaId = 4,
                ReferenciaUrl = "/detalle-trueque/4"
            },
            new Notificacion
            {
                Id = 5,
                UsuarioId = 2,
                Tipo = TipoNotificacion.MensajeNuevo,
                Titulo = "Nuevo mensaje",
                Mensaje = "Has recibido un nuevo mensaje de Andres Saez",
                Leida = false,
                Fecha = DateTime.Now.AddHours(-5),
                ReferenciaId = 1,
                ReferenciaUrl = "/mensajes/chat/1"
            },
            new Notificacion
            {
                Id = 6,
                UsuarioId = 1,
                Tipo = TipoNotificacion.NuevoSeguidor,
                Titulo = "Nuevo seguidor",
                Mensaje = "Lorenzo Rivas ha comenzado a seguirte",
                Leida = false,
                Fecha = DateTime.Now.AddDays(-1),
                ReferenciaId = 1,
                ReferenciaUrl = "/perfil/1"
            },
            new Notificacion
            {
                Id = 7,
                UsuarioId = 1,
                Tipo = TipoNotificacion.EvaluacionRecibida,
                Titulo = "Nueva evaluación",
                Mensaje = "Has recibido una evaluación de 5 estrellas por tu trueque",
                Leida = false,
                Fecha = DateTime.Now.AddDays(-2),
                ReferenciaId = 1,
                ReferenciaUrl = "/evaluaciones"
            },
            new Notificacion
            {
                Id = 8,
                UsuarioId = 1,
                Tipo = TipoNotificacion.BusquedaEncontrada,
                Titulo = "Coincidencia encontrada",
                Mensaje = "Se encontró una prenda que coincide con tu búsqueda guardada",
                Leida = true,
                Fecha = DateTime.Now.AddDays(-3),
                ReferenciaId = 5,
                ReferenciaUrl = "/prenda/5"
            }
        };

    public Task<List<Notificacion>> GetNotificacionesAsync()
    {
        return Task.FromResult(_notificaciones);
    }

    public Task<List<Notificacion>> GetNotificacionesByUsuarioIdAsync(int usuarioId)
    {
        return Task.FromResult(_notificaciones.Where(n => n.UsuarioId == usuarioId).ToList());
    }

    public Task<List<Notificacion>> GetNotificacionesNoLeidasByUsuarioIdAsync(int usuarioId)
    {
        return Task.FromResult(_notificaciones.Where(n => n.UsuarioId == usuarioId && !n.Leida).ToList());
    }

    public Task<Notificacion?> GetNotificacionByIdAsync(int id)
    {
        return Task.FromResult(_notificaciones.FirstOrDefault(n => n.Id == id));
    }

    public async Task<Notificacion> CreateNotificacionAsync(Notificacion notificacion)
    {
        // Simular un retraso de creación
        await Task.Delay(500);

        // Asignar un ID y agregar la notificación a la lista
        notificacion.Id = _notificaciones.Any() ? _notificaciones.Max(n => n.Id) + 1 : 1;
        notificacion.Fecha = DateTime.Now;
        notificacion.Leida = false;
        _notificaciones.Add(notificacion);

        return notificacion;
    }

    public async Task<bool> MarcarComoLeidaAsync(int id)
    {
        // Simular un retraso
        await Task.Delay(500);

        var notificacion = _notificaciones.FirstOrDefault(n => n.Id == id);
        if (notificacion == null)
        {
            return false;
        }

        notificacion.Leida = true;
        return true;
    }

    public async Task<bool> MarcarTodasComoLeidasAsync(int usuarioId)
    {
        // Simular un retraso
        await Task.Delay(500);

        var notificaciones = _notificaciones.Where(n => n.UsuarioId == usuarioId && !n.Leida).ToList();
        if (!notificaciones.Any())
        {
            return false;
        }

        foreach (var notificacion in notificaciones)
        {
            notificacion.Leida = true;
        }

        return true;
    }

    public async Task<bool> DeleteNotificacionAsync(int id)
    {
        // Simular un retraso
        await Task.Delay(500);

        var index = _notificaciones.FindIndex(n => n.Id == id);
        if (index == -1)
        {
            return false;
        }

        _notificaciones.RemoveAt(index);
        return true;
    }

    public async Task<bool> DeleteAllNotificacionesAsync(int usuarioId)
    {
        // Simular un retraso
        await Task.Delay(500);

        var count = _notificaciones.RemoveAll(n => n.UsuarioId == usuarioId);
        return count > 0;
    }

    // Métodos para crear notificaciones específicas
    // Usando todos los tipos válidos de BD: 1-8
    public async Task<Notificacion> CrearNotificacionPropuestaNuevaAsync(int usuarioId, int propuestaId, string nombrePrenda)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.NuevaPropuesta,
            Titulo = "Nueva propuesta de trueque",
            Mensaje = $"Has recibido una nueva propuesta de trueque para tu prenda '{nombrePrenda}'",
            ReferenciaId = propuestaId,
            ReferenciaUrl = $"/detalle-propuesta/{propuestaId}"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionPropuestaAceptadaAsync(int usuarioId, int propuestaId, string nombrePrenda)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.PropuestaAceptada,
            Titulo = "Propuesta aceptada",
            Mensaje = $"Tu propuesta de trueque para '{nombrePrenda}' ha sido aceptada",
            ReferenciaId = propuestaId,
            ReferenciaUrl = $"/detalle-propuesta/{propuestaId}"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionPropuestaRechazadaAsync(int usuarioId, int propuestaId, string nombrePrenda)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.PropuestaRechazada,
            Titulo = "Propuesta rechazada",
            Mensaje = $"Tu propuesta de trueque para '{nombrePrenda}' ha sido rechazada",
            ReferenciaId = propuestaId,
            ReferenciaUrl = $"/detalle-propuesta/{propuestaId}"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionTruequeCompletadoAsync(int usuarioId, int propuestaId, string prendaOfrecida, string prendaSolicitada)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.PropuestaAceptada,
            Titulo = "Trueque completado",
            Mensaje = $"El trueque de '{prendaOfrecida}' por '{prendaSolicitada}' ha sido completado",
            ReferenciaId = propuestaId,
            ReferenciaUrl = $"/detalle-trueque/{propuestaId}"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionMensajeNuevoAsync(int usuarioId, int propuestaId, string nombreRemitente)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.MensajeNuevo,
            Titulo = "Nuevo mensaje",
            Mensaje = $"Has recibido un nuevo mensaje de {nombreRemitente}",
            ReferenciaId = propuestaId,
            ReferenciaUrl = $"/mensajes/chat/{propuestaId}"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionEvaluacionAsync(int usuarioId, int truequeId, int estrellas)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.EvaluacionRecibida,
            Titulo = "Nueva evaluación",
            Mensaje = $"Has recibido una evaluación de {estrellas} estrellas",
            ReferenciaId = truequeId,
            ReferenciaUrl = $"/evaluaciones"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionNuevoSeguidorAsync(int usuarioId, int seguidorId, string nombreSeguidor)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.NuevoSeguidor,
            Titulo = "Nuevo seguidor",
            Mensaje = $"{nombreSeguidor} ha comenzado a seguirte",
            ReferenciaId = seguidorId,
            ReferenciaUrl = $"/perfil/{seguidorId}"
        };

        return await CreateNotificacionAsync(notificacion);
    }

    public async Task<Notificacion> CrearNotificacionSistemaAsync(int usuarioId, string titulo, string mensaje)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.Sistema,
            Titulo = titulo,
            Mensaje = mensaje
        };

        return await CreateNotificacionAsync(notificacion);
    }
}
