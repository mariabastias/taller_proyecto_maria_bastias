using TruequeTextil.Features.Onboarding.Interfaces;

namespace TruequeTextil.Features.Onboarding;

public static class DependencyInjection
{
    public static IServiceCollection AddOnboardingFeature(this IServiceCollection services)
    {
        services.AddScoped<IOnboardingRepository, OnboardingRepository>();
        services.AddScoped<IOnboardingService, OnboardingService>();

        return services;
    }
}
