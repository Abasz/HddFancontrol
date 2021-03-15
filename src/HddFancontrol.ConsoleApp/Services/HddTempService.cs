using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HddFancontrol.ConsoleApp.Libs;
using HddFancontrol.ConsoleApp.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace HddFancontrol.ConsoleApp.Services.Classes
{
    public class HddTempService : IHddTempService
    {
        private readonly ILogger<HddTempService> _logger;

        public HddTempService(ILogger<HddTempService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetAllHddTempsAsync()
        {
            _logger.LogDebug("Getting temps with hddtemp (hddtemp --numeric /dev/sd[a-z])");
            var hddTemps = (await "hddtemp --numeric /dev/sd[a-z]".BashAsync())
                .Split("\n")
                .Select(hdd =>
                {
                    _ = int.TryParse(hdd, out int hddTemp);

                    return hddTemp;
                })
                .Where(hddTemp => hddTemp > 0)
                .OrderByDescending(hddTemp => hddTemp);

            return hddTemps;
        }

        public async Task<int?> GetHddTempAsync(string diskPath)
        {
            _logger.LogDebug("Getting temps with hddtemp for {DiskPath}", diskPath);

            var tempResponse = await $"hddtemp --numeric {diskPath}".BashAsync();
            if (tempResponse.Contains("No such file or directory"))
                throw new ArgumentException($"{diskPath} is not a valid drive path");

            return int.TryParse(tempResponse, out int temp) ? temp : null;
        }
    }
}