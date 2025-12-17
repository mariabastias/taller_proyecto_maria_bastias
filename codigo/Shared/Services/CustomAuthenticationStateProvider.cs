using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly UsuarioRepository _usuarioRepository;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    private Usuario? _cachedUser;

    public CustomAuthenticationStateProvider(UsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task SignInAsync(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Surname, usuario.Apellido),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("CuentaVerificada", usuario.CuentaVerificada.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        _currentUser = new ClaimsPrincipal(identity);
        _cachedUser = usuario;

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        _cachedUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<Usuario?> GetCurrentUserAsync()
    {
        // Si tenemos usuario cacheado y es válido, retornarlo
        if (_cachedUser != null)
        {
            return _cachedUser;
        }

        // Usar el estado local
        if (_currentUser.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var localUserIdClaim = _currentUser.FindFirst(ClaimTypes.NameIdentifier);
        if (localUserIdClaim == null || !int.TryParse(localUserIdClaim.Value, out var localUserId))
        {
            return null;
        }

        _cachedUser = await _usuarioRepository.ObtenerUsuarioPorId(localUserId);
        return _cachedUser;
    }

    // Método para refrescar el usuario cacheado
    public async Task RefreshUserAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            _cachedUser = await _usuarioRepository.ObtenerUsuarioPorId(currentUser.UsuarioId);
        }
    }

    // Método para verificar si la sesión está activa
    public bool IsAuthenticated()
    {
        return _currentUser.Identity?.IsAuthenticated == true;
    }
}
