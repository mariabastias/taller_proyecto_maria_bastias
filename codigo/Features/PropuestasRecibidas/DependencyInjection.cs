using TruequeTextil.Features.PropuestasRecibidas.Interfaces;

namespace TruequeTextil.Features.PropuestasRecibidas;

public static class DependencyInjection
{
    public static IServiceCollection AddPropuestasRecibidasFeature(this IServiceCollection services)
    {
        // Servicios principales
        services.AddScoped<IPropuestasRecibidasRepository, PropuestasRecibidasRepository>();
        services.AddScoped<IPropuestasRecibidasService, PropuestasRecibidasService>();

        // RF-11: Servicios de expiración de propuestas
        services.AddScoped<IExpiracionPropuestasRepository, ExpiracionPropuestasRepository>();
        services.AddScoped<IExpiracionPropuestasService, ExpiracionPropuestasService>();

        // RF-11: Background service para procesar propuestas expiradas diariamente
        services.AddHostedService<ExpiracionPropuestasBackgroundService>();

        return services;
    }
}
