using TruequeTextil.Features.Mensajeria;
using TruequeTextil.Features.EditarPrenda;
using TruequeTextil.Features.SeguimientoUsuario;
using TruequeTextil.Features.ConfiguracionInterfaz;
using TruequeTextil.Features.ConfiguracionModulo;
using TruequeTextil.Features.LogInteraccionUI;
using TruequeTextil.Features.PrendaEtiqueta;
using TruequeTextil.Shared.Repositories;
using TruequeTextil.Shared.Repositories.Interfaces;

namespace TruequeTextil.Shared;

/// <summary>
/// Centralized service registration for dependency injection.
/// Unifies all feature DIs and shared services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Shared Infrastructure
        services.AddScoped<DatabaseConfig>();

        // Shared Repositories
        services.AddScoped<IUsuarioSharedRepository, UsuarioSharedRepository>();
        services.AddScoped<IPrendaSharedRepository, PrendaSharedRepository>();
        services.AddScoped<IReporteSharedRepository, ReporteSharedRepository>();
        services.AddScoped<ITruequeSharedRepository, TruequeSharedRepository>();

        // Shared Services
        services.AddScoped<UsuarioRepository>();
        services.AddScoped<CustomAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
        services.AddScoped<UsuarioService>();
        services.AddScoped<AutenticacionService>();
        services.AddScoped<PerfilService>();
        services.AddScoped<PrendaService>();
        services.AddScoped<Services.EvaluacionService>();
        services.AddScoped<RegionService>();
        services.AddScoped<ComunaService>();
        services.AddScoped<NotificacionService>();
        services.AddScoped<ReporteService>();
        services.AddScoped<TruequeService>();
        services.AddScoped<DropdownService>();

        // Feature DIs - Autenticacion
        services.AddRegistroFeature();
        services.AddInicioSesionFeature();
        services.AddRecuperarPasswordFeature();
        services.AddOnboardingFeature();

        // Feature DIs - Perfil
        services.AddCompletarPerfilFeature();
        services.AddEditarPerfilFeature();
        services.AddPerfilPublicoFeature();

        // Feature DIs - Prendas
        services.AddExplorarFeature();
        services.AddDetallePrendaFeature();
        services.AddPublicarPrendaFeature();
        services.AddEditarPrendaFeature();
        services.AddMisPrendasFeature();

        // Feature DIs - Trueque
        services.AddProponerTruequeFeature();
        services.AddPropuestasRecibidasFeature();
        services.AddDetallePropuestaFeature();
        services.AddEvaluacionFeature();

        // Feature DIs - Utilidades
        services.AddNotificacionesFeature();
        services.AddReportarFeature();

        // Feature DIs - Admin
        services.AddAdminFeature();

        // Feature DIs - Home
        services.AddHomeFeature();

        // Feature DIs - Favoritos (RF-08)
        services.AddFavoritosFeature();

        // Feature DIs - Mensajeria (RF-12)
        services.AddMensajeriaFeature();

        // Feature DIs - Nuevos Modelos
        services.AddSeguimientoUsuarioFeature();
        services.AddConfiguracionInterfazFeature();
        services.AddConfiguracionModuloFeature();
        services.AddLogInteraccionUIFeature();
        services.AddPrendaEtiquetaFeature();

        return services;
    }
}
