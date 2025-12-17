namespace TruequeTextil.Features.InicioSesion;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AutenticacionService _autenticacionService;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthController(
        AutenticacionService autenticacionService,
        UsuarioRepository usuarioRepository,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _autenticacionService = autenticacionService;
        _usuarioRepository = usuarioRepository;
        _authStateProvider = authStateProvider;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        // Validar credenciales usando el servicio existente (sin SignIn)
        var (success, error, usuario, requiresAdminVerification) = await _autenticacionService.ValidarCredenciales(req.Email, req.Password);

        if (!success || usuario == null)
        {
            return BadRequest(new { error = error ?? "Credenciales inválidas" });
        }

        // Actualizar último login
        await _usuarioRepository.ActualizarUltimoLogin(usuario.UsuarioId);

        // Crear claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new(ClaimTypes.Email, usuario.CorreoElectronico),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Role, usuario.Rol)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        // Sincronizar el estado local del AuthenticationStateProvider
        await _authStateProvider.SignInAsync(usuario);

        // Lógica especial para administradores: crear sesión y redirigir directamente
        if (usuario.Rol == "administrador")
        {
            // Crear sesión administrativa permanente
            var token = Guid.NewGuid().ToString();
            var sesion = new SesionAdministrador
            {
                AdministradorId = usuario.UsuarioId,
                TokenSesion = token,
                FechaInicio = DateTime.Now,
                FechaUltimaActividad = DateTime.Now,
                FechaExpiracion = null, // Permanente hasta logout
                Activa = true,
                Administrador = usuario
            };

            await _usuarioRepository.CrearSesionAdministrador(sesion);

            return Ok(new
            {
                ok = true,
                requiresAdminVerification = false,
                redirectUrl = "/admin"
            });
        }

        // Obtener página de redirección para usuarios normales
        var paginaRedireccion = await _autenticacionService.ObtenerPaginaRedireccionPostLogin(usuario.UsuarioId) ?? "/explorar";

        return Ok(new
        {
            ok = true,
            requiresAdminVerification,
            redirectUrl = requiresAdminVerification ? "/admin" : paginaRedireccion
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _authStateProvider.SignOutAsync();
        return Ok(new { ok = true });
    }

}