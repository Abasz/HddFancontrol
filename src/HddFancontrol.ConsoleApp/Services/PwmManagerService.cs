using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HddFancontrol.ConsoleApp.Libs;
using HddFancontrol.ConsoleApp.Models;
using HddFancontrol.ConsoleApp.Services.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HddFancontrol.ConsoleApp.Services.Classes
{
    public class PwmManagerService : IPwmManagerService
    {
        private readonly ILogger<PwmManagerService> _logger;
        private readonly List<PwmSettings> _pwmSettings;
        private readonly GeneralSettings _generalSettings;

        public PwmManagerService(ILogger<PwmManagerService> logger, IOptionsSnapshot<GeneralSettings> generalSettings, IOptionsSnapshot<List<PwmSettings>> pwmSettings)
        {
            _logger = logger;
            _pwmSettings = pwmSettings.Value;
            _generalSettings = generalSettings.Value;
        }

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

        public IEnumerable<int> CalculatePwms(int hddTemp)
        {
            var pwms = _pwmSettings.Select((pwmSetting, index) =>
            {
                index++;
                if (hddTemp < pwmSetting.MinTemp)
                {
                    _logger.LogDebug("Pwm for pwm{Index} is MinPwm ({MinPwm})", index, pwmSetting.MinPwm);
                    return pwmSetting.MinPwm;
                }

                if (hddTemp >= pwmSetting.MaxTemp)
                {
                    _logger.LogDebug("Pwm for pwm{Index} is MaxPwm ({MaxPwm})", index, pwmSetting.MaxPwm);
                    return pwmSetting.MaxPwm;
                }

                var pwmStep = (pwmSetting.MaxPwm - pwmSetting.MinStart) / (pwmSetting.MaxTemp - pwmSetting.MinTemp);
                var pwm = pwmSetting.MinStart + (hddTemp - pwmSetting.MinTemp) * pwmStep;

                _logger.LogDebug("Pwm for pwm{Index} is {Pwm}", index, pwm);

                return pwm;
            });

            return pwms;
        }
    }
}