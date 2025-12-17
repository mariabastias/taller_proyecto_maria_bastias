using TruequeTextil.Features.DetallePropuesta.Interfaces;

namespace TruequeTextil.Features.DetallePropuesta;

public static class DependencyInjection
{
    public static IServiceCollection AddDetallePropuestaFeature(this IServiceCollection services)
    {
        services.AddScoped<IDetallePropuestaRepository, DetallePropuestaRepository>();
        services.AddScoped<IDetallePropuestaService, DetallePropuestaService>();

        return services;
    }
}
