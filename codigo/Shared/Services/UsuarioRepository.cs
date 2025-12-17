using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class UsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    // Verificar unicidad del email
    public async Task<bool> VerificarUnicidadEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM Usuario WHERE correo_electronico = @Email";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count == 0;
    }

    // Registrar nuevo usuario
    public async Task<int> RegistrarNuevoUsuario(Usuario usuario)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                INSERT INTO Usuario (correo_electronico, password_hash, nombre, apellido, comuna_id, cuenta_verificada, fecha_ultimo_login, rol, reputacion_promedio)
                VALUES (@Correo, @PasswordHash, @Nombre, @Apellido, @ComunaId, 1, NULL, 'usuario', 0.00);
                SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Correo", usuario.CorreoElectronico);
        command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
        command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@Apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@ComunaId", usuario.ComunaId);

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return id;
    }

    // Obtener usuario por credenciales (para login)
    public async Task<Usuario?> ObtenerUsuarioPorCredenciales(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                SELECT u.usuario_id, u.correo_electronico, u.password_hash, u.nombre, u.apellido, u.comuna_id,
                       u.url_foto_perfil, u.biografia, u.reputacion_promedio, u.cuenta_verificada,
                       u.fecha_ultimo_login, u.rol,
                       c.nombre_comuna, r.nombre_region
                FROM Usuario u
                INNER JOIN Comuna c ON u.comuna_id = c.comuna_id
                INNER JOIN Region r ON c.region_id = r.region_id
                WHERE u.correo_electronico = @Email";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                UsuarioId = reader.GetInt32(0),
                CorreoElectronico = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Nombre = reader.GetString(3),
                Apellido = reader.GetString(4),
                ComunaId = reader.GetInt32(5),
                UrlFotoPerfil = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Biografia = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                ReputacionPromedio = reader.GetDecimal(8),
                CuentaVerificada = reader.GetBoolean(9),
                FechaUltimoLogin = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                Rol = reader.GetString(11),
                Comuna = new Comuna
                {
                    ComunaId = reader.GetInt32(5),
                    NombreComuna = reader.GetString(12),
                    Region = new Region
                    {
                        NombreRegion = reader.GetString(13)
                    }
                }
            };
        }
        return null;
    }

    // Actualizar último login
    public async Task ActualizarUltimoLogin(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Usuario SET fecha_ultimo_login = @Fecha WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Fecha", DateTime.Now);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Obtener roles y verificación
    public async Task<(string Rol, bool Verificado)> ObtenerRolesYVerificacion(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT rol, cuenta_verificada FROM Usuario WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader.GetString(0), reader.GetBoolean(1));
        }
        throw new KeyNotFoundException("Usuario no encontrado");
    }

    // Actualizar perfil
    public async Task ActualizarPerfil(int usuarioId, string urlFotoPerfil, string biografia)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Usuario SET url_foto_perfil = @Foto, biografia = @Bio WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Foto", urlFotoPerfil);
        command.Parameters.AddWithValue("@Bio", biografia);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Obtener perfil público
    public async Task<Usuario?> ObtenerPerfilPublico(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                SELECT u.usuario_id, u.nombre, u.apellido, u.url_foto_perfil, u.biografia, u.reputacion_promedio,
                       u.cuenta_verificada, u.fecha_ultimo_login, u.rol,
                       c.nombre_comuna, r.nombre_region,
                       (SELECT COUNT(*) FROM Prenda WHERE usuario_id = u.usuario_id) as prendas_publicadas,
                       (SELECT COUNT(*) FROM Evaluacion WHERE usuario_evaluado_id = u.usuario_id) as valoraciones
                FROM Usuario u
                INNER JOIN Comuna c ON u.comuna_id = c.comuna_id
                INNER JOIN Region r ON c.region_id = r.region_id
                WHERE u.usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var usuario = new Usuario
            {
                UsuarioId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellido = reader.GetString(2),
                UrlFotoPerfil = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Biografia = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                ReputacionPromedio = reader.GetDecimal(5),
                CuentaVerificada = reader.GetBoolean(6),
                FechaUltimoLogin = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Rol = reader.GetString(8),
                Comuna = new Comuna
                {
                    NombreComuna = reader.GetString(9),
                    Region = new Region
                    {
                        NombreRegion = reader.GetString(10)
                    }
                },
                Estadisticas = new Estadisticas
                {
                    PrendasPublicadas = reader.GetInt32(11),
                    Valoraciones = reader.GetInt32(12)
                }
            };
            return usuario;
        }
        return null;
    }

    // Marcar cuenta como verificada
    public async Task MarcarCuentaVerificada(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Usuario SET cuenta_verificada = 1 WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Obtener usuario por ID
    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                SELECT u.usuario_id, u.correo_electronico, u.password_hash, u.nombre, u.apellido, u.comuna_id,
                       u.url_foto_perfil, u.biografia, u.reputacion_promedio, u.cuenta_verificada,
                       u.fecha_ultimo_login, u.rol,
                       c.nombre_comuna, r.nombre_region
                FROM Usuario u
                INNER JOIN Comuna c ON u.comuna_id = c.comuna_id
                INNER JOIN Region r ON c.region_id = r.region_id
                WHERE u.usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                UsuarioId = reader.GetInt32(0),
                CorreoElectronico = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Nombre = reader.GetString(3),
                Apellido = reader.GetString(4),
                ComunaId = reader.GetInt32(5),
                UrlFotoPerfil = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Biografia = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                ReputacionPromedio = reader.GetDecimal(8),
                CuentaVerificada = reader.GetBoolean(9),
                FechaUltimoLogin = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                Rol = reader.GetString(11),
                Comuna = new Comuna
                {
                    ComunaId = reader.GetInt32(5),
                    NombreComuna = reader.GetString(12),
                    Region = new Region
                    {
                        NombreRegion = reader.GetString(13)
                    }
                }
            };
        }
        return null;
    }

    // Obtener todas las regiones
    public async Task<List<Region>> ObtenerRegiones()
    {
        try
        {
            var regiones = new List<Region>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT region_id, nombre_region, codigo_region FROM Region ORDER BY nombre_region";
            using var command = new SqlCommand(query, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                regiones.Add(new Region
                {
                    RegionId = reader.GetInt32(0),
                    NombreRegion = reader.GetString(1),
                    CodigoRegion = reader.GetString(2)
                });
            }
            return regiones;
        }
        catch
        {
            // Fallback to sample data if database is not available
            return new List<Region>
            {
                new Region { RegionId = 1, NombreRegion = "Región Metropolitana de Santiago", CodigoRegion = "RM" },
                new Region { RegionId = 2, NombreRegion = "Región de Valparaíso", CodigoRegion = "V" },
                new Region { RegionId = 3, NombreRegion = "Región del Libertador General Bernardo O'Higgins", CodigoRegion = "VI" },
                new Region { RegionId = 4, NombreRegion = "Región del Maule", CodigoRegion = "VII" },
                new Region { RegionId = 5, NombreRegion = "Región del Biobío", CodigoRegion = "VIII" },
                new Region { RegionId = 6, NombreRegion = "Región de la Araucanía", CodigoRegion = "IX" },
                new Region { RegionId = 7, NombreRegion = "Región de Los Lagos", CodigoRegion = "X" },
                new Region { RegionId = 8, NombreRegion = "Región Aysén del General Carlos Ibáñez del Campo", CodigoRegion = "XI" },
                new Region { RegionId = 9, NombreRegion = "Región de Magallanes y de la Antártica Chilena", CodigoRegion = "XII" },
                new Region { RegionId = 10, NombreRegion = "Región de Los Ríos", CodigoRegion = "XIV" },
                new Region { RegionId = 11, NombreRegion = "Región de Arica y Parinacota", CodigoRegion = "XV" },
                new Region { RegionId = 12, NombreRegion = "Región de Tarapacá", CodigoRegion = "I" },
                new Region { RegionId = 13, NombreRegion = "Región de Antofagasta", CodigoRegion = "II" },
                new Region { RegionId = 14, NombreRegion = "Región de Atacama", CodigoRegion = "III" },
                new Region { RegionId = 15, NombreRegion = "Región de Coquimbo", CodigoRegion = "IV" },
                new Region { RegionId = 16, NombreRegion = "Región del Ñuble", CodigoRegion = "XVI" }
            };
        }
    }

    // Obtener comunas por región
    public async Task<List<Comuna>> ObtenerComunasPorRegion(int regionId)
    {
        try
        {
            var comunas = new List<Comuna>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT comuna_id, nombre_comuna, codigo_comuna FROM Comuna WHERE region_id = @RegionId ORDER BY nombre_comuna";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RegionId", regionId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comunas.Add(new Comuna
                {
                    ComunaId = reader.GetInt32(0),
                    RegionId = regionId,
                    NombreComuna = reader.GetString(1),
                    CodigoComuna = reader.GetString(2)
                });
            }
            return comunas;
        }
        catch
        {
            // Fallback to sample data if database is not available
            var comunasFallback = new List<Comuna>();

            // Sample comunas based on regionId
            switch (regionId)
            {
                case 1: // Región Metropolitana
                    comunasFallback.AddRange(new[] {
                        new Comuna { ComunaId = 1, RegionId = 1, NombreComuna = "Santiago", CodigoComuna = "13101" },
                        new Comuna { ComunaId = 2, RegionId = 1, NombreComuna = "Providencia", CodigoComuna = "13123" },
                        new Comuna { ComunaId = 3, RegionId = 1, NombreComuna = "Las Condes", CodigoComuna = "13114" },
                        new Comuna { ComunaId = 4, RegionId = 1, NombreComuna = "Ñuñoa", CodigoComuna = "13120" },
                        new Comuna { ComunaId = 5, RegionId = 1, NombreComuna = "La Florida", CodigoComuna = "13110" }
                    });
                    break;
                case 2: // Región de Valparaíso
                    comunasFallback.AddRange(new[] {
                        new Comuna { ComunaId = 6, RegionId = 2, NombreComuna = "Valparaíso", CodigoComuna = "05101" },
                        new Comuna { ComunaId = 7, RegionId = 2, NombreComuna = "Viña del Mar", CodigoComuna = "05103" },
                        new Comuna { ComunaId = 8, RegionId = 2, NombreComuna = "Quilpué", CodigoComuna = "05109" }
                    });
                    break;
                case 3: // Región del Libertador General Bernardo O'Higgins
                    comunasFallback.AddRange(new[] {
                        new Comuna { ComunaId = 9, RegionId = 3, NombreComuna = "Rancagua", CodigoComuna = "06101" },
                        new Comuna { ComunaId = 10, RegionId = 3, NombreComuna = "Rengo", CodigoComuna = "06113" }
                    });
                    break;
                case 4: // Región del Maule
                    comunasFallback.AddRange(new[] {
                        new Comuna { ComunaId = 11, RegionId = 4, NombreComuna = "Talca", CodigoComuna = "07101" },
                        new Comuna { ComunaId = 12, RegionId = 4, NombreComuna = "Curicó", CodigoComuna = "07104" }
                    });
                    break;
                case 5: // Región del Biobío
                    comunasFallback.AddRange(new[] {
                        new Comuna { ComunaId = 13, RegionId = 5, NombreComuna = "Concepción", CodigoComuna = "08101" },
                        new Comuna { ComunaId = 14, RegionId = 5, NombreComuna = "Talcahuano", CodigoComuna = "08108" }
                    });
                    break;
                default:
                    comunasFallback.Add(new Comuna { ComunaId = 999, RegionId = regionId, NombreComuna = "Comuna de ejemplo", CodigoComuna = "00000" });
                    break;
            }

            return comunasFallback;
        }
    }

    // SesionAdministrador methods
    public async Task<int> CrearSesionAdministrador(SesionAdministrador sesion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO SesionAdministrador (administrador_id, token_sesion, fecha_inicio, fecha_ultima_actividad, fecha_expiracion, activa)
            VALUES (@AdminId, @Token, @FechaInicio, @FechaActividad, @FechaExpiracion, @Activa);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AdminId", sesion.AdministradorId);
        command.Parameters.AddWithValue("@Token", sesion.TokenSesion);
        command.Parameters.AddWithValue("@FechaInicio", sesion.FechaInicio);
        command.Parameters.AddWithValue("@FechaActividad", sesion.FechaUltimaActividad);
        command.Parameters.AddWithValue("@FechaExpiracion", sesion.FechaExpiracion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Activa", sesion.Activa);

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return id;
    }

    public async Task ActualizarActividadSesionAdministrador(int administradorId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE SesionAdministrador SET fecha_ultima_actividad = @Fecha WHERE administrador_id = @AdminId AND activa = 1";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Fecha", DateTime.Now);
        command.Parameters.AddWithValue("@AdminId", administradorId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> VerificarSesionAdministradorActiva(int administradorId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT COUNT(*) FROM SesionAdministrador
            WHERE administrador_id = @AdminId AND activa = 1
            AND (fecha_expiracion IS NULL OR fecha_expiracion > @Now)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AdminId", administradorId);
        command.Parameters.AddWithValue("@Now", DateTime.Now);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task DesactivarSesionAdministrador(int administradorId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE SesionAdministrador SET activa = 0 WHERE administrador_id = @AdminId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AdminId", administradorId);

        await command.ExecuteNonQueryAsync();
    }

    // Token de verificación - guardar
    public async Task GuardarTokenVerificacion(int usuarioId, string token, DateTime expiracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET token_verificacion = @Token,
                token_verificacion_expiracion = @Expiracion
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@Expiracion", expiracion);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Token de verificación - validar
    public async Task<bool> ValidarTokenVerificacion(int usuarioId, string token)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT COUNT(*) FROM Usuario
            WHERE usuario_id = @UsuarioId
                AND token_verificacion = @Token
                AND token_verificacion_expiracion > @Now
                AND cuenta_verificada = 0";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@Now", DateTime.Now);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    // Verificar perfil completo (RF-03)
    public async Task<bool> PerfilCompleto(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT CASE
                WHEN url_foto_perfil IS NOT NULL AND url_foto_perfil != ''
                AND biografia IS NOT NULL AND biografia != ''
                THEN 1 ELSE 0 END
            FROM Usuario WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        var result = await command.ExecuteScalarAsync();
        return result != null && Convert.ToInt32(result) == 1;
    }

    // Marcar onboarding completado
    public async Task MarcarOnboardingCompletado(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Usuario SET onboarding_completado = 1 WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Verificar si onboarding está completado
    public async Task<bool> OnboardingCompletado(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT ISNULL(onboarding_completado, 0) FROM Usuario WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        var result = await command.ExecuteScalarAsync();
        return result != null && Convert.ToBoolean(result);
    }

    // Obtener usuario por email (para verificación)
    public async Task<int?> ObtenerUsuarioIdPorEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT usuario_id FROM Usuario WHERE correo_electronico = @Email";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : null;
    }

    // Actualizar perfil completo con todos los campos
    public async Task ActualizarPerfilCompleto(int usuarioId, string urlFotoPerfil, string biografia, string numeroTelefono)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET url_foto_perfil = @Foto,
                biografia = @Bio,
                numero_telefono = @Telefono
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Foto", urlFotoPerfil ?? string.Empty);
        command.Parameters.AddWithValue("@Bio", biografia ?? string.Empty);
        command.Parameters.AddWithValue("@Telefono", numeroTelefono ?? string.Empty);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Obtener estado del usuario
    public async Task<string?> ObtenerEstadoUsuario(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT estado_usuario FROM Usuario WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }

    // Token de recuperación de contraseña - guardar
    public async Task GuardarTokenRecuperacion(int usuarioId, string token, DateTime expiracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET token_recuperacion = @Token,
                token_recuperacion_expiracion = @Expiracion
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@Expiracion", expiracion);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Token de recuperación - validar
    public async Task<int?> ValidarTokenRecuperacion(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT usuario_id FROM Usuario
            WHERE token_recuperacion = @Token
                AND token_recuperacion_expiracion > @Now";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@Now", DateTime.Now);

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : null;
    }

    // Actualizar contraseña
    public async Task ActualizarPassword(int usuarioId, string passwordHash)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET password_hash = @PasswordHash,
                token_recuperacion = NULL,
                token_recuperacion_expiracion = NULL
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // Limpiar token de verificación después de verificar
    public async Task LimpiarTokenVerificacion(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET token_verificacion = NULL,
                token_verificacion_expiracion = NULL,
                cuenta_verificada = 1,
                fecha_verificacion = @Fecha
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Fecha", DateTime.Now);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // RF-17: Obtener secreto TOTP del administrador para 2FA
    public async Task<string?> ObtenerTotpSecretAdministrador(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT totp_secret FROM Usuario WHERE usuario_id = @UsuarioId AND rol = 'administrador'";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? result?.ToString() : null;
    }

    // RF-17: Guardar secreto TOTP para administrador
    public async Task GuardarTotpSecretAdministrador(int usuarioId, string totpSecret)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Usuario
            SET totp_secret = @TotpSecret
            WHERE usuario_id = @UsuarioId AND rol = 'administrador'";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TotpSecret", totpSecret);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        await command.ExecuteNonQueryAsync();
    }

    // RF-17: Obtener todos los usuarios para panel de administración
    public async Task<List<UsuarioAdmin>> ObtenerTodosLosUsuarios()
    {
        var usuarios = new List<UsuarioAdmin>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT
                u.usuario_id,
                u.nombre,
                u.apellido,
                u.correo_electronico,
                u.reputacion_promedio,
                u.cuenta_verificada,
                u.fecha_registro,
                u.fecha_ultimo_login,
                u.cuenta_activa,
                (SELECT COUNT(*) FROM PropuestaTrueque pt
                 WHERE pt.estado_propuesta_id = 4
                 AND (pt.usuario_proponente_id = u.usuario_id
                      OR EXISTS (SELECT 1 FROM Prenda p WHERE p.prenda_id = pt.prenda_solicitada_id AND p.usuario_id = u.usuario_id))
                ) as trueques_completados
            FROM Usuario u
            WHERE u.rol = 'usuario'
            ORDER BY u.fecha_registro DESC";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usuarios.Add(new UsuarioAdmin
            {
                UsuarioId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellido = reader.GetString(2),
                CorreoElectronico = reader.GetString(3),
                PromedioCalificacion = reader.IsDBNull(4) ? 0 : Convert.ToDouble(reader.GetDecimal(4)),
                CuentaVerificada = reader.GetBoolean(5),
                FechaRegistro = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6),
                FechaUltimoLogin = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                CuentaActiva = reader.IsDBNull(8) ? true : reader.GetBoolean(8),
                TruequesCompletados = reader.GetInt32(9)
            });
        }

        return usuarios;
    }

    // RF-17: Cambiar estado de cuenta de usuario (activar/suspender)
    public async Task<bool> CambiarEstadoCuenta(int usuarioId, bool activa)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Usuario SET cuenta_activa = @Activa WHERE usuario_id = @UsuarioId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Activa", activa);
        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-17: Obtener estadísticas para dashboard de administración
    public async Task<EstadisticasAdmin> ObtenerEstadisticasAdmin()
    {
        var stats = new EstadisticasAdmin();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var inicioMesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var inicioMesAnterior = inicioMesActual.AddMonths(-1);

        var query = @"
            -- Usuarios totales
            SELECT COUNT(*) FROM Usuario WHERE rol = 'usuario';

            -- Usuarios nuevos este mes
            SELECT COUNT(*) FROM Usuario WHERE rol = 'usuario' AND fecha_registro >= @InicioMesActual;

            -- Usuarios mes anterior
            SELECT COUNT(*) FROM Usuario WHERE rol = 'usuario'
                AND fecha_registro >= @InicioMesAnterior AND fecha_registro < @InicioMesActual;

            -- Prendas totales disponibles
            SELECT COUNT(*) FROM Prenda WHERE estado_publicacion_id = 1;

            -- Prendas nuevas este mes
            SELECT COUNT(*) FROM Prenda WHERE fecha_publicacion >= @InicioMesActual;

            -- Prendas mes anterior
            SELECT COUNT(*) FROM Prenda
                WHERE fecha_publicacion >= @InicioMesAnterior AND fecha_publicacion < @InicioMesActual;

            -- Trueques completados total
            SELECT COUNT(*) FROM PropuestaTrueque WHERE estado_propuesta_id = 4;

            -- Trueques este mes
            SELECT COUNT(*) FROM PropuestaTrueque
                WHERE estado_propuesta_id = 4 AND fecha_propuesta >= @InicioMesActual;

            -- Trueques mes anterior
            SELECT COUNT(*) FROM PropuestaTrueque
                WHERE estado_propuesta_id = 4
                AND fecha_propuesta >= @InicioMesAnterior AND fecha_propuesta < @InicioMesActual;

            -- Reportes pendientes (propuestas con estado reportado si existe)
            SELECT COUNT(*) FROM PropuestaTrueque WHERE estado_propuesta_id = 5;";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@InicioMesActual", inicioMesActual);
        command.Parameters.AddWithValue("@InicioMesAnterior", inicioMesAnterior);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync()) stats.UsuariosTotales = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.UsuariosNuevosMes = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.UsuariosMesAnterior = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.PrendasTotales = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.PrendasNuevasMes = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.PrendasMesAnterior = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.TruequesCompletados = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.TruequesNuevosMes = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.TruequesMesAnterior = reader.GetInt32(0);
        if (await reader.NextResultAsync() && await reader.ReadAsync()) stats.ReportesPendientes = reader.GetInt32(0);

        return stats;
    }

    // RF-17: Obtener actividad reciente para dashboard
    public async Task<List<ActividadReciente>> ObtenerActividadReciente(int limite = 10)
    {
        var actividades = new List<ActividadReciente>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT TOP (@Limite)
                'trueque' as tipo,
                CONCAT('Trueque completado entre ', u1.nombre, ' y ', u2.nombre) as descripcion,
                pt.fecha_propuesta as fecha
            FROM PropuestaTrueque pt
            INNER JOIN Usuario u1 ON pt.usuario_proponente_id = u1.usuario_id
            INNER JOIN Prenda p ON pt.prenda_solicitada_id = p.prenda_id
            INNER JOIN Usuario u2 ON p.usuario_id = u2.usuario_id
            WHERE pt.estado_propuesta_id = 4
            UNION ALL
            SELECT TOP (@Limite)
                'usuario' as tipo,
                CONCAT('Nuevo usuario registrado: ', nombre, ' ', apellido) as descripcion,
                fecha_registro as fecha
            FROM Usuario
            WHERE rol = 'usuario'
            UNION ALL
            SELECT TOP (@Limite)
                'prenda' as tipo,
                CONCAT('Nueva prenda publicada: ', titulo_publicacion) as descripcion,
                fecha_publicacion as fecha
            FROM Prenda
            ORDER BY fecha DESC";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Limite", limite);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            actividades.Add(new ActividadReciente
            {
                Tipo = reader.GetString(0),
                Descripcion = reader.GetString(1),
                Fecha = reader.GetDateTime(2)
            });
        }

        return actividades.OrderByDescending(a => a.Fecha).Take(limite).ToList();
    }
}

// DTO para administración de usuarios
public class UsuarioAdmin
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public double PromedioCalificacion { get; set; }
    public bool CuentaVerificada { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaUltimoLogin { get; set; }
    public bool CuentaActiva { get; set; }
    public int TruequesCompletados { get; set; }
}

// DTO para estadísticas del dashboard
public class EstadisticasAdmin
{
    public int UsuariosTotales { get; set; }
    public int UsuariosNuevosMes { get; set; }
    public int UsuariosMesAnterior { get; set; }
    public int PrendasTotales { get; set; }
    public int PrendasNuevasMes { get; set; }
    public int PrendasMesAnterior { get; set; }
    public int TruequesCompletados { get; set; }
    public int TruequesNuevosMes { get; set; }
    public int TruequesMesAnterior { get; set; }
    public int ReportesPendientes { get; set; }
}

// DTO para actividad reciente
public class ActividadReciente
{
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
