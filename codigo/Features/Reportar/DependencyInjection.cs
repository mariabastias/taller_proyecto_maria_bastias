using TruequeTextil.Features.Reportar.Interfaces;

namespace TruequeTextil.Features.Reportar;

public static class DependencyInjection
{
    public static IServiceCollection AddReportarFeature(this IServiceCollection services)
    {
        services.AddScoped<IReportarRepository, ReportarRepository>();
        services.AddScoped<IReportarService, ReportarService>();

        return services;
    }
}
