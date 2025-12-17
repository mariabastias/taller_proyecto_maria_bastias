using TruequeTextil.Features.PublicarPrenda.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;

namespace TruequeTextil.Features.PublicarPrenda;

public class PublicarPrendaService : IPublicarPrendaService
{
    private readonly IPublicarPrendaRepository _repository;
    private readonly ILogger<PublicarPrendaService> _logger;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    private const int MIN_IMAGENES = 1;
    private const int MAX_IMAGENES = 5;

    public PublicarPrendaService(
        IPublicarPrendaRepository repository,
        ILogger<PublicarPrendaService> logger,
        CustomAuthenticationStateProvider authenticationStateProvider)
    {
        _repository = repository;
        _logger = logger;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<Usuario?> GetCurrentUser()
    {
        return await _authenticationStateProvider.GetCurrentUserAsync();
    }

    public async Task<(bool Success, int PrendaId, string? Error)> PublicarPrenda(
        int usuarioId,
        string titulo,
        string descripcion,
        int categoriaId,
        string talla,
        int estadoPrendaId,
        List<string> imagenes)
    {
        // Validaciones de negocio (RF-04)
        if (string.IsNullOrWhiteSpace(titulo))
            return (false, 0, "El titulo de la publicacion es obligatorio");

        if (titulo.Length > 100)
            return (false, 0, "El titulo no puede exceder 100 caracteres");

        if (string.IsNullOrWhiteSpace(descripcion))
            return (false, 0, "La descripcion es obligatoria");

        if (descripcion.Length > 500)
            return (false, 0, "La descripcion no puede exceder 500 caracteres");

        if (categoriaId <= 0)
            return (false, 0, "Debes seleccionar una categoria");

        if (string.IsNullOrWhiteSpace(talla))
            return (false, 0, "La talla es obligatoria");

        if (estadoPrendaId <= 0)
            return (false, 0, "Debes seleccionar el estado de la prenda");

        // Validacion de imagenes (RF-06: minimo 1, maximo 5)
        if (imagenes == null || imagenes.Count < MIN_IMAGENES)
            return (false, 0, $"Debes subir al menos {MIN_IMAGENES} imagen{(MIN_IMAGENES > 1 ? "es" : "")}");

        if (imagenes.Count > MAX_IMAGENES)
            return (false, 0, $"No puedes subir más de {MAX_IMAGENES} imágenes");

        try
        {
            var prenda = new Prenda
            {
                UsuarioId = usuarioId,
                TituloPublicacion = titulo.Trim(),
                DescripcionPublicacion = descripcion.Trim(),
                CategoriaId = categoriaId,
                Talla = talla,
                EstadoPrendaId = estadoPrendaId,
                EstadoPublicacionId = 1, // 'disponible'
                FechaPublicacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            var prendaId = await _repository.CrearPrenda(prenda);

            if (prendaId <= 0)
            {
                _logger.LogWarning("No se pudo crear la prenda para usuario {UsuarioId}", usuarioId);
                return (false, 0, "Error al crear la publicacion");
            }

            // Guardar imagenes
            for (int i = 0; i < imagenes.Count; i++)
            {
                var imageUrl = imagenes[i];
                var esPrincipal = i == 0;
                await _repository.AgregarImagenPrenda(prendaId, imageUrl, i + 1, esPrincipal);
            }

            _logger.LogInformation("Prenda {PrendaId} publicada por usuario {UsuarioId}", prendaId, usuarioId);
            return (true, prendaId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al publicar prenda para usuario {UsuarioId}", usuarioId);
            return (false, 0, "Error al procesar la publicacion");
        }
    }

    public async Task<List<CategoriaPrenda>> ObtenerCategorias()
    {
        try
        {
            return await _repository.ObtenerCategorias();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorias");
            return GetCategoriasFallback();
        }
    }

    public async Task<List<EstadoPrenda>> ObtenerEstadosPrenda()
    {
        try
        {
            return await _repository.ObtenerEstadosPrenda();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estados de prenda");
            return GetEstadosFallback();
        }
    }

    public Task<List<string>> ObtenerTallas()
    {
        var tallas = new List<string>
        {
            "XS", "S", "M", "L", "XL", "XXL",
            "36", "37", "38", "39", "40", "41", "42", "43", "44"
        };
        return Task.FromResult(tallas);
    }

    private List<CategoriaPrenda> GetCategoriasFallback()
    {
        return new List<CategoriaPrenda>
        {
            new() { CategoriaId = 1, NombreCategoria = "Camisa" },
            new() { CategoriaId = 2, NombreCategoria = "Camiseta" },
            new() { CategoriaId = 3, NombreCategoria = "Pantalon" },
            new() { CategoriaId = 4, NombreCategoria = "Vestido" },
            new() { CategoriaId = 5, NombreCategoria = "Falda" },
            new() { CategoriaId = 6, NombreCategoria = "Chaqueta" },
            new() { CategoriaId = 7, NombreCategoria = "Sueter" },
            new() { CategoriaId = 8, NombreCategoria = "Abrigo" },
            new() { CategoriaId = 9, NombreCategoria = "Calzado" },
            new() { CategoriaId = 10, NombreCategoria = "Accesorio" }
        };
    }

    private List<EstadoPrenda> GetEstadosFallback()
    {
        return new List<EstadoPrenda>
        {
            new() { EstadoId = 1, NombreEstado = "Nuevo" },
            new() { EstadoId = 2, NombreEstado = "Como nuevo" },
            new() { EstadoId = 3, NombreEstado = "Muy bueno" },
            new() { EstadoId = 4, NombreEstado = "Bueno" },
            new() { EstadoId = 5, NombreEstado = "Aceptable" }
        };
    }
}
