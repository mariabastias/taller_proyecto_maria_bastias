using TruequeTextil.Features.EditarPrenda.Interfaces;

namespace TruequeTextil.Features.EditarPrenda;

public static class DependencyInjection
{
    public static IServiceCollection AddEditarPrendaFeature(this IServiceCollection services)
    {
        services.AddScoped<IEditarPrendaRepository, EditarPrendaRepository>();
        services.AddScoped<IEditarPrendaService, EditarPrendaService>();

        return services;
    }
}