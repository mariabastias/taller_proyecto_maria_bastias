using TruequeTextil.Shared.Models;

using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.LogInteraccionUI.Interfaces;

public interface ILogInteraccionUIService
{
    Task<(bool Success, string? Error)> RegistrarInteraccion(LogInteraccionUIModel interaccion);
    Task<List<LogInteraccionUIModel>> ObtenerInteraccionesPorUsuario(int? usuarioId, int limite = 100);
    Task<List<LogInteraccionUIModel>> ObtenerInteraccionesPorElemento(string elementoUi, int limite = 100);
    Task<List<LogInteraccionUIModel>> ObtenerInteraccionesRecientes(int limite = 50);
    Task<int> ContarInteraccionesPorUsuario(int? usuarioId, DateTime? desde = null, DateTime? hasta = null);
}
