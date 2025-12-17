using TruequeTextil.Features.Notificaciones.Interfaces;

namespace TruequeTextil.Features.Notificaciones;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificacionesFeature(this IServiceCollection services)
    {
        services.AddScoped<INotificacionesRepository, NotificacionesRepository>();
        services.AddScoped<INotificacionesService, NotificacionesService>();

        return services;
    }
}
