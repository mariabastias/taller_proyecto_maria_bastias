using Microsoft.Extensions.Logging;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class PerfilService
{
    private readonly UsuarioRepository _usuarioRepository;
    private readonly ILogger<PerfilService> _logger;

    public PerfilService(UsuarioRepository usuarioRepository, ILogger<PerfilService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    // Obtener perfil público con estadísticas (RF-03)
    public async Task<Usuario?> ObtenerPerfilPublico(int usuarioId)
    {
        try
        {
            return await _usuarioRepository.ObtenerPerfilPublico(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil público del usuario {UsuarioId}", usuarioId);
            return null;
        }
    }

    // Completar perfil inicial (RF-03 - foto y bio obligatorios)
    public async Task<bool> CompletarPerfil(int usuarioId, string urlFotoPerfil, string biografia)
    {
        // Validar campos obligatorios según RF-03
        if (string.IsNullOrWhiteSpace(urlFotoPerfil))
        {
            _logger.LogWarning("Intento de completar perfil sin foto para usuario {UsuarioId}", usuarioId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(biografia) || biografia.Length < 10)
        {
            _logger.LogWarning("Intento de completar perfil con biografía insuficiente para usuario {UsuarioId}", usuarioId);
            return false;
        }

        try
        {
            await _usuarioRepository.ActualizarPerfil(usuarioId, urlFotoPerfil, biografia);
            _logger.LogInformation("Perfil completado exitosamente para usuario {UsuarioId}", usuarioId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar perfil del usuario {UsuarioId}", usuarioId);
            return false;
        }
    }

    // Actualizar perfil completo con todos los campos
    public async Task<bool> ActualizarPerfilCompleto(int usuarioId, string urlFotoPerfil, string biografia, string numeroTelefono)
    {
        try
        {
            await _usuarioRepository.ActualizarPerfilCompleto(usuarioId, urlFotoPerfil, biografia, numeroTelefono);
            _logger.LogInformation("Perfil actualizado completamente para usuario {UsuarioId}", usuarioId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil completo del usuario {UsuarioId}", usuarioId);
            return false;
        }
    }

    // Verificar si el perfil está completo (RF-03)
    public async Task<bool> PerfilCompleto(int usuarioId)
    {
        try
        {
            return await _usuarioRepository.PerfilCompleto(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar perfil completo del usuario {UsuarioId}", usuarioId);
            return false;
        }
    }

    // Obtener progreso del perfil (porcentaje)
    public async Task<int> ObtenerProgresoPerfil(int usuarioId)
    {
        try
        {
            var usuario = await _usuarioRepository.ObtenerUsuarioPorId(usuarioId);
            if (usuario == null) return 0;

            int progreso = 0;
            int totalCampos = 2; // Foto y biografía son los obligatorios

            if (!string.IsNullOrWhiteSpace(usuario.UrlFotoPerfil)) progreso++;
            if (!string.IsNullOrWhiteSpace(usuario.Biografia) && usuario.Biografia.Length >= 10) progreso++;

            return (progreso * 100) / totalCampos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular progreso de perfil del usuario {UsuarioId}", usuarioId);
            return 0;
        }
    }
}
