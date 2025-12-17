using TruequeTextil.Features.Favoritos.Interfaces;

namespace TruequeTextil.Features.Favoritos;

public static class DependencyInjection
{
    public static IServiceCollection AddFavoritosFeature(this IServiceCollection services)
    {
        services.AddScoped<IFavoritosRepository, FavoritosRepository>();
        services.AddScoped<IFavoritosService, FavoritosService>();

        return services;
    }
}
