using System.Data.SqlClient;
using TruequeTextil.Features.ConfiguracionInterfaz.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.ConfiguracionInterfaz;

public class ConfiguracionInterfazRepository : IConfiguracionInterfazRepository
{
    private readonly string _connectionString;

    public ConfiguracionInterfazRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<ConfiguracionInterfazModel?> ObtenerConfiguracionPorUsuario(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT configuracion_id, usuario_id, tema_oscuro, notificaciones_sonido,
                   densidad_contenido, tamanio_fuente, idioma
            FROM ConfiguracionInterfaz
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ConfiguracionInterfazModel
            {
                ConfiguracionId = reader.GetInt32(reader.GetOrdinal("configuracion_id")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                TemaOscuro = reader.GetBoolean(reader.GetOrdinal("tema_oscuro")),
                NotificacionesSonido = reader.GetBoolean(reader.GetOrdinal("notificaciones_sonido")),
                DensidadContenido = reader.IsDBNull(reader.GetOrdinal("densidad_contenido"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("densidad_contenido")),
                TamanioFuente = reader.IsDBNull(reader.GetOrdinal("tamanio_fuente"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("tamanio_fuente")),
                Idioma = reader.IsDBNull(reader.GetOrdinal("idioma"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("idioma"))
            };
        }

        return null;
    }

    public async Task<bool> CrearConfiguracion(ConfiguracionInterfazModel configuracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO ConfiguracionInterfaz (usuario_id, tema_oscuro, notificaciones_sonido,
                                               densidad_contenido, tamanio_fuente, idioma)
            VALUES (@UsuarioId, @TemaOscuro, @NotificacionesSonido, @DensidadContenido,
                    @TamanioFuente, @Idioma)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = configuracion.UsuarioId });
        command.Parameters.Add(new SqlParameter("@TemaOscuro", SqlDbType.Bit) { Value = configuracion.TemaOscuro });
        command.Parameters.Add(new SqlParameter("@NotificacionesSonido", SqlDbType.Bit) { Value = configuracion.NotificacionesSonido });
        command.Parameters.Add(new SqlParameter("@DensidadContenido", SqlDbType.NVarChar, 50)
        { Value = configuracion.DensidadContenido as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@TamanioFuente", SqlDbType.NVarChar, 20)
        { Value = configuracion.TamanioFuente as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Idioma", SqlDbType.NVarChar, 10)
        { Value = configuracion.Idioma as object ?? DBNull.Value });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ActualizarConfiguracion(ConfiguracionInterfazModel configuracion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            UPDATE ConfiguracionInterfaz
            SET tema_oscuro = @TemaOscuro,
                notificaciones_sonido = @NotificacionesSonido,
                densidad_contenido = @DensidadContenido,
                tamanio_fuente = @TamanioFuente,
                idioma = @Idioma
            WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = configuracion.UsuarioId });
        command.Parameters.Add(new SqlParameter("@TemaOscuro", SqlDbType.Bit) { Value = configuracion.TemaOscuro });
        command.Parameters.Add(new SqlParameter("@NotificacionesSonido", SqlDbType.Bit) { Value = configuracion.NotificacionesSonido });
        command.Parameters.Add(new SqlParameter("@DensidadContenido", SqlDbType.NVarChar, 50)
        { Value = configuracion.DensidadContenido as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@TamanioFuente", SqlDbType.NVarChar, 20)
        { Value = configuracion.TamanioFuente as object ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Idioma", SqlDbType.NVarChar, 10)
        { Value = configuracion.Idioma as object ?? DBNull.Value });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> EliminarConfiguracion(int usuarioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM ConfiguracionInterfaz WHERE usuario_id = @UsuarioId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
