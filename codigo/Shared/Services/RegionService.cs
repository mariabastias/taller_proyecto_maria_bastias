using TruequeTextil.Shared.Models;

namespace TruequeTextil.Shared.Services;

public class RegionService
{
    private readonly UsuarioRepository _usuarioRepository;

    public RegionService(UsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<Region>> GetAllRegionesAsync()
    {
        return await _usuarioRepository.ObtenerRegiones();
    }

    public async Task<Region?> GetRegionByIdAsync(int id)
    {
        var regiones = await _usuarioRepository.ObtenerRegiones();
        return regiones.FirstOrDefault(r => r.RegionId == id);
    }

    public async Task<Region?> GetRegionByCodigoAsync(string codigo)
    {
        var regiones = await _usuarioRepository.ObtenerRegiones();
        return regiones.FirstOrDefault(r => r.CodigoRegion == codigo);
    }
}
