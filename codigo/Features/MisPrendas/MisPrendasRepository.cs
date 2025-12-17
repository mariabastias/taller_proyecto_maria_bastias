using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.MisPrendas.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.MisPrendas;

public class MisPrendasRepository : IMisPrendasRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public MisPrendasRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<List<Prenda>> ObtenerPrendasUsuario(int usuarioId)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria,
                   ep.nombre_estado AS nombre_estado_prenda,
                   epu.nombre_estado AS nombre_estado_publicacion,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal,
                   (SELECT COUNT(*) FROM DetallePropuesta dp
                    INNER JOIN PropuestaTrueque pt ON dp.propuesta_id = pt.propuesta_id
                    WHERE dp.prenda_id = p.prenda_id AND pt.estado_propuesta_id IN (1, 2, 4)) AS propuestas_activas
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            LEFT JOIN EstadoPublicacion epu ON p.estado_publicacion_id = epu.estado_publicacion_id
            WHERE p.usuario_id = @UsuarioId AND p.estado_publicacion_id != 3
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(MapPrendaFromReader(reader));
        }

        return prendas;
    }

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.usuario_id, p.titulo_publicacion, p.descripcion_publicacion,
                   p.categoria_id, p.talla, p.estado_prenda_id, p.estado_publicacion_id,
                   p.fecha_publicacion, p.fecha_actualizacion,
                   c.nombre_categoria,
                   ep.nombre_estado AS nombre_estado_prenda,
                   epu.nombre_estado AS nombre_estado_publicacion,
                   (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal,
                   (SELECT COUNT(*) FROM DetallePropuesta dp
                    INNER JOIN PropuestaTrueque pt ON dp.propuesta_id = pt.propuesta_id
                    WHERE dp.prenda_id = p.prenda_id AND pt.estado_propuesta_id IN (1, 2, 4)) AS propuestas_activas
            FROM Prenda p
            LEFT JOIN CategoriaPrenda c ON p.categoria_id = c.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            LEFT JOIN EstadoPublicacion epu ON p.estado_publicacion_id = epu.estado_publicacion_id
            WHERE p.prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPrendaFromReader(reader);
        }

        return null;
    }

    public async Task<bool> ActualizarPrenda(Prenda prenda)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET titulo_publicacion = @Titulo,
                descripcion_publicacion = @Descripcion,
                categoria_id = @CategoriaId,
                talla = @Talla,
                estado_prenda_id = @EstadoPrendaId,
                fecha_actualizacion = @FechaActualizacion
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prenda.PrendaId });
        command.Parameters.Add(new SqlParameter("@Titulo", SqlDbType.NVarChar, 100) { Value = prenda.TituloPublicacion });
        command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = prenda.DescripcionPublicacion });
        command.Parameters.Add(new SqlParameter("@CategoriaId", SqlDbType.Int) { Value = prenda.CategoriaId });
        command.Parameters.Add(new SqlParameter("@Talla", SqlDbType.NVarChar, 20) { Value = prenda.Talla });
        command.Parameters.Add(new SqlParameter("@EstadoPrendaId", SqlDbType.Int) { Value = prenda.EstadoPrendaId });
        command.Parameters.Add(new SqlParameter("@FechaActualizacion", SqlDbType.DateTime2) { Value = DateTime.Now });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-06: Eliminacion logica - cambiar estado_publicacion_id a 3 (eliminada/inactiva)
    public async Task<bool> EliminarPrenda(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET estado_publicacion_id = 3, fecha_actualizacion = @FechaActualizacion
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
        command.Parameters.Add(new SqlParameter("@FechaActualizacion", SqlDbType.DateTime2) { Value = DateTime.Now });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CambiarDisponibilidad(int prendaId, int estadoPublicacionId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            UPDATE Prenda
            SET estado_publicacion_id = @EstadoPublicacionId, fecha_actualizacion = @FechaActualizacion
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });
        command.Parameters.Add(new SqlParameter("@EstadoPublicacionId", SqlDbType.Int) { Value = estadoPublicacionId });
        command.Parameters.Add(new SqlParameter("@FechaActualizacion", SqlDbType.DateTime2) { Value = DateTime.Now });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> ContarPropuestasActivas(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        // Estados activos: 1=Pendiente, 2=Aceptada, 4=Contraoferta
        const string sql = @"
            SELECT COUNT(*) FROM DetallePropuesta dp
            INNER JOIN PropuestaTrueque pt ON dp.propuesta_id = pt.propuesta_id
            WHERE dp.prenda_id = @PrendaId AND pt.estado_propuesta_id IN (1, 2, 4)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // RF-05: Registrar historial de cambios
    public async Task<bool> RegistrarHistorial(HistorialPrenda historial)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO HistorialPrenda (prenda_id, campo_modificado, valor_anterior, valor_nuevo,
                                         usuario_modificador_id, fecha_modificacion)
            VALUES (@PrendaId, @CampoModificado, @ValorAnterior, @ValorNuevo,
                    @UsuarioModificadorId, @FechaModificacion)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = historial.PrendaId });
        command.Parameters.Add(new SqlParameter("@CampoModificado", SqlDbType.NVarChar, 100) { Value = historial.CampoModificado });
        command.Parameters.Add(new SqlParameter("@ValorAnterior", SqlDbType.NVarChar, 500) { Value = (object?)historial.ValorAnterior ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@ValorNuevo", SqlDbType.NVarChar, 500) { Value = (object?)historial.ValorNuevo ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@UsuarioModificadorId", SqlDbType.Int) { Value = historial.UsuarioModificadorId });
        command.Parameters.Add(new SqlParameter("@FechaModificacion", SqlDbType.DateTime2) { Value = historial.FechaModificacion });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    // RF-06: Obtener usuarios que tienen esta prenda en sus favoritos
    public async Task<List<int>> ObtenerUsuariosConPrendaEnFavoritos(int prendaId)
    {
        var usuarios = new List<int>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT usuario_id FROM Favorito
            WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            usuarios.Add(reader.GetInt32(0));
        }

        return usuarios;
    }

    private Prenda MapPrendaFromReader(SqlDataReader reader)
    {
        var prenda = new Prenda
        {
            PrendaId = reader.GetInt32(reader.GetOrdinal("prenda_id")),
            UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
            TituloPublicacion = reader.GetString(reader.GetOrdinal("titulo_publicacion")),
            DescripcionPublicacion = reader.IsDBNull(reader.GetOrdinal("descripcion_publicacion"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
            CategoriaId = reader.GetInt32(reader.GetOrdinal("categoria_id")),
            Talla = reader.GetString(reader.GetOrdinal("talla")),
            EstadoPrendaId = reader.GetInt32(reader.GetOrdinal("estado_prenda_id")),
            EstadoPublicacionId = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")),
            FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
            FechaActualizacion = reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion")),
            PropuestasActivas = reader.GetInt32(reader.GetOrdinal("propuestas_activas"))
        };

        // Set navigation/computed values
        prenda.Tipo = reader.IsDBNull(reader.GetOrdinal("nombre_categoria"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_categoria"));

        prenda.Estado = reader.IsDBNull(reader.GetOrdinal("nombre_estado_prenda"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("nombre_estado_prenda"));

        prenda.Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_principal"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("imagen_principal"));

        return prenda;
    }
}
