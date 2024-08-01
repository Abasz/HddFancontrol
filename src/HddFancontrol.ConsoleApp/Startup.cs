namespace HddFancontrol.ConsoleApp;

public class Startup(ILogger<Startup> logger, IServiceProvider services, IHostApplicationLifetime app, IOptionsMonitor<GeneralSettings> generalSettings, IOptionsMonitor<List<PwmSettings>> pwmSettings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting HDD Fancontrol service");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug("Starting check run");

                using var scope = services.CreateScope();

                await scope.ServiceProvider
                    .GetRequiredService<IHddFancontrolApplication>()
                    .RunAsync();

                var interval = generalSettings.CurrentValue.Interval;
                logger.LogDebug("Waiting {Interval} seconds", interval);
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Stopping HDD Fancontrol service and setting pwm(s) to full speed");

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

            logger.LogError("Settings validation error in {Property}: {@ValidationErrors}", e.OptionsName, validationErrors);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An unhandled exception occurred, setting pwm(s) to full speed");
            await SetMaxPwmAsync();
        }
        finally
        {
            app.StopApplication();
        }
    }

    private async Task SetMaxPwmAsync()
    {
        var scope = services.CreateScope();
        var pwmManagerService =
            scope.ServiceProvider
            .GetRequiredService<IPwmManagerService>();

        await Task.WhenAll(
            pwmSettings.CurrentValue.Select(
                (pwm, index) => pwmManagerService.UpdatePwmFileAsync(pwm.MaxPwm, $"pwm{index + 1}")
            )
        );
    }
}