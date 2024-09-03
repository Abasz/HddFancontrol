namespace HddFancontrol.ConsoleApp.Tests;

public class HddTempServiceTests
{
    private readonly HddTempService _hddTempService;

    public HddTempServiceTests()
    {
        _hddTempService = new HddTempService(new NullLogger<HddTempService>());
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
}