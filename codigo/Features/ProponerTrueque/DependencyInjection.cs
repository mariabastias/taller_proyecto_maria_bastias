using TruequeTextil.Features.ProponerTrueque.Interfaces;

namespace TruequeTextil.Features.ProponerTrueque;

public static class DependencyInjection
{
    public static IServiceCollection AddProponerTruequeFeature(this IServiceCollection services)
    {
        services.AddScoped<IProponerTruequeRepository, ProponerTruequeRepository>();
        services.AddScoped<IProponerTruequeService, ProponerTruequeService>();

        return services;
    }
}
