using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.Home.Interfaces;

public interface IHomeRepository
{
    Task<List<Prenda>> ObtenerPrendasRecientes(int limite = 8);
    Task<List<Prenda>> ObtenerPrendasDestacadas(int limite = 4);
    Task<int> ContarUsuariosActivos();
    Task<int> ContarPrendasDisponibles();
    Task<int> ContarTruequesCompletados();
}
