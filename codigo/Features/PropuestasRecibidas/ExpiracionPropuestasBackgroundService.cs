using TruequeTextil.Features.PropuestasRecibidas.Interfaces;

namespace TruequeTextil.Features.PropuestasRecibidas;

/// <summary>
/// Background service que ejecuta diariamente la verificación de propuestas expiradas (RF-11)
/// </summary>
public class ExpiracionPropuestasBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiracionPropuestasBackgroundService> _logger;
    private readonly TimeSpan _intervaloEjecucion = TimeSpan.FromHours(24); // Ejecutar una vez al día

    public ExpiracionPropuestasBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ExpiracionPropuestasBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiracionPropuestasBackgroundService iniciado. Se ejecutará cada 24 horas.");

        // Ejecutar inmediatamente al inicio
        await ProcesarExpiraciones(stoppingToken);

        // Luego ejecutar cada 24 horas
        using var timer = new PeriodicTimer(_intervaloEjecucion);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcesarExpiraciones(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ExpiracionPropuestasBackgroundService detenido.");
        }
    }

    private async Task ProcesarExpiraciones(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Iniciando procesamiento de propuestas expiradas - {Fecha}", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var expiracionService = scope.ServiceProvider.GetRequiredService<IExpiracionPropuestasService>();

            // 1. Procesar propuestas expiradas
            var propuestasExpiradas = await expiracionService.ProcesarPropuestasExpiradas();

            if (propuestasExpiradas > 0)
            {
                _logger.LogInformation(
                    "Se procesaron {Cantidad} propuestas expiradas y se enviaron notificaciones a los usuarios afectados",
                    propuestasExpiradas);
            }

            // 2. Enviar recordatorios de propuestas próximas a expirar (menos de 48 horas)
            var propuestasProximas = await expiracionService.ObtenerPropuestasProximasAExpirar();

            if (propuestasProximas.Count > 0)
            {
                _logger.LogInformation(
                    "Se enviaron recordatorios para {Cantidad} propuestas próximas a expirar",
                    propuestasProximas.Count);
            }

            _logger.LogInformation("Procesamiento de propuestas expiradas completado - {Fecha}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar propuestas expiradas en background service");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiracionPropuestasBackgroundService deteniendo...");
        await base.StopAsync(stoppingToken);
    }
}
