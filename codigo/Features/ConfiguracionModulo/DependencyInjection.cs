using TruequeTextil.Features.ConfiguracionModulo.Interfaces;

namespace TruequeTextil.Features.ConfiguracionModulo;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguracionModuloFeature(this IServiceCollection services)
    {
        services.AddScoped<IConfiguracionModuloRepository, ConfiguracionModuloRepository>();
        services.AddScoped<IConfiguracionModuloService, ConfiguracionModuloService>();

        return services;
    }
}
