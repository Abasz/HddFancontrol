using System.Text.RegularExpressions;

namespace HddFancontrol.ConsoleApp.Services.Classes;

public class PwmManagerService(ILogger<PwmManagerService> logger, IOptionsSnapshot<GeneralSettings> generalSettings, IOptionsSnapshot<List<PwmSettings>> pwmSettings) : IPwmManagerService
{
    private readonly List<PwmSettings> _pwmSettings = pwmSettings.Value;
    private readonly GeneralSettings _generalSettings = generalSettings.Value;

    public Task UpdatePwmFileAsync(int pwm, string name)
    {
        return $"echo {pwm} > {Path.Combine(_generalSettings.DevPath, name)}"
            .BashAsync();
    }

    public async Task<IEnumerable<int>> GetCurrentPwmsAsync()
    {
        var pwmFiles = Directory.GetFiles(_generalSettings.DevPath).Where(x => Regex.IsMatch(x, $@"^{_generalSettings.DevPath}pwm\d$"));

        var pwms = await Task.WhenAll(
            pwmFiles.Select(pwmFile => File.ReadAllTextAsync(pwmFile))
        );

        return pwms.Select(pwm => int.Parse(pwm));
    }

    public IEnumerable<PwmDto> CalculatePwms(int hddTemp)
    {
        var pwms = _pwmSettings.Select((pwmSetting, index) =>
        {
            index = pwmSetting.FanId ?? index + 1;
            if (hddTemp < pwmSetting.MinTemp)
            {
                logger.LogDebug("Pwm for pwm{Index} is MinPwm ({MinPwm})", index, pwmSetting.MinPwm);
                return new PwmDto { Pwm = pwmSetting.MinPwm, Id = index };
            }

            if (hddTemp >= pwmSetting.MaxTemp)
            {
                logger.LogDebug("Pwm for pwm{Index} is MaxPwm ({MaxPwm})", index, pwmSetting.MaxPwm);
                return new PwmDto { Pwm = pwmSetting.MaxPwm, Id = index };
            }

            var pwmStep = (pwmSetting.MaxPwm - pwmSetting.MinStart) / (pwmSetting.MaxTemp - pwmSetting.MinTemp);
            var pwm = pwmSetting.MinStart + (hddTemp - pwmSetting.MinTemp) * pwmStep;

            logger.LogDebug("Pwm for pwm{Index} is {Pwm}", index, pwm);

            return new PwmDto { Pwm = pwm, Id = index };
        });

        return pwms;
    }
}