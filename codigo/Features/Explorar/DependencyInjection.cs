using TruequeTextil.Features.Explorar.Interfaces;

namespace TruequeTextil.Features.Explorar;

public static class DependencyInjection
{
    public static IServiceCollection AddExplorarFeature(this IServiceCollection services)
    {
        services.AddScoped<IExplorarRepository, ExplorarRepository>();
        services.AddScoped<IExplorarService, ExplorarService>();

        return services;
    }
}
