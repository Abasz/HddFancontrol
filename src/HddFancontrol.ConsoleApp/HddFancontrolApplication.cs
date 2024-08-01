namespace HddFancontrol.ConsoleApp;

public class HddFancontrolApplication(ILogger<HddFancontrolApplication> logger, IPwmManagerService pwmManagerService, IHddTempService hddTempService) : IHddFancontrolApplication
{
    public async Task RunAsync()
    {
        var hddTemps = await hddTempService.GetAllHddTempsAsync();

        logger.LogDebug("Current HDD temps: {hddTemps}", hddTemps);

        if (!hddTemps.Any())
            throw new InvalidOperationException("There are no HDD temps available");

        // should decide wether getting current pwm and if equals to the to be set, no update is performed.
        await Task.WhenAll(
            pwmManagerService.CalculatePwms(hddTemps.ElementAt(0))
            .Select((pwm, index) => pwmManagerService.UpdatePwmFileAsync(pwm, $"pwm{index + 1}"))
        );
    }
}

public interface IHddFancontrolApplication
{
    Task RunAsync();
}