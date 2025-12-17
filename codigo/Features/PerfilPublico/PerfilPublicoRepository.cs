using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.PerfilPublico.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;
using EvaluacionModel = TruequeTextil.Shared.Models.Evaluacion;

namespace TruequeTextil.Features.PerfilPublico;

public class PerfilPublicoRepository : IPerfilPublicoRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public PerfilPublicoRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<Usuario?> ObtenerPerfilPublico(int usuarioId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.nombre, u.apellido, u.url_foto_perfil, u.biografia,
                u.reputacion_promedio, u.cuenta_verificada, u.fecha_ultimo_login, u.rol,
                c.nombre_comuna, r.nombre_region,
                (SELECT COUNT(*) FROM Prenda WHERE usuario_id = u.usuario_id AND estado_publicacion_id = 1) as prendas_publicadas,
                (SELECT COUNT(*) FROM Evaluacion WHERE usuario_evaluado_id = u.usuario_id) as valoraciones,
                (SELECT COUNT(DISTINCT pt.propuesta_id) 
                    FROM PropuestaTrueque pt 
                    INNER JOIN DetallePropuesta dp ON pt.propuesta_id = dp.propuesta_id 
                    INNER JOIN Prenda p ON dp.prenda_id = p.prenda_id 
                    WHERE (pt.usuario_proponente_id = u.usuario_id OR p.usuario_id = u.usuario_id) 
                    AND pt.estado_propuesta_id = 6) as trueques_completados
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
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("apellido")),
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
                Comuna = reader.IsDBNull(reader.GetOrdinal("nombre_comuna")) ? null! : new Comuna
                {
                    NombreComuna = reader.GetString(reader.GetOrdinal("nombre_comuna")),
                    Region = reader.IsDBNull(reader.GetOrdinal("nombre_region")) ? null! : new Region
                    {
                        NombreRegion = reader.GetString(reader.GetOrdinal("nombre_region"))
                    }
                },
                Estadisticas = new Estadisticas
                {
                    PrendasPublicadas = reader.GetInt32(reader.GetOrdinal("prendas_publicadas")),
                    Valoraciones = reader.GetInt32(reader.GetOrdinal("valoraciones"))
                },
                TruequesConcretados = reader.GetInt32(reader.GetOrdinal("trueques_completados"))
            };
        }

        return null;
    }

    public async Task<List<EvaluacionModel>> ObtenerValoraciones(int usuarioId)
    {
        var valoraciones = new List<EvaluacionModel>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT e.evaluacion_id, e.calificacion_general, e.comentario, e.fecha_evaluacion,
                   u.usuario_id, u.nombre, u.apellido
            FROM Evaluacion e
            INNER JOIN Usuario u ON e.usuario_evaluador_id = u.usuario_id
            WHERE e.usuario_evaluado_id = @UsuarioId
            ORDER BY e.fecha_evaluacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            valoraciones.Add(new EvaluacionModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("evaluacion_id")),
                Puntuacion = reader.GetInt32(reader.GetOrdinal("calificacion_general")),
                Comentario = reader.IsDBNull(reader.GetOrdinal("comentario"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("comentario")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_evaluacion")),
                UsuarioEvaluador = new Usuario
                {
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("apellido"))
                }
            });
        }

        return valoraciones;
    }

    public async Task<List<Prenda>> ObtenerPrendasUsuario(int usuarioId)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                p.categoria_id, cp.nombre_categoria,
                p.talla, p.estado_prenda_id, ep.nombre_estado as estado_prenda,
                p.estado_publicacion_id, p.fecha_publicacion,
                (SELECT TOP 1 imagen_url FROM ImagenPrenda WHERE prenda_id = p.prenda_id AND es_principal = 1) AS imagen_principal
            FROM Prenda p
            LEFT JOIN CategoriaPrenda cp ON p.categoria_id = cp.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.usuario_id = @UsuarioId AND p.estado_publicacion_id = 1";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prendas.Add(new Prenda
            {
                Id = reader.GetInt32(reader.GetOrdinal("prenda_id")),
                Titulo = reader.GetString(reader.GetOrdinal("titulo_publicacion")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion_publicacion"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
                Tipo = reader.GetString(reader.GetOrdinal("nombre_categoria")),
                Talla = reader.GetString(reader.GetOrdinal("talla")),
                Estado = reader.GetString(reader.GetOrdinal("estado_prenda")),
                Imagen = reader.IsDBNull(reader.GetOrdinal("imagen_principal"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("imagen_principal")),
                FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
                Disponible = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")) == 1
            });

        }

        return prendas;
    }
}
