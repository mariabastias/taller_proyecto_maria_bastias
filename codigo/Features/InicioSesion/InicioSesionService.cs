namespace TruequeTextil.Features.InicioSesion;

public class InicioSesionService : IInicioSesionService
{
    private readonly IInicioSesionRepository _repository;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<InicioSesionService> _logger;

    public InicioSesionService(
        IInicioSesionRepository repository,
        CustomAuthenticationStateProvider authenticationStateProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<InicioSesionService> logger)
    {
        _repository = repository;
        _authenticationStateProvider = authenticationStateProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, Usuario? Usuario, bool RequiresAdminVerification)> IniciarSesion(string email, string password)
    {
        var usuario = await _repository.ObtenerUsuarioPorEmail(email);
        if (usuario == null)
        {
            _logger.LogWarning("Intento de login fallido - usuario no encontrado: {Email}", email);
            return (false, "Usuario no encontrado", null, false);
        }

        // Verificar estado del usuario (RNF-03)
        var estadoUsuario = await _repository.ObtenerEstadoUsuario(usuario.UsuarioId);
        if (estadoUsuario == "suspendido")
        {
            _logger.LogWarning("Intento de login de cuenta suspendida: {Email}", email);
            return (false, "Tu cuenta ha sido suspendida. Contacta a soporte para mas informacion.", null, false);
        }
        if (estadoUsuario == "eliminado" || estadoUsuario == "inactivo")
        {
            _logger.LogWarning("Intento de login de cuenta inactiva/eliminada: {Email}", email);
            return (false, "Esta cuenta ya no esta activa.", null, false);
        }

        // Verificar hash de contrasena
        if (!VerifyPassword(password, usuario.PasswordHash))
        {
            _logger.LogWarning("Intento de login con contrasena incorrecta: {Email}", email);
            return (false, "Contrasena incorrecta", null, false);
        }


        // Actualizar ultimo login
        await _repository.ActualizarUltimoLogin(usuario.UsuarioId);

        // Verificar rol para administradores (RNF-03)
        bool requiresAdminVerification = usuario.Rol == "administrador";

        // Sign in with ASP.NET Core authentication
        await SignInWithClaimsAsync(usuario);

        _logger.LogInformation("Login exitoso para usuario: {Email}, RequiresAdminVerification: {RequiresAdmin}", email, requiresAdminVerification);

        return (true, null, usuario, requiresAdminVerification);
    }

    private async Task SignInWithClaimsAsync(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);
        }

        await _authenticationStateProvider.SignInAsync(usuario);
    }

    public async Task Logout()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        await _authenticationStateProvider.SignOutAsync();
    }

    public async Task<bool> IsSessionExpired()
    {
        var currentUser = await GetCurrentUser();
        if (currentUser == null)
        {
            return true;
        }

        if (currentUser.FechaUltimoLogin.HasValue)
        {
            var timeSinceLastActivity = DateTime.Now - currentUser.FechaUltimoLogin.Value;
            var timeoutMinutes = 30;
            return timeSinceLastActivity.TotalMinutes > timeoutMinutes;
        }

        return true;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<string?> ObtenerPaginaRedireccionPostLogin(int usuarioId)
    {
        var onboardingCompletado = await _repository.OnboardingCompletado(usuarioId);
        if (!onboardingCompletado)
        {
            return "/onboarding";
        }

        var perfilCompleto = await _repository.PerfilCompleto(usuarioId);
        if (!perfilCompleto)
        {
            return "/completar-perfil";
        }

        return null;
    }

    public async Task UpdateActivity()
    {
        var currentUser = await GetCurrentUser();
        if (currentUser != null)
        {
            await _repository.ActualizarUltimoLogin(currentUser.UsuarioId);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        try
        {
            var parts = hash.Split('$');
            if (parts.Length != 3 || parts[0] != "pbkdf2")
                return false;

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            byte[] actualHash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 50000, 32);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch
        {
            return false;
        }
    }
}
