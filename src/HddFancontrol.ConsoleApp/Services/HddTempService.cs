using System.Text.RegularExpressions;

namespace HddFancontrol.ConsoleApp.Services.Classes;

public partial class HddTempService(ILogger<HddTempService> logger, IOptionsMonitor<GeneralSettings> generalSettings) : IHddTempService
{
    public async Task<IEnumerable<int>> GetAllHddTempsAsync()
    {
        logger.LogDebug("Getting temps for all disks with smartctl");

        var hddTemps = (await Task.WhenAll((await "lsblk -d -o NAME -n".BashAsync())
            .Trim()
            .Split(Environment.NewLine)
            .Where(disk => generalSettings.CurrentValue.ExcludePatter is not null ? !new Regex(generalSettings.CurrentValue.ExcludePatter).IsMatch(disk) : true)
            .Select(async disk =>
            {
                var match = tempRegex().Match(await $"smartctl -a /dev/{disk} | grep Temperature".BashAsync());

                if (match.Groups["nvmeTemp"].Success)
                {
                    return int.Parse(match.Groups["nvmeTemp"].Value);
                }

                if (match.Groups["hddTemp"].Success)
                {
                    return int.Parse(match.Groups["hddTemp"].Value);
                }

                return 0;
            })))
            .Where(hddTemp => hddTemp > 0)
            .OrderByDescending(hddTemp => hddTemp);

        return hddTemps;
    }

    public async Task<int?> GetHddTempAsync(string diskPath)
    {
        logger.LogDebug("Getting temps with hddtemp for {DiskPath}", diskPath);

        var tempResponse = await $"hddtemp --numeric {diskPath}".BashAsync();
        if (tempResponse.Contains("No such file or directory"))
            throw new ArgumentException($"{diskPath} is not a valid drive path");

        return int.TryParse(tempResponse, out int temp) ? temp : null;
    }

    [GeneratedRegex(@"(?:Temperature:\s+(?<nvmeTemp>\d+)\s+Celsius$|-\s+(?<hddTemp>\d+)\s*)", RegexOptions.Multiline)]
    private static partial Regex tempRegex();
}