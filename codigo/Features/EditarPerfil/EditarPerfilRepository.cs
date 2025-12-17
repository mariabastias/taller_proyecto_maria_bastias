using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.EditarPerfil.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPerfil;

public class EditarPerfilRepository : IEditarPerfilRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public EditarPerfilRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.correo_electronico, u.nombre, u.apellido,
                   u.url_foto_perfil, u.biografia, u.numero_telefono, u.comuna_id,
                   c.nombre_comuna, c.region_id, r.nombre_region
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
                    ComunaId = reader.GetInt32(reader.GetOrdinal("comuna_id")),
                    RegionId = reader.GetInt32(reader.GetOrdinal("region_id")),
                    NombreComuna = reader.GetString(reader.GetOrdinal("nombre_comuna")),
                    Region = reader.IsDBNull(reader.GetOrdinal("nombre_region")) ? null! : new Region
                    {
                        RegionId = reader.GetInt32(reader.GetOrdinal("region_id")),
                        NombreRegion = reader.GetString(reader.GetOrdinal("nombre_region"))
                    }
                }
            };
        }

        return null;
    }

    public async Task<List<Region>> ObtenerRegiones()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT region_id, nombre_region, codigo_region FROM Region ORDER BY nombre_region";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        var regiones = new List<Region>();
        while (await reader.ReadAsync())
        {
            regiones.Add(new Region
            {
                RegionId = reader.GetInt32(reader.GetOrdinal("region_id")),
                NombreRegion = reader.GetString(reader.GetOrdinal("nombre_region")),
                CodigoRegion = reader.IsDBNull(reader.GetOrdinal("codigo_region"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("codigo_region"))
            });
        }

        return regiones;
    }

    public async Task<List<Comuna>> ObtenerComunasPorRegion(int regionId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT comuna_id, region_id, nombre_comuna, codigo_comuna
            FROM Comuna
            WHERE region_id = @RegionId
            ORDER BY nombre_comuna";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@RegionId", SqlDbType.Int) { Value = regionId });

        using var reader = await command.ExecuteReaderAsync();

        var comunas = new List<Comuna>();
        while (await reader.ReadAsync())
        {
            comunas.Add(new Comuna
            {
                ComunaId = reader.GetInt32(reader.GetOrdinal("comuna_id")),
                RegionId = reader.GetInt32(reader.GetOrdinal("region_id")),
                NombreComuna = reader.GetString(reader.GetOrdinal("nombre_comuna")),
                CodigoComuna = reader.IsDBNull(reader.GetOrdinal("codigo_comuna"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("codigo_comuna"))
            });
        }

        return comunas;
    }

    public async Task ActualizarDatosBasicos(int usuarioId, string nombre, string apellido, int comunaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Usuario
            SET nombre = @Nombre, apellido = @Apellido, comuna_id = @ComunaId
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 100) { Value = nombre });
        command.Parameters.Add(new SqlParameter("@Apellido", SqlDbType.NVarChar, 100) { Value = apellido });
        command.Parameters.Add(new SqlParameter("@ComunaId", SqlDbType.Int) { Value = comunaId });

        await command.ExecuteNonQueryAsync();
    }

    // RF-03: Actualizar foto de perfil y biografía post-onboarding
    public async Task ActualizarFotoYBiografia(int usuarioId, string urlFotoPerfil, string biografia)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Usuario
            SET url_foto_perfil = @UrlFotoPerfil, biografia = @Biografia
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@UrlFotoPerfil", SqlDbType.NVarChar, 500) { Value = (object?)urlFotoPerfil ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Biografia", SqlDbType.NVarChar, 500) { Value = (object?)biografia ?? DBNull.Value });

        await command.ExecuteNonQueryAsync();
    }

    public async Task RegistrarCambioHistorial(int usuarioId, string campoModificado, string? valorAnterior, string? valorNuevo)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Verificar si la tabla existe antes de insertar
        const string checkTableSql = @"
            IF OBJECT_ID('HistorialPerfil', 'U') IS NOT NULL
            BEGIN
                INSERT INTO HistorialPerfil (usuario_id, campo_modificado, valor_anterior, valor_nuevo)
                VALUES (@UsuarioId, @CampoModificado, @ValorAnterior, @ValorNuevo)
            END";

        using var command = new SqlCommand(checkTableSql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
        command.Parameters.Add(new SqlParameter("@CampoModificado", SqlDbType.NVarChar, 50) { Value = campoModificado });
        command.Parameters.Add(new SqlParameter("@ValorAnterior", SqlDbType.NVarChar, 500) { Value = (object?)valorAnterior ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@ValorNuevo", SqlDbType.NVarChar, 500) { Value = (object?)valorNuevo ?? DBNull.Value });

        await command.ExecuteNonQueryAsync();
    }
}
