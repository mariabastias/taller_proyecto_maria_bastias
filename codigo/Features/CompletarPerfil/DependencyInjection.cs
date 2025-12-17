using TruequeTextil.Features.CompletarPerfil.Interfaces;

namespace TruequeTextil.Features.CompletarPerfil;

public static class DependencyInjection
{
    public static IServiceCollection AddCompletarPerfilFeature(this IServiceCollection services)
    {
        services.AddScoped<ICompletarPerfilRepository, CompletarPerfilRepository>();
        services.AddScoped<ICompletarPerfilService, CompletarPerfilService>();

        return services;
    }
}
