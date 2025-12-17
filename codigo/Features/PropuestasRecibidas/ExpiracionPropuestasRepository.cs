using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.PropuestasRecibidas.Interfaces;
using TruequeTextil.Shared.Infrastructure;

namespace TruequeTextil.Features.PropuestasRecibidas;

/// <summary>
/// Repositorio para operaciones de expiración de propuestas de trueque (RF-11)
/// </summary>
public class ExpiracionPropuestasRepository : IExpiracionPropuestasRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public ExpiracionPropuestasRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    /// <summary>
    /// RF-11: Marca como expiradas las propuestas pendientes que superaron los 7 días sin respuesta
    /// </summary>
    public async Task<int> MarcarPropuestasExpiradas()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE PropuestaTrueque
            SET estado_propuesta_id = 5
            WHERE estado_propuesta_id = 1
              AND fecha_expiracion IS NOT NULL
              AND fecha_expiracion < GETDATE()";

        using var command = new SqlCommand(sql, connection);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Obtiene información de las propuestas que fueron marcadas como expiradas recientemente
    /// para enviar notificaciones a los usuarios afectados
    /// </summary>
    public async Task<List<(int PropuestaId, int UsuarioProponenteId, int UsuarioReceptorId, string TituloPrenda)>> ObtenerPropuestasRecienExpiradas()
    {
        var propuestas = new List<(int, int, int, string)>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Obtener propuestas que expiraron en las últimas 24 horas
        const string sql = @"
            SELECT
                pt.propuesta_id,
                pt.usuario_proponente_id,
                p_sol.usuario_id AS usuario_receptor_id,
                p_sol.titulo_publicacion
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id
                AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            WHERE pt.estado_propuesta_id = 5
              AND pt.fecha_expiracion IS NOT NULL
              AND pt.fecha_expiracion >= DATEADD(hour, -24, GETDATE())
              AND pt.fecha_expiracion < GETDATE()";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            propuestas.Add((
                reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                reader.GetInt32(reader.GetOrdinal("usuario_proponente_id")),
                reader.GetInt32(reader.GetOrdinal("usuario_receptor_id")),
                reader.GetString(reader.GetOrdinal("titulo_publicacion"))
            ));
        }

        return propuestas;
    }

    /// <summary>
    /// RF-11: Obtiene las propuestas que están próximas a expirar (menos de 48 horas)
    /// para enviar notificaciones de recordatorio al receptor
    /// </summary>
    public async Task<List<(int PropuestaId, int UsuarioReceptorId, string TituloPrenda, DateTime FechaExpiracion)>> ObtenerPropuestasProximasAExpirar()
    {
        var propuestas = new List<(int, int, string, DateTime)>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Obtener propuestas pendientes que expiran en menos de 48 horas
        const string sql = @"
            SELECT
                pt.propuesta_id,
                p_sol.usuario_id AS usuario_receptor_id,
                p_sol.titulo_publicacion,
                pt.fecha_expiracion
            FROM PropuestaTrueque pt
            INNER JOIN DetallePropuesta dp_sol ON pt.propuesta_id = dp_sol.propuesta_id
                AND dp_sol.tipo_participacion = 'solicitada'
            INNER JOIN Prenda p_sol ON dp_sol.prenda_id = p_sol.prenda_id
            WHERE pt.estado_propuesta_id = 1
              AND pt.fecha_expiracion IS NOT NULL
              AND pt.fecha_expiracion > GETDATE()
              AND pt.fecha_expiracion <= DATEADD(hour, 48, GETDATE())";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            propuestas.Add((
                reader.GetInt32(reader.GetOrdinal("propuesta_id")),
                reader.GetInt32(reader.GetOrdinal("usuario_receptor_id")),
                reader.GetString(reader.GetOrdinal("titulo_publicacion")),
                reader.GetDateTime(reader.GetOrdinal("fecha_expiracion"))
            ));
        }

        return propuestas;
    }
}
