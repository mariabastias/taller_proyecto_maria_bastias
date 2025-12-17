using TruequeTextil.Features.EditarPerfil.Interfaces;

namespace TruequeTextil.Features.EditarPerfil;

public static class DependencyInjection
{
    public static IServiceCollection AddEditarPerfilFeature(this IServiceCollection services)
    {
        services.AddScoped<IEditarPerfilRepository, EditarPerfilRepository>();
        services.AddScoped<IEditarPerfilService, EditarPerfilService>();

        return services;
    }
}
