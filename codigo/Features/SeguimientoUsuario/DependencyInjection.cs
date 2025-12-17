using TruequeTextil.Features.SeguimientoUsuario.Interfaces;

namespace TruequeTextil.Features.SeguimientoUsuario;

public static class DependencyInjection
{
    public static IServiceCollection AddSeguimientoUsuarioFeature(this IServiceCollection services)
    {
        services.AddScoped<ISeguimientoUsuarioRepository, SeguimientoUsuarioRepository>();
        services.AddScoped<ISeguimientoUsuarioService, SeguimientoUsuarioService>();

        return services;
    }
}
