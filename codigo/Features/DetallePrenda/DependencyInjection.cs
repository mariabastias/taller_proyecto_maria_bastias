using TruequeTextil.Features.DetallePrenda.Interfaces;

namespace TruequeTextil.Features.DetallePrenda;

public static class DependencyInjection
{
    public static IServiceCollection AddDetallePrendaFeature(this IServiceCollection services)
    {
        services.AddScoped<IDetallePrendaRepository, DetallePrendaRepository>();
        services.AddScoped<IDetallePrendaService, DetallePrendaService>();

        return services;
    }
}
