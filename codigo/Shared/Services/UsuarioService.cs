using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class UsuarioService
{
    // En una aplicación real, esto sería un servicio que se comunica con una API
    private readonly List<Usuario> _usuarios = new List<Usuario>
        {
            new Usuario
            {
                UsuarioId = 1,
                Nombre = "Juan Perez",
                CorreoElectronico = "juan@example.com",
                FechaRegistro = DateTime.Now.AddMonths(-3),
                Comuna = new Comuna { NombreComuna = "Santiago Centro", Region = new Region { NombreRegion = "Metropolitana" } },
                Biografia = "Apasionado por la moda sostenible y el intercambio de prendas.",
                UrlFotoPerfil = "👨",
                EstadoUsuario = "activo",
                CuentaVerificada = true,
                ReputacionPromedio = 4.5m,
                FechaUltimoLogin = DateTime.Now.AddHours(-2)
            },
            new Usuario
            {
                UsuarioId = 2,
                Nombre = "María González",
                CorreoElectronico = "maria@example.com",
                FechaRegistro = DateTime.Now.AddMonths(-2),
                Comuna = new Comuna { NombreComuna = "Providencia", Region = new Region { NombreRegion = "Metropolitana" } },
                Biografia = "Me encanta la ropa vintage y dar nueva vida a prendas que ya no uso.",
                UrlFotoPerfil = "👩",
                EstadoUsuario = "activo",
                CuentaVerificada = true,
                ReputacionPromedio = 4.8m,
                FechaUltimoLogin = DateTime.Now.AddHours(-5)
            },
            new Usuario
            {
                UsuarioId = 3,
                Nombre = "Juan Pérez",
                CorreoElectronico = "juan@example.com",
                FechaRegistro = DateTime.Now.AddMonths(-1),
                Comuna = new Comuna { NombreComuna = "Las Condes", Region = new Region { NombreRegion = "Metropolitana" } },
                Biografia = "Buscando renovar mi guardarropa de manera sostenible.",
                UrlFotoPerfil = "👨",
                EstadoUsuario = "activo",
                CuentaVerificada = false,
                ReputacionPromedio = 0.0m,
                FechaUltimoLogin = DateTime.Now.AddDays(-1)
            },
            new Usuario
            {
                UsuarioId = 4,
                Nombre = "Ana Silva",
                CorreoElectronico = "ana@example.com",
                FechaRegistro = DateTime.Now.AddDays(-15),
                Comuna = new Comuna { NombreComuna = "Ñuñoa", Region = new Region { NombreRegion = "Metropolitana" } },
                Biografia = "Diseñadora de moda interesada en el intercambio de prendas únicas.",
                UrlFotoPerfil = "👩",
                EstadoUsuario = "activo",
                CuentaVerificada = true,
                ReputacionPromedio = 5.0m,
                FechaUltimoLogin = DateTime.Now.AddHours(-12)
            },
            new Usuario
            {
                UsuarioId = 5,
                Nombre = "Admin",
                CorreoElectronico = "admin@trueque-textil.com",
                FechaRegistro = DateTime.Now.AddYears(-1),
                Comuna = new Comuna { NombreComuna = "Santiago", Region = new Region { NombreRegion = "Metropolitana" } },
                Biografia = "Administrador de la plataforma Trueque Textil.",
                UrlFotoPerfil = "🧑‍💼",
                EstadoUsuario = "activo",
                CuentaVerificada = true,
                ReputacionPromedio = 0.0m,
                FechaUltimoLogin = DateTime.Now.AddMinutes(-30),
                Rol = "administrador"
            }
        };

    public Task<List<Usuario>> GetUsuariosAsync()
    {
        return Task.FromResult(_usuarios);
    }

    public Task<Usuario?> GetUsuarioByIdAsync(int id)
    {
        return Task.FromResult(_usuarios.FirstOrDefault(u => u.UsuarioId == id));
    }

    public Task<Usuario?> GetUsuarioByEmailAsync(string email)
    {
        return Task.FromResult(_usuarios.FirstOrDefault(u =>
            string.Equals(u.CorreoElectronico, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<List<Usuario>> GetUsuariosByEstadoAsync(string estado)
    {
        return Task.FromResult(_usuarios.Where(u => u.EstadoUsuario == estado).ToList());
    }

    public Task<List<Usuario>> GetUsuariosVerificadosAsync()
    {
        return Task.FromResult(_usuarios.Where(u => u.CuentaVerificada).ToList());
    }

    public Task<List<Usuario>> GetUsuariosAdminAsync()
    {
        return Task.FromResult(_usuarios.Where(u => u.Rol == "administrador").ToList());
    }

    public async Task<Usuario> CreateUsuarioAsync(Usuario usuario)
    {
        // Simular un retraso de creación
        await Task.Delay(500);

        // Asignar un ID y agregar el usuario a la lista
        usuario.UsuarioId = _usuarios.Max(u => u.UsuarioId) + 1;
        usuario.FechaRegistro = DateTime.Now;
        usuario.EstadoUsuario = "activo";
        usuario.ReputacionPromedio = 0.0m;
        usuario.FechaUltimoLogin = DateTime.Now;
        usuario.Rol = usuario.Rol ?? "usuario";
        _usuarios.Add(usuario);

        return usuario;
    }

    public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
    {
        // Simular un retraso
        await Task.Delay(500);

        var existingUsuario = _usuarios.FirstOrDefault(u => u.UsuarioId == usuario.UsuarioId);
        if (existingUsuario == null)
        {
            return false;
        }

        // Actualizar solo los campos proporcionados, preservando los valores existentes
        // para los campos que no se están actualizando
        if (!string.IsNullOrEmpty(usuario.Nombre)) existingUsuario.Nombre = usuario.Nombre;
        if (!string.IsNullOrEmpty(usuario.CorreoElectronico)) existingUsuario.CorreoElectronico = usuario.CorreoElectronico;
        if (usuario.ComunaId > 0) existingUsuario.ComunaId = usuario.ComunaId;
        if (!string.IsNullOrEmpty(usuario.Biografia)) existingUsuario.Biografia = usuario.Biografia;
        if (!string.IsNullOrEmpty(usuario.UrlFotoPerfil)) existingUsuario.UrlFotoPerfil = usuario.UrlFotoPerfil;
        if (!string.IsNullOrEmpty(usuario.EstadoUsuario)) existingUsuario.EstadoUsuario = usuario.EstadoUsuario;
        if (!string.IsNullOrEmpty(usuario.Rol)) existingUsuario.Rol = usuario.Rol;

        // Actualizar campos numéricos y booleanos solo si tienen valores diferentes a los predeterminados
        if (usuario.ReputacionPromedio > 0) existingUsuario.ReputacionPromedio = usuario.ReputacionPromedio;

        // Actualizar campos de fecha solo si son diferentes a los valores predeterminados
        if (usuario.FechaUltimoLogin.HasValue) existingUsuario.FechaUltimoLogin = usuario.FechaUltimoLogin;

        // Actualizar campos booleanos
        existingUsuario.CuentaVerificada = usuario.CuentaVerificada;

        return true;
    }

    public async Task<bool> DeleteUsuarioAsync(int id)
    {
        // Simular un retraso
        await Task.Delay(500);

        var index = _usuarios.FindIndex(u => u.UsuarioId == id);
        if (index == -1)
        {
            return false;
        }

        _usuarios.RemoveAt(index);
        return true;
    }

    public async Task<bool> CambiarEstadoUsuarioAsync(int id, string estado)
    {
        // Simular un retraso
        await Task.Delay(500);

        var usuario = _usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario == null)
        {
            return false;
        }

        usuario.EstadoUsuario = estado;
        return true;
    }

    public async Task<bool> VerificarUsuarioAsync(int id)
    {
        // Simular un retraso
        await Task.Delay(500);

        var usuario = _usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario == null)
        {
            return false;
        }

        usuario.CuentaVerificada = true;
        return true;
    }

    public async Task<bool> ActualizarUltimoAccesoAsync(int id)
    {
        var usuario = _usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario == null)
        {
            return false;
        }

        usuario.FechaUltimoLogin = DateTime.Now;
        return true;
    }

    public async Task<bool> IncrementarTruequesConcretadosAsync(int id)
    {
        var usuario = _usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario == null)
        {
            return false;
        }

        // Actualizar la reputación basada en el número de trueques (placeholder logic)
        if (usuario.ReputacionPromedio > 0)
        {
            // En una aplicación real, esto se calcularía basado en todas las evaluaciones
            usuario.ReputacionPromedio = usuario.ReputacionPromedio; // Placeholder
        }

        return true;
    }

    public async Task<bool> ActualizarCalificacionPromedioAsync(int id, double calificacion)
    {
        var usuario = _usuarios.FirstOrDefault(u => u.UsuarioId == id);
        if (usuario == null)
        {
            return false;
        }

        // En una aplicación real, esto se calcularía basado en todas las evaluaciones
        usuario.ReputacionPromedio = (decimal)calificacion;
        return true;
    }

    // Método para autenticación (simulado)
    public async Task<Usuario?> AuthenticateAsync(string email, string password)
    {
        // Simular un retraso de autenticación
        await Task.Delay(500);

        // En una aplicación real, se verificaría la contraseña hasheada
        var usuario = _usuarios.FirstOrDefault(u =>
            string.Equals(u.CorreoElectronico, email, StringComparison.OrdinalIgnoreCase));

        if (usuario != null)
        {
            await ActualizarUltimoAccesoAsync(usuario.UsuarioId);
        }

        return usuario;
    }
}
