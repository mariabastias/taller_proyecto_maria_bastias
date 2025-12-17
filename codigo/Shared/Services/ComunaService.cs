using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class ComunaService
{
    private readonly UsuarioRepository _usuarioRepository;

    public ComunaService(UsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<Comuna>> GetAllComunasAsync()
    {
        // Since we don't have a method to get all comunas at once, we'll get them by calling for all regions
        // In a real implementation, this would be optimized
        var allComunas = new List<Comuna>();
        var regiones = await _usuarioRepository.ObtenerRegiones();

        foreach (var region in regiones)
        {
            var comunas = await _usuarioRepository.ObtenerComunasPorRegion(region.RegionId);
            allComunas.AddRange(comunas);
        }

        return allComunas;
    }

    public async Task<Comuna?> GetComunaByIdAsync(int id)
    {
        var allComunas = await GetAllComunasAsync();
        return allComunas.FirstOrDefault(c => c.ComunaId == id);
    }

    public async Task<List<Comuna>> GetComunasByRegionIdAsync(int regionId)
    {
        return await _usuarioRepository.ObtenerComunasPorRegion(regionId);
    }

    public async Task<Comuna?> GetComunaByCodigoAsync(string codigo)
    {
        var allComunas = await GetAllComunasAsync();
        return allComunas.FirstOrDefault(c => c.CodigoComuna == codigo);
    }
}
