using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Repositories;

public class UsuarioSharedRepository : IUsuarioSharedRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public UsuarioSharedRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.correo_electronico, u.password_hash, u.nombre, u.apellido, u.comuna_id,
                   u.url_foto_perfil, u.biografia, u.reputacion_promedio, u.cuenta_verificada,
                   u.fecha_ultimo_login, u.rol, u.estado_usuario,
                   c.nombre_comuna, r.nombre_region
            FROM Usuario u
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN Region r ON c.region_id = r.region_id
            WHERE u.usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                CorreoElectronico = reader.GetString(reader.GetOrdinal("correo_electronico")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("apellido")),
                ComunaId = reader.GetInt32(reader.GetOrdinal("comuna_id")),
                UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("url_foto_perfil")),
                Biografia = reader.IsDBNull(reader.GetOrdinal("biografia"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("biografia")),
                ReputacionPromedio = reader.GetDecimal(reader.GetOrdinal("reputacion_promedio")),
                CuentaVerificada = reader.GetBoolean(reader.GetOrdinal("cuenta_verificada")),
                FechaUltimoLogin = reader.IsDBNull(reader.GetOrdinal("fecha_ultimo_login"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_ultimo_login")),
                Rol = reader.GetString(reader.GetOrdinal("rol")),
                EstadoUsuario = reader.IsDBNull(reader.GetOrdinal("estado_usuario"))
                    ? "activo"
                    : reader.GetString(reader.GetOrdinal("estado_usuario")),
                Comuna = reader.IsDBNull(reader.GetOrdinal("nombre_comuna")) ? null! : new Comuna
                {
                    ComunaId = reader.GetInt32(reader.GetOrdinal("comuna_id")),
                    NombreComuna = reader.GetString(reader.GetOrdinal("nombre_comuna")),
                    Region = reader.IsDBNull(reader.GetOrdinal("nombre_region")) ? null! : new Region
                    {
                        NombreRegion = reader.GetString(reader.GetOrdinal("nombre_region"))
                    }
                }
            };
        }

        return null;
    }
}
