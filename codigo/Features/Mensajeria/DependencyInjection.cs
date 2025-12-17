using TruequeTextil.Features.Mensajeria.Interfaces;

namespace TruequeTextil.Features.Mensajeria;

public static class DependencyInjection
{
    public static IServiceCollection AddMensajeriaFeature(this IServiceCollection services)
    {
        services.AddScoped<IMensajeriaRepository, MensajeriaRepository>();
        services.AddScoped<IMensajeriaService, MensajeriaService>();
        
        // SignalR Hub para chat en tiempo real
        services.AddSignalR();

        return services;
    }
}
