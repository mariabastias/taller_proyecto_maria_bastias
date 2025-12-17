using TruequeTextil.Features.EditarPerfil.Interfaces;
using TruequeTextil.Shared.Services;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPerfil;

public class EditarPerfilService : IEditarPerfilService
{
    private readonly IEditarPerfilRepository _repository;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<EditarPerfilService> _logger;

    public EditarPerfilService(
        IEditarPerfilRepository repository,
        CustomAuthenticationStateProvider authenticationStateProvider,
        ILogger<EditarPerfilService> logger)
    {
        _repository = repository;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        return await _repository.ObtenerUsuarioPorId(usuarioId);
    }

    public async Task<List<Region>> ObtenerRegiones()
    {
        return await _repository.ObtenerRegiones();
    }

    public async Task<List<Comuna>> ObtenerComunasPorRegion(int regionId)
    {
        return await _repository.ObtenerComunasPorRegion(regionId);
    }

    public async Task<(bool Success, string Message)> ActualizarPerfil(int usuarioId, EditarPerfilDto datos)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(datos.Nombre) || datos.Nombre.Length < 2)
        {
            _logger.LogWarning("Intento de actualizar perfil con nombre inválido para usuario {UsuarioId}", usuarioId);
            return (false, "El nombre debe tener al menos 2 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(datos.Apellido) || datos.Apellido.Length < 2)
        {
            _logger.LogWarning("Intento de actualizar perfil con apellido inválido para usuario {UsuarioId}", usuarioId);
            return (false, "El apellido debe tener al menos 2 caracteres.");
        }

        if (datos.ComunaId <= 0)
        {
            _logger.LogWarning("Intento de actualizar perfil sin comuna para usuario {UsuarioId}", usuarioId);
            return (false, "Debe seleccionar una comuna.");
        }

        // RF-03: Validar biografía si se proporciona
        if (!string.IsNullOrEmpty(datos.Biografia) && datos.Biografia.Length > 500)
        {
            return (false, "La biografía no puede exceder 500 caracteres.");
        }

        try
        {
            // Obtener datos actuales para auditoría
            var usuarioActual = await _repository.ObtenerUsuarioPorId(usuarioId);
            if (usuarioActual == null)
            {
                return (false, "Usuario no encontrado.");
            }

            // Registrar cambios en historial si los valores cambiaron
            if (usuarioActual.Nombre != datos.Nombre)
            {
                await _repository.RegistrarCambioHistorial(usuarioId, "nombre", usuarioActual.Nombre, datos.Nombre);
            }

            if (usuarioActual.Apellido != datos.Apellido)
            {
                await _repository.RegistrarCambioHistorial(usuarioId, "apellido", usuarioActual.Apellido, datos.Apellido);
            }

            if (usuarioActual.ComunaId != datos.ComunaId)
            {
                await _repository.RegistrarCambioHistorial(usuarioId, "comuna_id",
                    usuarioActual.ComunaId.ToString(), datos.ComunaId.ToString());
            }

            // RF-03: Registrar cambios de foto y biografía
            if (usuarioActual.UrlFotoPerfil != datos.UrlFotoPerfil)
            {
                await _repository.RegistrarCambioHistorial(usuarioId, "url_foto_perfil",
                    usuarioActual.UrlFotoPerfil ?? "", datos.UrlFotoPerfil ?? "");
            }

            if (usuarioActual.Biografia != datos.Biografia)
            {
                await _repository.RegistrarCambioHistorial(usuarioId, "biografia",
                    usuarioActual.Biografia ?? "", datos.Biografia ?? "");
            }

            // Actualizar datos básicos
            await _repository.ActualizarDatosBasicos(usuarioId, datos.Nombre.Trim(), datos.Apellido.Trim(), datos.ComunaId);

            // RF-03: Actualizar foto y biografía
            await _repository.ActualizarFotoYBiografia(usuarioId, datos.UrlFotoPerfil ?? "", datos.Biografia ?? "");

            _logger.LogInformation("Perfil actualizado exitosamente para usuario {UsuarioId}", usuarioId);
            return (true, "Perfil actualizado correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil del usuario {UsuarioId}", usuarioId);
            return (false, "Error al actualizar el perfil. Por favor, intente nuevamente.");
        }
    }
}
