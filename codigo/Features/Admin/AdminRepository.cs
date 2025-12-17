namespace TruequeTextil.Features.Admin;

public class AdminRepository : IAdminRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public AdminRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    // RF-17: Obtener reportes con filtros y paginacion
    public async Task<List<Reporte>> ObtenerReportes(string? estado = null, string? tipo = null, int pagina = 1, int porPagina = 20)
    {
        var reportes = new List<Reporte>();
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT r.reporte_id, r.tipo, r.motivo, r.descripcion,
                   r.usuario_reportante_id, r.usuario_reportado_id, r.prenda_reportada_id,
                   r.fecha_creacion, r.estado,
                   ur.nombre AS reportante_nombre, ur.apellido AS reportante_apellido,
                   COALESCE(ure.nombre, '') AS reportado_nombre, COALESCE(ure.apellido, '') AS reportado_apellido,
                   COALESCE(p.titulo_publicacion, '') AS prenda_titulo
            FROM Reporte r
            INNER JOIN Usuario ur ON r.usuario_reportante_id = ur.usuario_id
            LEFT JOIN Usuario ure ON r.usuario_reportado_id = ure.usuario_id
            LEFT JOIN Prenda p ON r.prenda_reportada_id = p.prenda_id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            sql += " AND r.estado = @Estado";
        }

        if (!string.IsNullOrEmpty(tipo) && tipo != "todos")
        {
            sql += " AND r.tipo = @Tipo";
        }

        sql += @"
            ORDER BY r.fecha_creacion DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = (pagina - 1) * porPagina });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = porPagina });

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = estado });
        }

        if (!string.IsNullOrEmpty(tipo) && tipo != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.NVarChar, 30) { Value = tipo });
        }

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
                Estado = ParseEstadoReporte(reader.GetString(8)),
                UsuarioReportante = new Usuario
                {
                    Nombre = reader.GetString(9),
                    Apellido = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                }
            };

            if (reporte.UsuarioReportadoId.HasValue)
            {
                reporte.UsuarioReportado = new Usuario
                {
                    UsuarioId = reporte.UsuarioReportadoId.Value,
                    Nombre = reader.GetString(11),
                    Apellido = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
                };
            }

            if (reporte.PrendaReportadaId.HasValue)
            {
                reporte.PrendaReportada = new Prenda
                {
                    PrendaId = reporte.PrendaReportadaId.Value,
                    TituloPublicacion = reader.GetString(13)
                };
            }

            reportes.Add(reporte);
        }

        return reportes;
    }

    public async Task<int> ContarReportes(string? estado = null, string? tipo = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM Reporte WHERE 1=1";

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            sql += " AND estado = @Estado";
        }

        if (!string.IsNullOrEmpty(tipo) && tipo != "todos")
        {
            sql += " AND tipo = @Tipo";
        }

        using var command = new SqlCommand(sql, connection);

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = estado });
        }

        if (!string.IsNullOrEmpty(tipo) && tipo != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.NVarChar, 30) { Value = tipo });
        }

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<Reporte?> ObtenerReportePorId(int reporteId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.reporte_id, r.tipo, r.motivo, r.descripcion,
                   r.usuario_reportante_id, r.usuario_reportado_id, r.prenda_reportada_id,
                   r.fecha_creacion, r.estado
            FROM Reporte r
            WHERE r.reporte_id = @ReporteId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ReporteId", SqlDbType.Int) { Value = reporteId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Reporte
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
        }

        return null;
    }

    public async Task<bool> ActualizarEstadoReporte(int reporteId, string nuevoEstado, int adminId, string? comentario = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Actualizar estado
            const string sqlUpdate = @"
                UPDATE Reporte
                SET estado = @Estado, fecha_resolucion = GETDATE(), admin_resolutor_id = @AdminId
                WHERE reporte_id = @ReporteId";

            using (var command = new SqlCommand(sqlUpdate, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = nuevoEstado });
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@ReporteId", SqlDbType.Int) { Value = reporteId });
                await command.ExecuteNonQueryAsync();
            }

            // Registrar accion de moderacion
            const string sqlLog = @"
                INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                VALUES (@AdminId, 'cambio_estado_reporte', 'reporte', @ReporteId, @Descripcion, GETDATE())";

            using (var command = new SqlCommand(sqlLog, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@ReporteId", SqlDbType.Int) { Value = reporteId });
                command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = comentario ?? $"Estado cambiado a {nuevoEstado}" });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(int Pendientes, int Revisados, int Resueltos, int Desestimados)> ObtenerEstadisticasReportes()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT
                SUM(CASE WHEN estado = 'pendiente' THEN 1 ELSE 0 END) AS Pendientes,
                SUM(CASE WHEN estado = 'revisado' THEN 1 ELSE 0 END) AS Revisados,
                SUM(CASE WHEN estado = 'resuelto' THEN 1 ELSE 0 END) AS Resueltos,
                SUM(CASE WHEN estado = 'desestimado' THEN 1 ELSE 0 END) AS Desestimados
            FROM Reporte";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return (
                reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
            );
        }

        return (0, 0, 0, 0);
    }

    // Gestion de usuarios
    public async Task<List<Usuario>> ObtenerUsuarios(string? busqueda = null, string? estado = null, int pagina = 1, int porPagina = 20)
    {
        var usuarios = new List<Usuario>();
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT u.usuario_id, u.correo_electronico, u.nombre, u.apellido,
                   u.reputacion_promedio, u.cuenta_verificada, u.fecha_registro,
                   u.estado_usuario, u.rol, c.nombre_comuna
            FROM Usuario u
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(busqueda))
        {
            sql += " AND (u.nombre LIKE @Busqueda OR u.apellido LIKE @Busqueda OR u.correo_electronico LIKE @Busqueda)";
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            sql += " AND u.estado_usuario = @Estado";
        }

        sql += @"
            ORDER BY u.fecha_registro DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = (pagina - 1) * porPagina });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = porPagina });

        if (!string.IsNullOrEmpty(busqueda))
        {
            command.Parameters.Add(new SqlParameter("@Busqueda", SqlDbType.NVarChar, 100) { Value = $"%{busqueda}%" });
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = estado });
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            usuarios.Add(new Usuario
            {
                UsuarioId = reader.GetInt32(0),
                CorreoElectronico = reader.GetString(1),
                Nombre = reader.GetString(2),
                Apellido = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ReputacionPromedio = reader.GetDecimal(4),
                CuentaVerificada = reader.GetBoolean(5),
                FechaRegistro = reader.GetDateTime(6),
                EstadoUsuario = reader.GetString(7),
                Rol = reader.GetString(8),
                Comuna = new Comuna { NombreComuna = reader.IsDBNull(9) ? string.Empty : reader.GetString(9) }
            });
        }

        return usuarios;
    }

    public async Task<int> ContarUsuarios(string? busqueda = null, string? estado = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM Usuario WHERE 1=1";

        if (!string.IsNullOrEmpty(busqueda))
        {
            sql += " AND (nombre LIKE @Busqueda OR apellido LIKE @Busqueda OR correo_electronico LIKE @Busqueda)";
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            sql += " AND estado_usuario = @Estado";
        }

        using var command = new SqlCommand(sql, connection);

        if (!string.IsNullOrEmpty(busqueda))
        {
            command.Parameters.Add(new SqlParameter("@Busqueda", SqlDbType.NVarChar, 100) { Value = $"%{busqueda}%" });
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = estado });
        }

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> SuspenderUsuario(int usuarioId, int adminId, string motivo, int? diasSuspension = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlUpdate = "UPDATE Usuario SET estado_usuario = 'suspendido' WHERE usuario_id = @UsuarioId";
            using (var command = new SqlCommand(sqlUpdate, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            // Registrar accion
            const string sqlLog = @"
                INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                VALUES (@AdminId, 'suspension_usuario', 'usuario', @UsuarioId, @Motivo, GETDATE())";

            using (var command = new SqlCommand(sqlLog, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                command.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 500) { Value = motivo });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ReactivarUsuario(int usuarioId, int adminId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlUpdate = "UPDATE Usuario SET estado_usuario = 'activo' WHERE usuario_id = @UsuarioId";
            using (var command = new SqlCommand(sqlUpdate, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            const string sqlLog = @"
                INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                VALUES (@AdminId, 'reactivacion_usuario', 'usuario', @UsuarioId, 'Usuario reactivado', GETDATE())";

            using (var command = new SqlCommand(sqlLog, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> VerificarUsuario(int usuarioId, int adminId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlUpdate = "UPDATE Usuario SET cuenta_verificada = 1, fecha_verificacion = GETDATE() WHERE usuario_id = @UsuarioId";
            using (var command = new SqlCommand(sqlUpdate, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            const string sqlLog = @"
                INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                VALUES (@AdminId, 'verificacion_usuario', 'usuario', @UsuarioId, 'Usuario verificado manualmente', GETDATE())";

            using (var command = new SqlCommand(sqlLog, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Prenda>> ObtenerPrendasReportadas(int pagina = 1, int porPagina = 20)
    {
        var prendas = new List<Prenda>();
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT DISTINCT p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.talla, p.estado_publicacion_id, p.fecha_publicacion,
                   u.nombre, u.apellido,
                   (SELECT COUNT(*) FROM Reporte r WHERE r.prenda_reportada_id = p.prenda_id AND r.estado = 'pendiente') AS reportes_pendientes
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            WHERE EXISTS (SELECT 1 FROM Reporte r WHERE r.prenda_reportada_id = p.prenda_id)
            ORDER BY reportes_pendientes DESC, p.fecha_publicacion DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = (pagina - 1) * porPagina });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = porPagina });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(new Prenda
            {
                PrendaId = reader.GetInt32(0),
                TituloPublicacion = reader.GetString(1),
                DescripcionPublicacion = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Talla = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                EstadoPublicacionId = reader.GetInt32(4),
                FechaPublicacion = reader.GetDateTime(5),
                Usuario = new Usuario
                {
                    Nombre = reader.GetString(6),
                    Apellido = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                }
            });
        }

        return prendas;
    }

    public async Task<bool> DesactivarPrenda(int prendaId, int adminId, string motivo)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Estado 5 = eliminado_admin
            const string sqlUpdate = "UPDATE Prenda SET estado_publicacion_id = 5 WHERE prenda_id = @PrendaId";
            using (var command = new SqlCommand(sqlUpdate, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
                await command.ExecuteNonQueryAsync();
            }

            const string sqlLog = @"
                INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                VALUES (@AdminId, 'desactivacion_prenda', 'prenda', @PrendaId, @Motivo, GETDATE())";

            using (var command = new SqlCommand(sqlLog, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
                command.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 500) { Value = motivo });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(int TotalUsuarios, int TotalPrendas, int TotalTrueques, int UsuariosActivos)> ObtenerEstadisticasGenerales()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM Usuario) AS TotalUsuarios,
                (SELECT COUNT(*) FROM Prenda WHERE estado_publicacion_id = 1) AS TotalPrendas,
                (SELECT COUNT(*) FROM PropuestaTrueque WHERE estado_propuesta_id = 6) AS TotalTrueques,
                (SELECT COUNT(*) FROM Usuario WHERE fecha_ultimo_login >= DATEADD(day, -30, GETDATE())) AS UsuariosActivos";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return (
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3)
            );
        }

        return (0, 0, 0, 0);
    }

    // RF-17: Obtener prendas con filtros y paginacion
    public async Task<List<Prenda>> ObtenerPrendas(string? busqueda = null, string? estado = null, string? categoria = null, int pagina = 1, int porPagina = 20)
    {
        var prendas = new List<Prenda>();
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.talla, p.estado_publicacion_id, p.fecha_publicacion,
                   u.usuario_id, u.nombre, u.apellido,
                   c.categoria_id, c.nombre_categoria
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(busqueda))
        {
            sql += " AND (p.titulo_publicacion LIKE @Busqueda OR p.descripcion_publicacion LIKE @Busqueda)";
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            sql += " AND p.estado_publicacion_id = @Estado";
        }

        if (!string.IsNullOrEmpty(categoria) && categoria != "todos")
        {
            sql += " AND c.nombre_categoria = @Categoria";
        }

        sql += @"
            ORDER BY p.fecha_publicacion DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = (pagina - 1) * porPagina });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = porPagina });

        if (!string.IsNullOrEmpty(busqueda))
        {
            command.Parameters.Add(new SqlParameter("@Busqueda", SqlDbType.NVarChar, 200) { Value = $"%{busqueda}%" });
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.Int) { Value = int.Parse(estado) });
        }

        if (!string.IsNullOrEmpty(categoria) && categoria != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Categoria", SqlDbType.NVarChar, 50) { Value = categoria });
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(new Prenda
            {
                PrendaId = reader.GetInt32(0),
                TituloPublicacion = reader.GetString(1),
                DescripcionPublicacion = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Talla = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                EstadoPublicacionId = reader.GetInt32(4),
                FechaPublicacion = reader.GetDateTime(5),
                Usuario = new Usuario
                {
                    UsuarioId = reader.GetInt32(6),
                    Nombre = reader.GetString(7),
                    Apellido = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                },
                Categoria = reader.IsDBNull(9) ? null : new CategoriaPrenda
                {
                    CategoriaId = reader.GetInt32(9),
                    NombreCategoria = reader.GetString(10)
                }
            });
        }

        return prendas;
    }

    public async Task<int> ContarPrendas(string? busqueda = null, string? estado = null, string? categoria = null)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT COUNT(*)
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(busqueda))
        {
            sql += " AND (p.titulo_publicacion LIKE @Busqueda OR p.descripcion_publicacion LIKE @Busqueda)";
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            sql += " AND p.estado_publicacion_id = @Estado";
        }

        if (!string.IsNullOrEmpty(categoria) && categoria != "todos")
        {
            sql += " AND c.nombre_categoria = @Categoria";
        }

        using var command = new SqlCommand(sql, connection);

        if (!string.IsNullOrEmpty(busqueda))
        {
            command.Parameters.Add(new SqlParameter("@Busqueda", SqlDbType.NVarChar, 200) { Value = $"%{busqueda}%" });
        }

        if (!string.IsNullOrEmpty(estado) && estado != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.Int) { Value = int.Parse(estado) });
        }

        if (!string.IsNullOrEmpty(categoria) && categoria != "todos")
        {
            command.Parameters.Add(new SqlParameter("@Categoria", SqlDbType.NVarChar, 50) { Value = categoria });
        }

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // RF-17: Estadisticas para informes
    public async Task<(int UsuariosNuevos, int PrendasNuevas, int TruequesCompletados, int TruequesPendientes, int ReportesResueltos, decimal ReputacionPromedio)> ObtenerEstadisticasPeriodo(DateTime? fechaInicio, DateTime? fechaFin)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        var fechaInicioReal = fechaInicio ?? DateTime.Now.AddMonths(-1);
        var fechaFinReal = fechaFin ?? DateTime.Now;

        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM Usuario WHERE fecha_registro >= @FechaInicio AND fecha_registro <= @FechaFin) AS UsuariosNuevos,
                (SELECT COUNT(*) FROM Prenda WHERE fecha_publicacion >= @FechaInicio AND fecha_publicacion <= @FechaFin) AS PrendasNuevas,
                (SELECT COUNT(*) FROM PropuestaTrueque WHERE estado_propuesta_id = 6 AND fecha_propuesta >= @FechaInicio AND fecha_propuesta <= @FechaFin) AS TruequesCompletados,
                (SELECT COUNT(*) FROM PropuestaTrueque WHERE estado_propuesta_id IN (1, 2, 3)) AS TruequesPendientes,
                (SELECT COUNT(*) FROM Reporte WHERE estado = 'resuelto' AND fecha_resolucion >= @FechaInicio AND fecha_resolucion <= @FechaFin) AS ReportesResueltos,
                (SELECT ISNULL(AVG(reputacion_promedio), 0) FROM Usuario WHERE rol = 'usuario') AS ReputacionPromedio";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime) { Value = fechaInicioReal });
        command.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime) { Value = fechaFinReal });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetDecimal(5)
            );
        }

        return (0, 0, 0, 0, 0, 0);
    }

    private static TipoReporte ParseTipoReporte(string tipo)
    {
        return tipo.ToLower() switch
        {
            "usuario" => TipoReporte.Usuario,
            "prenda" => TipoReporte.Prenda,
            _ => TipoReporte.Usuario
        };
    }

    private static EstadoReporte ParseEstadoReporte(string estado)
    {
        return estado.ToLower() switch
        {
            "pendiente" => EstadoReporte.Pendiente,
            "revisado" => EstadoReporte.Revisado,
            "resuelto" => EstadoReporte.Resuelto,
            "desestimado" => EstadoReporte.Desestimado,
            _ => EstadoReporte.Pendiente
        };
    }

    // RF-17: Obtener propuestas pendientes
    public async Task<int> ObtenerPropuestasPendientes()
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM PropuestaTrueque
            WHERE estado_propuesta_id IN (1, 2, 4)"; // Pendiente, Aceptada, Contraoferta

        using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // RF-17: Obtener detalle completo de usuario
    public async Task<Usuario?> ObtenerDetalleUsuario(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.correo_electronico, u.nombre, u.apellido,
                   u.reputacion_promedio, u.cuenta_verificada, u.fecha_registro,
                   u.estado_usuario, u.rol, u.telefono, u.fecha_ultimo_login,
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
                UsuarioId = reader.GetInt32(0),
                CorreoElectronico = reader.GetString(1),
                Nombre = reader.GetString(2),
                Apellido = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ReputacionPromedio = reader.GetDecimal(4),
                CuentaVerificada = reader.GetBoolean(5),
                FechaRegistro = reader.GetDateTime(6),
                EstadoUsuario = reader.GetString(7),
                Rol = reader.GetString(8),
                Telefono = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                FechaUltimoLogin = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                Comuna = new Comuna
                {
                    NombreComuna = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Region = new Region
                    {
                        NombreRegion = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
                    }
                }
            };
        }

        return null;
    }

    // RF-17: Cambiar rol de usuario
    public async Task<bool> CambiarRolUsuario(int usuarioId, int adminId, string nuevoRol)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlUpdate = "UPDATE Usuario SET rol = @NuevoRol WHERE usuario_id = @UsuarioId";
            using (var command = new SqlCommand(sqlUpdate, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@NuevoRol", SqlDbType.NVarChar, 20) { Value = nuevoRol });
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                await command.ExecuteNonQueryAsync();
            }

            const string sqlLog = @"
                INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                VALUES (@AdminId, 'cambio_rol', 'usuario', @UsuarioId, @Descripcion, GETDATE())";

            using (var command = new SqlCommand(sqlLog, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = $"Rol cambiado a {nuevoRol}" });
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // RF-17: Ejecutar accion al aprobar reporte
    public async Task<bool> EjecutarAccionReporte(int reporteId, int adminId, string tipoAccion)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Obtener el reporte
        var reporte = await ObtenerReportePorId(reporteId);
        if (reporte == null) return false;

        using var transaction = connection.BeginTransaction();

        try
        {
            // Ejecutar accion segun el tipo de reporte
            if (reporte.Tipo == TipoReporte.Usuario && reporte.UsuarioReportadoId.HasValue)
            {
                // Suspender usuario reportado
                const string sqlSuspender = "UPDATE Usuario SET estado_usuario = 'suspendido' WHERE usuario_id = @UsuarioId";
                using (var command = new SqlCommand(sqlSuspender, connection, transaction))
                {
                    command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = reporte.UsuarioReportadoId.Value });
                    await command.ExecuteNonQueryAsync();
                }

                // Registrar accion
                const string sqlLog = @"
                    INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                    VALUES (@AdminId, 'suspension_automatica', 'usuario', @UsuarioId, @Motivo, GETDATE())";

                using (var command = new SqlCommand(sqlLog, connection, transaction))
                {
                    command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                    command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = reporte.UsuarioReportadoId.Value });
                    command.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 500)
                        { Value = $"Suspension automatica por reporte aprobado: {reporte.Motivo}" });
                    await command.ExecuteNonQueryAsync();
                }
            }
            else if (reporte.Tipo == TipoReporte.Prenda && reporte.PrendaReportadaId.HasValue)
            {
                // Desactivar prenda reportada
                const string sqlDesactivar = "UPDATE Prenda SET estado_publicacion_id = 5 WHERE prenda_id = @PrendaId";
                using (var command = new SqlCommand(sqlDesactivar, connection, transaction))
                {
                    command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = reporte.PrendaReportadaId.Value });
                    await command.ExecuteNonQueryAsync();
                }

                // Registrar accion
                const string sqlLog = @"
                    INSERT INTO AccionModeracion (admin_id, tipo_accion, entidad_afectada, entidad_id, descripcion_accion, fecha_accion)
                    VALUES (@AdminId, 'desactivacion_automatica', 'prenda', @PrendaId, @Motivo, GETDATE())";

                using (var command = new SqlCommand(sqlLog, connection, transaction))
                {
                    command.Parameters.Add(new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId });
                    command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = reporte.PrendaReportadaId.Value });
                    command.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 500)
                        { Value = $"Desactivacion automatica por reporte aprobado: {reporte.Motivo}" });
                    await command.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
