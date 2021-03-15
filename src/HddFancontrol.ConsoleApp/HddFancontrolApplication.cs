using System;
using System.Linq;
using System.Threading.Tasks;

using HddFancontrol.ConsoleApp.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace HddFancontrol.ConsoleApp
{
    public class HddFancontrolApplication : IHddFancontrolApplication
    {
        private readonly ILogger<HddFancontrolApplication> _logger;
        private readonly IPwmManagerService _pwmManagerService;
        private readonly IHddTempService _hddTempService;

        public HddFancontrolApplication(ILogger<HddFancontrolApplication> logger, IPwmManagerService pwmManagerService, IHddTempService hddTempService)
        {
            _logger = logger;
            _pwmManagerService = pwmManagerService;
            _hddTempService = hddTempService;
        }

        public async Task RunAsync()
        {
            var hddTemps = await _hddTempService.GetAllHddTempsAsync();

            _logger.LogDebug("Current HDD temps: {hddTemps}", hddTemps);

            if (!hddTemps.Any())
                throw new InvalidOperationException("There are no HDD temps available");

            // should decide wether getting current pwm and if equals to the to be set, no update is performed.
            await Task.WhenAll(
                _pwmManagerService.CalculatePwms(hddTemps.ElementAt(0))
                .Select((pwm, index) => _pwmManagerService.UpdatePwmFileAsync(pwm, $"pwm{index + 1}"))
            );
        }
    }

    public interface IHddFancontrolApplication
    {
        Task RunAsync();
    }
}