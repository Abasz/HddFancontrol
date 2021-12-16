namespace HddFancontrol.ConsoleApp;

public class Startup : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHostApplicationLifetime _app;
    private readonly IOptionsMonitor<GeneralSettings> _generalSettings;
    private readonly IOptionsMonitor<List<PwmSettings>> _pwmSettings;
    private readonly ILogger<Startup> _logger;

    public Startup(ILogger<Startup> logger, IServiceProvider services, IHostApplicationLifetime app, IOptionsMonitor<GeneralSettings> generalSettings, IOptionsMonitor<List<PwmSettings>> pwmSettings)
    {
        _logger = logger;
        _services = services;
        _app = app;
        _generalSettings = generalSettings;
        _pwmSettings = pwmSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting HDD Fancontrol service");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Starting check run");

                using var scope = _services.CreateScope();

                await scope.ServiceProvider
                    .GetRequiredService<IHddFancontrolApplication>()
                    .RunAsync();

                var interval = _generalSettings.CurrentValue.Interval;
                _logger.LogDebug("Waiting {Interval} seconds", interval);
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Stopping HDD Fancontrol service and setting pwm(s) to full speed");

            await SetMaxPwmAsync();
        }
        catch (OptionsValidationException e)
        {
            var validationErrors = e.Failures.Select(error =>
            {
                var keyAndMessage = error.Split('|');
                return new Dictionary<string, string>()
                {
                    {
                        keyAndMessage[0],
                            keyAndMessage[1]
                    }
                };
            });

            _logger.LogError("Settings validation error in {Property}: {@ValidationErrors}", e.OptionsName, validationErrors);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unhandled exception occurred, setting pwm(s) to full speed");
            await SetMaxPwmAsync();
        }
        finally
        {
            _app.StopApplication();
        }
    }

    private async Task SetMaxPwmAsync()
    {
        var scope = _services.CreateScope();
        var pwmManagerService =
            scope.ServiceProvider
            .GetRequiredService<IPwmManagerService>();

        await Task.WhenAll(
            _pwmSettings.CurrentValue.Select(
                (pwm, index) => pwmManagerService.UpdatePwmFileAsync(pwm.MaxPwm, $"pwm{index + 1}")
            )
        );
    }
}