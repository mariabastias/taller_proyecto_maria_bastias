using TruequeTextil.Features.MisPrendas.Interfaces;
using TruequeTextil.Features.Notificaciones.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.MisPrendas;

public class MisPrendasService : IMisPrendasService
{
    private readonly IMisPrendasRepository _repository;
    private readonly INotificacionesService _notificacionesService;
    private readonly ILogger<MisPrendasService> _logger;

    public MisPrendasService(
        IMisPrendasRepository repository,
        INotificacionesService notificacionesService,
        ILogger<MisPrendasService> logger)
    {
        _repository = repository;
        _notificacionesService = notificacionesService;
        _logger = logger;
    }

    public async Task<List<Prenda>> ObtenerMisPrendas(int usuarioId)
    {
        try
        {
            return await _repository.ObtenerPrendasUsuario(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prendas del usuario {UsuarioId}", usuarioId);
            return new List<Prenda>();
        }
    }

    public async Task<Prenda?> ObtenerPrenda(int prendaId, int usuarioId)
    {
        try
        {
            var prenda = await _repository.ObtenerPrendaPorId(prendaId);
            if (prenda != null && prenda.UsuarioId == usuarioId)
            {
                return prenda;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prenda {PrendaId}", prendaId);
            return null;
        }
    }

    // RF-05: Verificar propuestas activas antes de permitir edicion
    public async Task<(bool TienePropuestas, int CantidadPropuestas)> VerificarPropuestasActivas(int prendaId)
    {
        try
        {
            var cantidad = await _repository.ContarPropuestasActivas(prendaId);
            return (cantidad > 0, cantidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar propuestas activas para prenda {PrendaId}", prendaId);
            return (false, 0);
        }
    }

    // RF-05: Actualizar prenda con historial de cambios
    public async Task<(bool Success, string? Error)> ActualizarPrenda(
        int prendaId,
        int usuarioId,
        string titulo,
        string descripcion,
        int categoriaId,
        string talla,
        int estadoPrendaId)
    {
        try
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(titulo))
                return (false, "El titulo es obligatorio");

            if (titulo.Length > 100)
                return (false, "El titulo no puede exceder 100 caracteres");

            if (string.IsNullOrWhiteSpace(descripcion))
                return (false, "La descripcion es obligatoria");

            if (descripcion.Length > 500)
                return (false, "La descripcion no puede exceder 500 caracteres");

            if (categoriaId <= 0)
                return (false, "Debes seleccionar una categoria");

            if (string.IsNullOrWhiteSpace(talla))
                return (false, "La talla es obligatoria");

            if (estadoPrendaId <= 0)
                return (false, "Debes seleccionar el estado de la prenda");

            // Obtener prenda actual para comparar y registrar cambios
            var prendaActual = await _repository.ObtenerPrendaPorId(prendaId);
            if (prendaActual == null)
                return (false, "Prenda no encontrada");

            if (prendaActual.UsuarioId != usuarioId)
                return (false, "No tienes permiso para editar esta prenda");

            // Registrar cambios en historial (RF-05)
            var cambios = new List<(string campo, string? anterior, string? nuevo)>();

            if (prendaActual.TituloPublicacion != titulo.Trim())
                cambios.Add(("titulo_publicacion", prendaActual.TituloPublicacion, titulo.Trim()));

            if (prendaActual.DescripcionPublicacion != descripcion.Trim())
                cambios.Add(("descripcion_publicacion", prendaActual.DescripcionPublicacion, descripcion.Trim()));

            if (prendaActual.CategoriaId != categoriaId)
                cambios.Add(("categoria_id", prendaActual.CategoriaId.ToString(), categoriaId.ToString()));

            if (prendaActual.Talla != talla)
                cambios.Add(("talla", prendaActual.Talla, talla));

            if (prendaActual.EstadoPrendaId != estadoPrendaId)
                cambios.Add(("estado_prenda_id", prendaActual.EstadoPrendaId.ToString(), estadoPrendaId.ToString()));

            if (cambios.Count == 0)
                return (true, null); // No hay cambios

            // Actualizar prenda
            var prendaActualizada = new Prenda
            {
                PrendaId = prendaId,
                TituloPublicacion = titulo.Trim(),
                DescripcionPublicacion = descripcion.Trim(),
                CategoriaId = categoriaId,
                Talla = talla,
                EstadoPrendaId = estadoPrendaId
            };

            var actualizado = await _repository.ActualizarPrenda(prendaActualizada);

            if (!actualizado)
                return (false, "Error al actualizar la prenda");

            // Registrar historial de cambios
            foreach (var (campo, anterior, nuevo) in cambios)
            {
                var historial = new HistorialPrenda
                {
                    PrendaId = prendaId,
                    CampoModificado = campo,
                    ValorAnterior = anterior,
                    ValorNuevo = nuevo,
                    UsuarioModificadorId = usuarioId,
                    FechaModificacion = DateTime.Now
                };
                await _repository.RegistrarHistorial(historial);
            }

            _logger.LogInformation("Prenda {PrendaId} actualizada por usuario {UsuarioId}. Campos modificados: {Campos}",
                prendaId, usuarioId, string.Join(", ", cambios.Select(c => c.campo)));

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar prenda {PrendaId}", prendaId);
            return (false, "Error al procesar la actualizacion");
        }
    }

    // RF-06: Eliminacion logica con cambio de estado y notificacion a usuarios con favoritos
    public async Task<(bool Success, string? Error)> EliminarPrenda(int prendaId, int usuarioId)
    {
        try
        {
            // Verificar propiedad
            var prenda = await _repository.ObtenerPrendaPorId(prendaId);
            if (prenda == null)
                return (false, "Prenda no encontrada");

            if (prenda.UsuarioId != usuarioId)
                return (false, "No tienes permiso para eliminar esta prenda");

            // Verificar propuestas activas
            var propuestasActivas = await _repository.ContarPropuestasActivas(prendaId);
            if (propuestasActivas > 0)
            {
                return (false, $"No puedes eliminar esta prenda porque tiene {propuestasActivas} propuesta(s) activa(s)");
            }

            // RF-06: Obtener usuarios que tienen esta prenda en favoritos ANTES de eliminar
            var usuariosConFavorito = await _repository.ObtenerUsuariosConPrendaEnFavoritos(prendaId);

            // Eliminacion logica (cambiar estado_publicacion_id a 3)
            var eliminado = await _repository.EliminarPrenda(prendaId);

            if (!eliminado)
                return (false, "Error al eliminar la prenda");

            // Registrar en historial
            var historial = new HistorialPrenda
            {
                PrendaId = prendaId,
                CampoModificado = "estado_publicacion_id",
                ValorAnterior = prenda.EstadoPublicacionId.ToString(),
                ValorNuevo = "3",
                UsuarioModificadorId = usuarioId,
                FechaModificacion = DateTime.Now
            };
            await _repository.RegistrarHistorial(historial);

            // RF-06: Notificar a usuarios que tenian esta prenda en favoritos
            foreach (var favUsuarioId in usuariosConFavorito)
            {
                await _notificacionesService.EnviarNotificacion(
                    favUsuarioId,
                    "Prenda eliminada de favoritos",
                    $"La prenda \"{prenda.TituloPublicacion}\" que tenias en favoritos ya no esta disponible.",
                    "prenda_eliminada",
                    prendaId);
            }

            if (usuariosConFavorito.Count > 0)
            {
                _logger.LogInformation("Prenda {PrendaId} eliminada. Notificados {Count} usuarios con favorito.",
                    prendaId, usuariosConFavorito.Count);
            }

            _logger.LogInformation("Prenda {PrendaId} eliminada (logica) por usuario {UsuarioId}", prendaId, usuarioId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar prenda {PrendaId}", prendaId);
            return (false, "Error al procesar la eliminacion");
        }
    }

    // RF-08: Notificar cambios de disponibilidad a usuarios con favoritos
    public async Task<(bool Success, string? Error)> CambiarDisponibilidad(int prendaId, int usuarioId, bool disponible)
    {
        try
        {
            var prenda = await _repository.ObtenerPrendaPorId(prendaId);
            if (prenda == null)
                return (false, "Prenda no encontrada");

            if (prenda.UsuarioId != usuarioId)
                return (false, "No tienes permiso para modificar esta prenda");

            // estado_publicacion_id: 1=disponible, 2=en negociacion, 3=eliminada
            var nuevoEstado = disponible ? 1 : 2;

            // Si no hay cambio real, no hacer nada
            if (prenda.EstadoPublicacionId == nuevoEstado)
                return (true, null);

            var cambiado = await _repository.CambiarDisponibilidad(prendaId, nuevoEstado);

            if (!cambiado)
                return (false, "Error al cambiar la disponibilidad");

            // Registrar en historial
            var historial = new HistorialPrenda
            {
                PrendaId = prendaId,
                CampoModificado = "estado_publicacion_id",
                ValorAnterior = prenda.EstadoPublicacionId.ToString(),
                ValorNuevo = nuevoEstado.ToString(),
                UsuarioModificadorId = usuarioId,
                FechaModificacion = DateTime.Now
            };
            await _repository.RegistrarHistorial(historial);

            // RF-08: Notificar a usuarios con favoritos sobre el cambio de estado
            var usuariosConFavorito = await _repository.ObtenerUsuariosConPrendaEnFavoritos(prendaId);
            var mensaje = disponible
                ? $"La prenda \"{prenda.TituloPublicacion}\" que tienes en favoritos ahora esta disponible."
                : $"La prenda \"{prenda.TituloPublicacion}\" que tienes en favoritos ahora esta en negociacion.";
            var tipo = disponible ? "favorito_disponible" : "favorito_en_negociacion";

            foreach (var favUsuarioId in usuariosConFavorito)
            {
                await _notificacionesService.EnviarNotificacion(
                    favUsuarioId,
                    disponible ? "Prenda disponible" : "Prenda en negociacion",
                    mensaje,
                    tipo,
                    prendaId);
            }

            if (usuariosConFavorito.Count > 0)
            {
                _logger.LogInformation("Cambio de disponibilidad de prenda {PrendaId}. Notificados {Count} usuarios.",
                    prendaId, usuariosConFavorito.Count);
            }

            _logger.LogInformation("Disponibilidad de prenda {PrendaId} cambiada a {Estado} por usuario {UsuarioId}",
                prendaId, disponible ? "disponible" : "no disponible", usuarioId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar disponibilidad de prenda {PrendaId}", prendaId);
            return (false, "Error al procesar el cambio");
        }
    }
}
