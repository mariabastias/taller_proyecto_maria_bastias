using TruequeTextil.Features.Admin.Interfaces;

namespace TruequeTextil.Features.Admin;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminFeature(this IServiceCollection services)
    {
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}
