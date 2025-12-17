using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using TruequeTextil.Features.RecuperarPassword.Interfaces;

namespace TruequeTextil.Features.RecuperarPassword;

public class RecuperarPasswordService : IRecuperarPasswordService
{
    private readonly IRecuperarPasswordRepository _repository;
    private readonly ILogger<RecuperarPasswordService> _logger;

    public RecuperarPasswordService(
        IRecuperarPasswordRepository repository,
        ILogger<RecuperarPasswordService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token)> EnviarEmailRecuperacion(string email)
    {
        var usuarioId = await _repository.ObtenerUsuarioIdPorEmail(email);
        if (usuarioId == null)
        {
            // Por seguridad, no revelamos si el email existe o no
            _logger.LogInformation("Solicitud de recuperacion para email no registrado: {Email}", email);
            return (true, null); // Retornamos success para no revelar si el email existe
        }

        // Generar token seguro
        var token = GenerarTokenRecuperacion();
        var expiracion = DateTime.Now.AddHours(1); // 1 hora de validez

        // Guardar token en BD
        await _repository.GuardarTokenRecuperacion(usuarioId.Value, token, expiracion);

        // En produccion, aqui se integraria con un servicio de email
        _logger.LogInformation("Token de recuperacion generado para: {Email}", email);

        // Simular delay de envio de email
        await Task.Delay(500);

        return (true, token);
    }

    public async Task<int?> ValidarTokenRecuperacion(string token)
    {
        return await _repository.ValidarTokenRecuperacion(token);
    }

    public async Task<bool> RestablecerPassword(string token, string nuevaPassword)
    {
        var usuarioId = await _repository.ValidarTokenRecuperacion(token);
        if (usuarioId == null)
        {
            _logger.LogWarning("Token de recuperacion invalido o expirado");
            return false;
        }

        // Hashear nueva contrasena (RNF-02)
        var passwordHash = HashPassword(nuevaPassword);

        // Actualizar contrasena y limpiar token
        await _repository.ActualizarPassword(usuarioId.Value, passwordHash);

        _logger.LogInformation("Contrasena restablecida para usuario ID: {UsuarioId}", usuarioId);
        return true;
    }

    private string GenerarTokenRecuperacion()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        var startTime = DateTime.Now;
        byte[] hashBytes = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 50000, 32);

        // Ensure minimum 500ms processing time (RNF-02)
        var elapsed = DateTime.Now - startTime;
        if (elapsed.TotalMilliseconds < 500)
        {
            Thread.Sleep(500 - (int)elapsed.TotalMilliseconds);
        }

        string base64Salt = Convert.ToBase64String(salt);
        string base64Hash = Convert.ToBase64String(hashBytes);

        return $"pbkdf2${base64Salt}${base64Hash}";
    }
}
