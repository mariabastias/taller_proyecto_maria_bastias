using TruequeTextil.Features.PerfilPublico.Interfaces;

namespace TruequeTextil.Features.PerfilPublico;

public static class DependencyInjection
{
    public static IServiceCollection AddPerfilPublicoFeature(this IServiceCollection services)
    {
        services.AddScoped<IPerfilPublicoRepository, PerfilPublicoRepository>();
        services.AddScoped<IPerfilPublicoService, PerfilPublicoService>();

        return services;
    }
}
