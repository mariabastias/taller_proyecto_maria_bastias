using TruequeTextil.Features.ConfiguracionInterfaz.Interfaces;

namespace TruequeTextil.Features.ConfiguracionInterfaz;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguracionInterfazFeature(this IServiceCollection services)
    {
        services.AddScoped<IConfiguracionInterfazRepository, ConfiguracionInterfazRepository>();
        services.AddScoped<IConfiguracionInterfazService, ConfiguracionInterfazService>();

        return services;
    }
}
