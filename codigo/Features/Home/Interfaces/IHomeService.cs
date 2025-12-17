using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Home.Interfaces;

public interface IHomeService
{
    Task<List<Prenda>> ObtenerPrendasRecientes();
    Task<List<Prenda>> ObtenerPrendasDestacadas();
    Task<HomeStats> ObtenerEstadisticas();
}

public class HomeStats
{
    public int UsuariosActivos { get; set; }
    public int PrendasDisponibles { get; set; }
    public int TruequesCompletados { get; set; }
}
