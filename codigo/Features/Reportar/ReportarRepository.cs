using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.Reportar.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;

namespace TruequeTextil.Features.Reportar;

public class ReportarRepository : IReportarRepository
{
    private readonly DatabaseConfig _databaseConfig;
    private readonly ReporteService _reporteService;
    private readonly UsuarioService _usuarioService;
    private readonly PrendaService _prendaService;

    // Categorías de reporte predefinidas
    private static readonly List<CategoriaReporteDTO> _categoriasUsuario = new()
    {
        new(1, "Comportamiento inapropiado", "Conducta irrespetuosa o agresiva durante la negociación", TipoReporte.Usuario),
        new(2, "Perfil falso o suplantación", "Usuario con información falsa o que suplanta identidad", TipoReporte.Usuario),
        new(3, "No cumple acuerdos", "No se presentó al intercambio o incumplió lo acordado", TipoReporte.Usuario),
        new(4, "Acoso o intimidación", "Mensajes amenazantes, acosadores o intimidatorios", TipoReporte.Usuario),
        new(5, "Fraude o estafa", "Intento de engaño o estafa en el trueque", TipoReporte.Usuario),
        new(6, "Spam o publicidad", "Envío de mensajes publicitarios no solicitados", TipoReporte.Usuario),
        new(7, "Otro", "Otro motivo no listado", TipoReporte.Usuario)
    };

    private static readonly List<CategoriaReporteDTO> _categoriasPrenda = new()
    {
        new(10, "Descripción engañosa", "La descripción no coincide con la prenda real", TipoReporte.Prenda),
        new(11, "Fotos falsas o robadas", "Las imágenes no corresponden a la prenda ofrecida", TipoReporte.Prenda),
        new(12, "Prenda en mal estado", "Estado real muy diferente al publicado", TipoReporte.Prenda),
        new(13, "Contenido inapropiado", "Imágenes o texto ofensivo o inapropiado", TipoReporte.Prenda),
        new(14, "Producto prohibido", "Artículo no permitido en la plataforma", TipoReporte.Prenda),
        new(15, "Duplicado o spam", "Publicación repetida o spam", TipoReporte.Prenda),
        new(16, "Otro", "Otro motivo no listado", TipoReporte.Prenda)
    };

    public ReportarRepository(
        DatabaseConfig databaseConfig,
        ReporteService reporteService,
        UsuarioService usuarioService,
        PrendaService prendaService)
    {
        _databaseConfig = databaseConfig;
        _reporteService = reporteService;
        _usuarioService = usuarioService;
        _prendaService = prendaService;
    }

    public async Task<int> CrearReporte(Reporte reporte)
    {
        // Intentar con base de datos SQL Server
        try
        {
            using var connection = _databaseConfig.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO Reporte (tipo, motivo, descripcion, usuario_reportante_id,
                                    usuario_reportado_id, prenda_reportada_id, fecha_creacion, estado)
                VALUES (@Tipo, @Motivo, @Descripcion, @UsuarioReportanteId,
                        @UsuarioReportadoId, @PrendaReportadaId, @FechaCreacion, @Estado);
                SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.NVarChar, 30) { Value = reporte.Tipo.ToString().ToLower() });
            command.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 100) { Value = reporte.Motivo });
            command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = (object?)reporte.Descripcion ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@UsuarioReportanteId", SqlDbType.Int) { Value = reporte.UsuarioReportanteId });
            command.Parameters.Add(new SqlParameter("@UsuarioReportadoId", SqlDbType.Int) { Value = (object?)reporte.UsuarioReportadoId ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@PrendaReportadaId", SqlDbType.Int) { Value = (object?)reporte.PrendaReportadaId ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@FechaCreacion", SqlDbType.DateTime) { Value = DateTime.Now });
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = "pendiente" });

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch
        {
            // Fallback a datos en memoria
            var nuevoReporte = await _reporteService.CreateReporteAsync(reporte);
            return nuevoReporte.ReporteId;
        }
    }

    public async Task<bool> ExisteReporteActivo(int usuarioReportadorId, int? usuarioReportadoId, int? prendaId)
    {
        // Intentar con base de datos SQL Server
        try
        {
            using var connection = _databaseConfig.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT COUNT(*) FROM Reporte
                WHERE usuario_reportante_id = @UsuarioReportanteId
                  AND estado IN ('pendiente', 'revisado')";

            if (usuarioReportadoId.HasValue)
            {
                sql += " AND usuario_reportado_id = @UsuarioReportadoId";
            }

            if (prendaId.HasValue)
            {
                sql += " AND prenda_reportada_id = @PrendaReportadaId";
            }

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@UsuarioReportanteId", SqlDbType.Int) { Value = usuarioReportadorId });

            if (usuarioReportadoId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@UsuarioReportadoId", SqlDbType.Int) { Value = usuarioReportadoId.Value });
            }

            if (prendaId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@PrendaReportadaId", SqlDbType.Int) { Value = prendaId.Value });
            }

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch
        {
            // Fallback a datos en memoria
            var reportes = await _reporteService.GetReportesAsync();
            return reportes.Any(r =>
                r.UsuarioReportanteId == usuarioReportadorId &&
                (r.Estado == EstadoReporte.Pendiente || r.Estado == EstadoReporte.Revisado) &&
                (usuarioReportadoId == null || r.UsuarioReportadoId == usuarioReportadoId) &&
                (prendaId == null || r.PrendaReportadaId == prendaId));
        }
    }

    public Task<List<CategoriaReporteDTO>> ObtenerCategoriasReporte(TipoReporte tipo)
    {
        var categorias = tipo == TipoReporte.Usuario ? _categoriasUsuario : _categoriasPrenda;
        return Task.FromResult(categorias);
    }

    public async Task<Usuario?> ObtenerUsuarioPorId(int usuarioId)
    {
        return await _usuarioService.GetUsuarioByIdAsync(usuarioId);
    }

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        return await _prendaService.GetPrendaByIdAsync(prendaId);
    }

    public async Task<List<Reporte>> ObtenerMisReportes(int usuarioId)
    {
        // Intentar con base de datos SQL Server
        try
        {
            var reportes = new List<Reporte>();
            using var connection = _databaseConfig.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
                SELECT r.reporte_id, r.tipo, r.motivo, r.descripcion,
                       r.usuario_reportante_id, r.usuario_reportado_id, r.prenda_reportada_id,
                       r.fecha_creacion, r.estado,
                       COALESCE(ure.nombre, '') AS reportado_nombre, COALESCE(ure.apellido, '') AS reportado_apellido,
                       COALESCE(p.titulo_publicacion, '') AS prenda_titulo
                FROM Reporte r
                LEFT JOIN Usuario ure ON r.usuario_reportado_id = ure.usuario_id
                LEFT JOIN Prenda p ON r.prenda_reportada_id = p.prenda_id
                WHERE r.usuario_reportante_id = @UsuarioId
                ORDER BY r.fecha_creacion DESC";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var reporte = new Reporte
                {
                    ReporteId = reader.GetInt32(0),
                    Tipo = ParseTipoReporte(reader.GetString(1)),
                    Motivo = reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    UsuarioReportanteId = reader.GetInt32(4),
                    UsuarioReportadoId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    PrendaReportadaId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    FechaCreacion = reader.GetDateTime(7),
                    Estado = ParseEstadoReporte(reader.GetString(8))
                };

                if (reporte.UsuarioReportadoId.HasValue)
                {
                    reporte.UsuarioReportado = new Usuario
                    {
                        UsuarioId = reporte.UsuarioReportadoId.Value,
                        Nombre = reader.GetString(9),
                        Apellido = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                    };
                }

                if (reporte.PrendaReportadaId.HasValue)
                {
                    reporte.PrendaReportada = new Prenda
                    {
                        PrendaId = reporte.PrendaReportadaId.Value,
                        TituloPublicacion = reader.GetString(11)
                    };
                }

                reportes.Add(reporte);
            }

            return reportes;
        }
        catch
        {
            // Fallback a datos en memoria
            return await _reporteService.GetReportesByUsuarioReportanteIdAsync(usuarioId);
        }
    }

    private static TipoReporte ParseTipoReporte(string tipo)
    {
        return tipo.ToLower() switch
        {
            "usuario" => TipoReporte.Usuario,
            "prenda" => TipoReporte.Prenda,
            "propuesta" => TipoReporte.Propuesta,
            _ => TipoReporte.Prenda
        };
    }

    private static EstadoReporte ParseEstadoReporte(string estado)
    {
        return estado.ToLower() switch
        {
            "pendiente" => EstadoReporte.Pendiente,
            "revisado" or "en_revision" => EstadoReporte.Revisado,
            "resuelto" => EstadoReporte.Resuelto,
            "desestimado" or "descartado" => EstadoReporte.Desestimado,
            _ => EstadoReporte.Pendiente
        };
    }
}
