using TruequeTextil.Features.InicioSesion.Interfaces;

namespace TruequeTextil.Features.InicioSesion;

public static class DependencyInjection
{
    public static IServiceCollection AddInicioSesionFeature(this IServiceCollection services)
    {
        services.AddScoped<IInicioSesionRepository, InicioSesionRepository>();
        services.AddScoped<IInicioSesionService, InicioSesionService>();

        return services;
    }
}
