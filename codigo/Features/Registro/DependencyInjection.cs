using TruequeTextil.Features.Registro.Interfaces;

namespace TruequeTextil.Features.Registro;

public static class DependencyInjection
{
    public static IServiceCollection AddRegistroFeature(this IServiceCollection services)
    {
        services.AddScoped<IRegistroRepository, RegistroRepository>();
        services.AddScoped<IRegistroService, RegistroService>();

        return services;
    }
}
