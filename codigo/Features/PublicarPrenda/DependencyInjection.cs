using TruequeTextil.Features.PublicarPrenda.Interfaces;

namespace TruequeTextil.Features.PublicarPrenda;

public static class DependencyInjection
{
    public static IServiceCollection AddPublicarPrendaFeature(this IServiceCollection services)
    {
        services.AddScoped<IPublicarPrendaRepository, PublicarPrendaRepository>();
        services.AddScoped<IPublicarPrendaService, PublicarPrendaService>();

        return services;
    }
}
