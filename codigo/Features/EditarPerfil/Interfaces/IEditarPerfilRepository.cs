using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.EditarPerfil.Interfaces;

public interface IEditarPerfilRepository
{
    Task<Usuario?> ObtenerUsuarioPorId(int usuarioId);
    Task<List<Region>> ObtenerRegiones();
    Task<List<Comuna>> ObtenerComunasPorRegion(int regionId);
    Task ActualizarDatosBasicos(int usuarioId, string nombre, string apellido, int comunaId);
    Task ActualizarFotoYBiografia(int usuarioId, string urlFotoPerfil, string biografia);
    Task RegistrarCambioHistorial(int usuarioId, string campoModificado, string? valorAnterior, string? valorNuevo);
}
