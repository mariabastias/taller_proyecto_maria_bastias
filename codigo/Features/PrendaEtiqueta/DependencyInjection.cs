using TruequeTextil.Features.PrendaEtiqueta.Interfaces;

namespace TruequeTextil.Features.PrendaEtiqueta;

public static class DependencyInjection
{
    public static IServiceCollection AddPrendaEtiquetaFeature(this IServiceCollection services)
    {
        services.AddScoped<IPrendaEtiquetaRepository, PrendaEtiquetaRepository>();
        services.AddScoped<IPrendaEtiquetaService, PrendaEtiquetaService>();

        return services;
    }
}
