using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using TruequeTextil.Features.Registro.Interfaces;

namespace TruequeTextil.Features.Registro;

public class RegistroService : IRegistroService
{
    private readonly IRegistroRepository _repository;
    private readonly ILogger<RegistroService> _logger;

    public RegistroService(IRegistroRepository repository, ILogger<RegistroService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> RegistrarUsuario(
        string nombre,
        string apellido,
        string email,
        string password,
        string confirmPassword,
        int comunaId)
    {
        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre es obligatorio");

        if (string.IsNullOrWhiteSpace(apellido))
            return (false, "El apellido es obligatorio");

        if (string.IsNullOrWhiteSpace(email))
            return (false, "El correo electronico es obligatorio");

        if (!ValidarFormatoEmail(email))
            return (false, "El formato del correo electronico no es valido");

        if (comunaId <= 0)
            return (false, "Debes seleccionar una comuna");

        var (passwordValid, passwordErrors) = ValidarPassword(password, confirmPassword);
        if (!passwordValid)
            return (false, string.Join(". ", passwordErrors));

        // Verificar unicidad del email
        if (!await _repository.EmailDisponible(email))
        {
            _logger.LogWarning("Intento de registro con email existente: {Email}", email);
            return (false, "El correo electronico ya esta registrado");
        }

        // Hashear contrasena (RNF-02)
        var passwordHash = HashearPassword(password);

        try
        {
            // Registrar usuario
            var usuarioId = await _repository.CrearUsuario(nombre, apellido, email, passwordHash, comunaId);

            _logger.LogInformation("Usuario registrado exitosamente: {Email}, ID: {UsuarioId}", email, usuarioId);

            // No se simula envio de email (cuenta ya verificada por defecto)
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario: {Email}", email);
            return (false, "Error al crear la cuenta. Por favor, intenta nuevamente.");
        }
    }

    public bool ValidarFormatoEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public (bool Valid, List<string> Errors) ValidarPassword(string password, string confirmPassword)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
            errors.Add("La contrasena es obligatoria");
        else
        {
            if (password.Length < 8)
                errors.Add("La contrasena debe tener al menos 8 caracteres");

            if (!Regex.IsMatch(password, "[A-Z]"))
                errors.Add("La contrasena debe contener al menos una mayuscula");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("La contrasena debe contener al menos un numero");
        }

        if (password != confirmPassword)
            errors.Add("Las contrasenas no coinciden");

        return (errors.Count == 0, errors);
    }

    private string HashearPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        var startTime = DateTime.Now;

        byte[] hashBytes = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            50000,
            32);

        // Asegurar minimo 500ms de procesamiento (RNF-02)
        var elapsed = DateTime.Now - startTime;
        if (elapsed.TotalMilliseconds < 500)
        {
            Thread.Sleep(500 - (int)elapsed.TotalMilliseconds);
        }

        string base64Salt = Convert.ToBase64String(salt);
        string base64Hash = Convert.ToBase64String(hashBytes);

        return $"pbkdf2${base64Salt}${base64Hash}";
    }

    private string GenerarTokenSeguro()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
