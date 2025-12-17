using TruequeTextil.Features.RecuperarPassword.Interfaces;

namespace TruequeTextil.Features.RecuperarPassword;

public static class DependencyInjection
{
    public static IServiceCollection AddRecuperarPasswordFeature(this IServiceCollection services)
    {
        services.AddScoped<IRecuperarPasswordRepository, RecuperarPasswordRepository>();
        services.AddScoped<IRecuperarPasswordService, RecuperarPasswordService>();

        return services;
    }
}
