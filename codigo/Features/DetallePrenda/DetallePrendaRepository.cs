using System.Data;
using System.Data.SqlClient;
using TruequeTextil.Features.DetallePrenda.Interfaces;
using TruequeTextil.Shared.Infrastructure;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePrenda;

public class DetallePrendaRepository : IDetallePrendaRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public DetallePrendaRepository(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
           SELECT p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                p.categoria_id, cp.nombre_categoria,
                p.talla, p.estado_prenda_id, ep.nombre_estado as estado_prenda,
                p.estado_publicacion_id, p.fecha_publicacion, p.fecha_actualizacion, p.usuario_id,
                c.nombre_comuna, r.nombre_region
            FROM Prenda p
            LEFT JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN Region r ON c.region_id = r.region_id
            LEFT JOIN CategoriaPrenda cp ON p.categoria_id = cp.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            WHERE p.prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
           return new Prenda
            {
                Id = reader.GetInt32(reader.GetOrdinal("prenda_id")),
                Titulo = reader.GetString(reader.GetOrdinal("titulo_publicacion")),  
                Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion_publicacion"))  
                    ? string.Empty : reader.GetString(reader.GetOrdinal("descripcion_publicacion")),
                Tipo = reader.GetString(reader.GetOrdinal("nombre_categoria")),  
                Talla = reader.GetString(reader.GetOrdinal("talla")),
                Estado = reader.GetString(reader.GetOrdinal("estado_prenda")),  
                FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
                Disponible = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")) == 1,  
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                Ubicacion = reader.IsDBNull(reader.GetOrdinal("nombre_comuna"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("nombre_comuna"))
            };

        }

        return null;
    }

    public async Task<List<string>> ObtenerImagenesPrenda(int prendaId)
    {
        var imagenes = new List<string>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT imagen_url FROM ImagenPrenda
            WHERE prenda_id = @PrendaId
            ORDER BY es_principal DESC, imagen_id ASC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            imagenes.Add(reader.GetString(0));
        }

        return imagenes;
    }

    public async Task<Usuario?> ObtenerPropietario(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT u.usuario_id, u.nombre, u.apellido, u.url_foto_perfil, u.reputacion_promedio, u.cuenta_verificada,
                   c.nombre_comuna, r.nombre_region,
                   (SELECT COUNT(*) FROM Prenda WHERE usuario_id = u.usuario_id AND estado_publicacion_id = 1) as prendas_publicadas
            FROM Prenda p
            INNER JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN Region r ON c.region_id = r.region_id
            WHERE p.prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

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
                ReputacionPromedio = reader.GetDecimal(reader.GetOrdinal("reputacion_promedio")),
                CuentaVerificada = reader.GetBoolean(reader.GetOrdinal("cuenta_verificada")),
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
                    PrendasPublicadas = reader.GetInt32(reader.GetOrdinal("prendas_publicadas"))
                }
            };
        }

        return null;
    }

    public async Task IncrementarVistas(int prendaId)
    {
        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = "UPDATE Prenda SET vistas = vistas + 1 WHERE prenda_id = @PrendaId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Prenda>> ObtenerPrendasSimilares(int prendaId, string tipo, int limite = 4)
    {
        var prendas = new List<Prenda>();

        using var connection = _databaseConfig.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT TOP (@Limite) p.prenda_id, p.titulo_publicacion, p.descripcion_publicacion,
                p.categoria_id, cp.nombre_categoria,
                p.talla, p.estado_prenda_id, ep.nombre_estado as estado_prenda,
                p.estado_publicacion_id, p.fecha_publicacion, p.fecha_actualizacion, p.usuario_id,
                c.nombre_comuna, ip.imagen_url as imagen_principal
            FROM Prenda p
            LEFT JOIN Usuario u ON p.usuario_id = u.usuario_id
            LEFT JOIN Comuna c ON u.comuna_id = c.comuna_id
            LEFT JOIN CategoriaPrenda cp ON p.categoria_id = cp.categoria_id
            LEFT JOIN EstadoPrenda ep ON p.estado_prenda_id = ep.estado_id
            LEFT JOIN ImagenPrenda ip ON p.prenda_id = ip.prenda_id AND ip.es_principal = 1
            WHERE cp.nombre_categoria = @Tipo AND p.prenda_id <> @PrendaId AND p.estado_publicacion_id = 1
            ORDER BY p.fecha_publicacion DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Limite", SqlDbType.Int) { Value = limite });
        command.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.NVarChar, 50) { Value = tipo });
        command.Parameters.Add(new SqlParameter("@PrendaId", SqlDbType.Int) { Value = prendaId });

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
                FechaPublicacion = reader.GetDateTime(reader.GetOrdinal("fecha_publicacion")),
                Disponible = reader.GetInt32(reader.GetOrdinal("estado_publicacion_id")) == 1,  
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                UrlImagenPrincipal = reader.IsDBNull(reader.GetOrdinal("imagen_principal")) 
                    ? null : reader.GetString(reader.GetOrdinal("imagen_principal")),
                Ubicacion = reader.IsDBNull(reader.GetOrdinal("nombre_comuna"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("nombre_comuna"))
            });
        }

        return prendas;
    }

}
