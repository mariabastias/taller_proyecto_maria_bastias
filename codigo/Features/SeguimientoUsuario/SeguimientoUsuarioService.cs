using TruequeTextil.Features.SeguimientoUsuario.Interfaces;

namespace TruequeTextil.Features.SeguimientoUsuario;

public class SeguimientoUsuarioService : ISeguimientoUsuarioService
{
    private readonly ISeguimientoUsuarioRepository _repository;
    private readonly ILogger<SeguimientoUsuarioService> _logger;

    public SeguimientoUsuarioService(ISeguimientoUsuarioRepository repository, ILogger<SeguimientoUsuarioService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> SeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId)
    {
        if (usuarioSeguidorId <= 0 || usuarioSeguidoId <= 0)
            return (false, "IDs de usuario inválidos");

        if (usuarioSeguidorId == usuarioSeguidoId)
            return (false, "No puedes seguirte a ti mismo");

        // Verificar si ya está siguiendo
        if (await _repository.EstaSiguiendo(usuarioSeguidorId, usuarioSeguidoId))
            return (false, "Ya estás siguiendo a este usuario");

        try
        {
            var success = await _repository.SeguirUsuario(usuarioSeguidorId, usuarioSeguidoId);
            if (success)
            {
                _logger.LogInformation("Usuario {SeguidorId} ahora sigue a {SeguidoId}", usuarioSeguidorId, usuarioSeguidoId);
                return (true, null);
            }
            else
            {
                return (false, "Error al seguir al usuario");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al seguir usuario {SeguidorId} -> {SeguidoId}", usuarioSeguidorId, usuarioSeguidoId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<(bool Success, string? Error)> DejarDeSeguirUsuario(int usuarioSeguidorId, int usuarioSeguidoId)
    {
        if (usuarioSeguidorId <= 0 || usuarioSeguidoId <= 0)
            return (false, "IDs de usuario inválidos");

        // Verificar si está siguiendo
        if (!await _repository.EstaSiguiendo(usuarioSeguidorId, usuarioSeguidoId))
            return (false, "No estás siguiendo a este usuario");

        try
        {
            var success = await _repository.DejarDeSeguirUsuario(usuarioSeguidorId, usuarioSeguidoId);
            if (success)
            {
                _logger.LogInformation("Usuario {SeguidorId} dejó de seguir a {SeguidoId}", usuarioSeguidorId, usuarioSeguidoId);
                return (true, null);
            }
            else
            {
                return (false, "Error al dejar de seguir al usuario");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al dejar de seguir usuario {SeguidorId} -> {SeguidoId}", usuarioSeguidorId, usuarioSeguidoId);
            return (false, "Error interno del servidor");
        }
    }

    public async Task<bool> EstaSiguiendo(int usuarioSeguidorId, int usuarioSeguidoId)
    {
        if (usuarioSeguidorId <= 0 || usuarioSeguidoId <= 0)
            return false;

        try
        {
            return await _repository.EstaSiguiendo(usuarioSeguidorId, usuarioSeguidoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar seguimiento {SeguidorId} -> {SeguidoId}", usuarioSeguidorId, usuarioSeguidoId);
            return false;
        }
    }

    public async Task<List<int>> ObtenerSeguidores(int usuarioId)
    {
        if (usuarioId <= 0)
            return new List<int>();

        try
        {
            return await _repository.ObtenerSeguidores(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener seguidores de usuario {UsuarioId}", usuarioId);
            return new List<int>();
        }
    }

    public async Task<List<int>> ObtenerSeguidos(int usuarioId)
    {
        if (usuarioId <= 0)
            return new List<int>();

        try
        {
            return await _repository.ObtenerSeguidos(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener seguidos de usuario {UsuarioId}", usuarioId);
            return new List<int>();
        }
    }

    public async Task<int> ContarSeguidores(int usuarioId)
    {
        if (usuarioId <= 0)
            return 0;

        try
        {
            return await _repository.ContarSeguidores(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar seguidores de usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }

    public async Task<int> ContarSeguidos(int usuarioId)
    {
        if (usuarioId <= 0)
            return 0;

        try
        {
            return await _repository.ContarSeguidos(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar seguidos de usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }
}
