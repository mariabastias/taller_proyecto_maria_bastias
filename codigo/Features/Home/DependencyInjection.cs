using TruequeTextil.Features.Home.Interfaces;

namespace TruequeTextil.Features.Home;

public static class DependencyInjection
{
    public static IServiceCollection AddHomeFeature(this IServiceCollection services)
    {
        services.AddScoped<IHomeRepository, HomeRepository>();
        services.AddScoped<IHomeService, HomeService>();

        return services;
    }
}
