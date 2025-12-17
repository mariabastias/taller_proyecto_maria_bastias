using TruequeTextil.Features.Evaluacion.Interfaces;

namespace TruequeTextil.Features.Evaluacion;

public static class DependencyInjection
{
    public static IServiceCollection AddEvaluacionFeature(this IServiceCollection services)
    {
        services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();
        services.AddScoped<IEvaluacionService, EvaluacionService>();

        return services;
    }
}
