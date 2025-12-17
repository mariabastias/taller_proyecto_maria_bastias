using TruequeTextil.Features.MisPrendas.Interfaces;

namespace TruequeTextil.Features.MisPrendas;

public static class DependencyInjection
{
    public static IServiceCollection AddMisPrendasFeature(this IServiceCollection services)
    {
        services.AddScoped<IMisPrendasRepository, MisPrendasRepository>();
        services.AddScoped<IMisPrendasService, MisPrendasService>();

        return services;
    }
}
