using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared.Repositories;

public class ReporteSharedRepository : IReporteSharedRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public ReporteSharedRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<Reporte>> GetReportesAsync()
    {
        var reportes = new List<Reporte>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.reporte_id, r.usuario_reportador_id, r.usuario_reportado_id,
                   r.prenda_reportada_id, r.categoria_reporte_id, r.descripcion,
                   r.evidencia_url, r.estado_reporte, r.fecha_reporte,
                   r.fecha_resolucion, r.administrador_id, r.comentario_resolucion,
                   cr.nombre_categoria
            FROM Reporte r
            LEFT JOIN CategoriaReporte cr ON r.categoria_reporte_id = cr.categoria_reporte_id
            ORDER BY r.fecha_reporte DESC";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            reportes.Add(MapReporteFromReader(reader));
        }

        return reportes;
    }

    public async Task<Reporte?> GetReporteByIdAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.reporte_id, r.usuario_reportador_id, r.usuario_reportado_id,
                   r.prenda_reportada_id, r.categoria_reporte_id, r.descripcion,
                   r.evidencia_url, r.estado_reporte, r.fecha_reporte,
                   r.fecha_resolucion, r.administrador_id, r.comentario_resolucion,
                   cr.nombre_categoria
            FROM Reporte r
            LEFT JOIN CategoriaReporte cr ON r.categoria_reporte_id = cr.categoria_reporte_id
            WHERE r.reporte_id = @ReporteId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ReporteId", SqlDbType.Int) { Value = id });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapReporteFromReader(reader);
        }

        return null;
    }

    public async Task<List<Reporte>> GetReportesByUsuarioReportanteIdAsync(int usuarioId)
    {
        var reportes = new List<Reporte>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.reporte_id, r.usuario_reportador_id, r.usuario_reportado_id,
                   r.prenda_reportada_id, r.categoria_reporte_id, r.descripcion,
                   r.evidencia_url, r.estado_reporte, r.fecha_reporte,
                   r.fecha_resolucion, r.administrador_id, r.comentario_resolucion,
                   cr.nombre_categoria
            FROM Reporte r
            LEFT JOIN CategoriaReporte cr ON r.categoria_reporte_id = cr.categoria_reporte_id
            WHERE r.usuario_reportador_id = @UsuarioId
            ORDER BY r.fecha_reporte DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reportes.Add(MapReporteFromReader(reader));
        }

        return reportes;
    }

    public async Task<List<Reporte>> GetReportesByUsuarioReportadoIdAsync(int usuarioId)
    {
        var reportes = new List<Reporte>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.reporte_id, r.usuario_reportador_id, r.usuario_reportado_id,
                   r.prenda_reportada_id, r.categoria_reporte_id, r.descripcion,
                   r.evidencia_url, r.estado_reporte, r.fecha_reporte,
                   r.fecha_resolucion, r.administrador_id, r.comentario_resolucion,
                   cr.nombre_categoria
            FROM Reporte r
            LEFT JOIN CategoriaReporte cr ON r.categoria_reporte_id = cr.categoria_reporte_id
            WHERE r.usuario_reportado_id = @UsuarioId
            ORDER BY r.fecha_reporte DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reportes.Add(MapReporteFromReader(reader));
        }

        return reportes;
    }

    public async Task<List<Reporte>> GetReportesByEstadoAsync(string estado)
    {
        var reportes = new List<Reporte>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.reporte_id, r.usuario_reportador_id, r.usuario_reportado_id,
                   r.prenda_reportada_id, r.categoria_reporte_id, r.descripcion,
                   r.evidencia_url, r.estado_reporte, r.fecha_reporte,
                   r.fecha_resolucion, r.administrador_id, r.comentario_resolucion,
                   cr.nombre_categoria
            FROM Reporte r
            LEFT JOIN CategoriaReporte cr ON r.categoria_reporte_id = cr.categoria_reporte_id
            WHERE r.estado_reporte = @Estado
            ORDER BY r.fecha_reporte DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = estado });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reportes.Add(MapReporteFromReader(reader));
        }

        return reportes;
    }

    public async Task<int> CreateReporteAsync(Reporte reporte)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO Reporte (usuario_reportador_id, usuario_reportado_id, prenda_reportada_id,
                                 categoria_reporte_id, descripcion, evidencia_url,
                                 estado_reporte, fecha_reporte)
            VALUES (@UsuarioReportadorId, @UsuarioReportadoId, @PrendaReportadaId,
                    @CategoriaReporteId, @Descripcion, @EvidenciaUrl,
                    @EstadoReporte, GETDATE());
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioReportadorId", SqlDbType.Int) { Value = reporte.UsuarioReportadorId });
        command.Parameters.Add(new SqlParameter("@UsuarioReportadoId", SqlDbType.Int) { Value = (object?)reporte.UsuarioReportadoId ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@PrendaReportadaId", SqlDbType.Int) { Value = (object?)reporte.PrendaReportadaId ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@CategoriaReporteId", SqlDbType.Int) { Value = reporte.CategoriaReporteId });
        command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar, 1000) { Value = reporte.Descripcion });
        command.Parameters.Add(new SqlParameter("@EvidenciaUrl", SqlDbType.VarChar, 500) { Value = (object?)reporte.EvidenciaUrl ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@EstadoReporte", SqlDbType.VarChar, 20) { Value = "pendiente" });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> ActualizarEstadoAsync(int id, string estado)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Reporte
            SET estado_reporte = @Estado,
                fecha_resolucion = CASE WHEN @Estado IN ('resuelto', 'descartado') THEN GETDATE() ELSE fecha_resolucion END
            WHERE reporte_id = @ReporteId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ReporteId", SqlDbType.Int) { Value = id });
        command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = estado });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteReporteAsync(int id)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "DELETE FROM Reporte WHERE reporte_id = @ReporteId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ReporteId", SqlDbType.Int) { Value = id });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Reporte MapReporteFromReader(SqlDataReader reader)
    {
        return new Reporte
        {
            ReporteId = reader.GetInt32(reader.GetOrdinal("reporte_id")),
            UsuarioReportadorId = reader.GetInt32(reader.GetOrdinal("usuario_reportador_id")),
            UsuarioReportadoId = reader.IsDBNull(reader.GetOrdinal("usuario_reportado_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("usuario_reportado_id")),
            PrendaReportadaId = reader.IsDBNull(reader.GetOrdinal("prenda_reportada_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("prenda_reportada_id")),
            CategoriaReporteId = reader.GetInt32(reader.GetOrdinal("categoria_reporte_id")),
            Descripcion = reader.GetString(reader.GetOrdinal("descripcion")),
            EvidenciaUrl = reader.IsDBNull(reader.GetOrdinal("evidencia_url"))
                ? null
                : reader.GetString(reader.GetOrdinal("evidencia_url")),
            EstadoReporteStr = reader.GetString(reader.GetOrdinal("estado_reporte")),
            FechaReporte = reader.GetDateTime(reader.GetOrdinal("fecha_reporte")),
            FechaResolucion = reader.IsDBNull(reader.GetOrdinal("fecha_resolucion"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("fecha_resolucion")),
            AdministradorId = reader.IsDBNull(reader.GetOrdinal("administrador_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("administrador_id")),
            ComentarioResolucion = reader.IsDBNull(reader.GetOrdinal("comentario_resolucion"))
                ? null
                : reader.GetString(reader.GetOrdinal("comentario_resolucion")),
            Categoria = !reader.IsDBNull(reader.GetOrdinal("nombre_categoria"))
                ? new CategoriaReporte
                {
                    CategoriaReporteId = reader.GetInt32(reader.GetOrdinal("categoria_reporte_id")),
                    NombreCategoria = reader.GetString(reader.GetOrdinal("nombre_categoria"))
                }
                : null
        };
    }
}
