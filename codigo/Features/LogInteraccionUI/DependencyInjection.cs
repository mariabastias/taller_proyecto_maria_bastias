using TruequeTextil.Features.LogInteraccionUI.Interfaces;

namespace TruequeTextil.Features.LogInteraccionUI;

public static class DependencyInjection
{
    public static IServiceCollection AddLogInteraccionUIFeature(this IServiceCollection services)
    {
        services.AddScoped<ILogInteraccionUIRepository, LogInteraccionUIRepository>();
        services.AddScoped<ILogInteraccionUIService, LogInteraccionUIService>();

        return services;
    }
}
