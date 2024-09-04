namespace HddFancontrol.ConsoleApp.Tests;

public partial class HddTempServiceTests
{
    private readonly GeneralSettings _noExcludeGeneralSettings;

    private readonly HddTempService _hddTempService;
    private readonly Mock<IOptionsMonitor<GeneralSettings>> _mockGeneralSettingsMonitor;


    public HddTempServiceTests()
    {
        _noExcludeGeneralSettings = new()
        {
            DevPath = "./",
            Interval = 1
        };

        _mockGeneralSettingsMonitor = new();
        _mockGeneralSettingsMonitor
            .SetupGet(x => x.CurrentValue)
            .Returns(_noExcludeGeneralSettings);
        _hddTempService = new HddTempService(new NullLogger<HddTempService>(), _mockGeneralSettingsMonitor.Object);
    }

    [Fact]
    public async Task ShouldGetHddTempForSpecificDrive()
    {
        var temp = await _hddTempService.GetHddTempAsync("/dev/sda");

        Assert.NotEqual(0, temp);
    }

    [Fact]
    public async Task ShouldReturnNullWhenTempIsNotAvailableForSpecificDrive()
    {
        var temp = await _hddTempService.GetHddTempAsync("/dev/sde");

        Assert.Null(temp);
    }

    [Fact]
    public async Task ShouldThrowArgumentExceptionWhenDrivePathIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _hddTempService.GetHddTempAsync("/invalid/path"));
    }

    [Fact]
    public async Task ShouldGetTempForAllDrivesWhereAvailable()
    {
        var temps = await _hddTempService.GetAllHddTempsAsync();

        Assert.NotEmpty(temps);
        var lastTemp = temps.ElementAt(temps.Count() - 1);
        var firstTemp = temps.ElementAt(0);
        Assert.True(firstTemp >= lastTemp, $"Invalid order {firstTemp} is less than {lastTemp}");
    }

    [Fact]
    public async Task ShouldGetTempForDrivesThatAreNotExcluded()
    {
        var allDrives = await _hddTempService.GetAllHddTempsAsync();

        _mockGeneralSettingsMonitor
            .SetupGet(x => x.CurrentValue)
            .Returns(new GeneralSettings
            {
                DevPath = _noExcludeGeneralSettings.DevPath,
                Interval = _noExcludeGeneralSettings.Interval,
                ExcludePatter = "sd.*",
            });

        var tempsWithExcluded = await _hddTempService.GetAllHddTempsAsync();

        Assert.NotEmpty(tempsWithExcluded);
        Assert.NotEqual(allDrives.Count(), tempsWithExcluded.Count());
    }
}