using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.CompletarPerfil.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.CompletarPerfil;

public class CompletarPerfilRepository : ICompletarPerfilRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public CompletarPerfilRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<bool> PerfilCompleto(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT CASE
                WHEN url_foto_perfil IS NOT NULL AND url_foto_perfil != ''
                 AND biografia IS NOT NULL AND biografia != ''
                THEN 1 ELSE 0 END
            FROM Usuario WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value && (int)result == 1;
    }

    public async Task ActualizarPerfil(int usuarioId, string urlFotoPerfil, string biografia)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Usuario
            SET url_foto_perfil = @Foto, biografia = @Bio
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@Foto", SqlDbType.NVarChar, 500) { Value = urlFotoPerfil });
        command.Parameters.Add(new SqlParameter("@Bio", SqlDbType.NVarChar, 1000) { Value = biografia });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.correo_electronico, u.nombre, u.apellido,
                   u.url_foto_perfil, u.biografia, u.numero_telefono, u.comuna_id,
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
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("apellido")),
                UrlFotoPerfil = reader.IsDBNull(reader.GetOrdinal("url_foto_perfil"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("url_foto_perfil")),
                Biografia = reader.IsDBNull(reader.GetOrdinal("biografia"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("biografia")),
                NumeroTelefono = reader.IsDBNull(reader.GetOrdinal("numero_telefono"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("numero_telefono")),
                ComunaId = reader.GetInt32(reader.GetOrdinal("comuna_id")),
                Comuna = reader.IsDBNull(reader.GetOrdinal("nombre_comuna")) ? null! : new Comuna
                {
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
