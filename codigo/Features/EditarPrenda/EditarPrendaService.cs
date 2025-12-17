using TruequeTextil.Features.EditarPrenda.Interfaces;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;

namespace TruequeTextil.Features.EditarPrenda;

public class EditarPrendaService : IEditarPrendaService
{
    private readonly IEditarPrendaRepository _repository;
    private readonly ILogger<EditarPrendaService> _logger;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    private const int MIN_IMAGENES = 1;
    private const int MAX_IMAGENES = 5;

    public EditarPrendaService(
        IEditarPrendaRepository repository,
        ILogger<EditarPrendaService> logger,
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

    public async Task<Prenda?> ObtenerPrendaPorId(int prendaId)
    {
        try
        {
            return await _repository.ObtenerPrendaPorId(prendaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prenda {PrendaId}", prendaId);
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> ActualizarPrenda(
        int prendaId,
        int usuarioId,
        string titulo,
        string descripcion,
        int categoriaId,
        string talla,
        int estadoPrendaId,
        List<string> imagenes)
    {
        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(titulo))
            return (false, "El titulo de la publicacion es obligatorio");

        if (titulo.Length > 100)
            return (false, "El titulo no puede exceder 100 caracteres");

        if (string.IsNullOrWhiteSpace(descripcion))
            return (false, "La descripcion es obligatoria");

        if (descripcion.Length > 500)
            return (false, "La descripcion no puede exceder 500 caracteres");

        if (categoriaId <= 0)
            return (false, "Debes seleccionar una categoria");

        if (string.IsNullOrWhiteSpace(talla))
            return (false, "La talla es obligatoria");

        if (estadoPrendaId <= 0)
            return (false, "Debes seleccionar el estado de la prenda");

        // Validacion de imagenes
        if (imagenes == null || imagenes.Count < MIN_IMAGENES)
            return (false, $"Debes subir al menos {MIN_IMAGENES} imagen");

        if (imagenes.Count > MAX_IMAGENES)
            return (false, $"No puedes subir mas de {MAX_IMAGENES} imagenes");

        try
        {
            var prenda = new Prenda
            {
                PrendaId = prendaId,
                TituloPublicacion = titulo.Trim(),
                DescripcionPublicacion = descripcion.Trim(),
                CategoriaId = categoriaId,
                Talla = talla,
                EstadoPrendaId = estadoPrendaId,
                FechaActualizacion = DateTime.Now
            };

            var success = await _repository.ActualizarPrenda(prenda);

            if (!success)
            {
                _logger.LogWarning("No se pudo actualizar la prenda {PrendaId} para usuario {UsuarioId}", prendaId, usuarioId);
                return (false, "Error al actualizar la prenda");
            }

            // Eliminar imagenes existentes y agregar nuevas
            await _repository.EliminarImagenesPrenda(prendaId);

            for (int i = 0; i < imagenes.Count; i++)
            {
                var imageUrl = imagenes[i];
                var esPrincipal = i == 0;
                await _repository.AgregarImagenPrenda(prendaId, imageUrl, i + 1, esPrincipal);
            }

            _logger.LogInformation("Prenda {PrendaId} actualizada por usuario {UsuarioId}", prendaId, usuarioId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar prenda {PrendaId} para usuario {UsuarioId}", prendaId, usuarioId);
            return (false, "Error al procesar la actualizacion");
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