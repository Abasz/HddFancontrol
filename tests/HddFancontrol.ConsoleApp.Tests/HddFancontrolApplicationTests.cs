namespace HddFancontrol.ConsoleApp.Tests;

public class HddFancontrolApplicationTests
{
    private readonly Mock<IPwmManagerService> _mockPwmManager;
    private readonly Mock<IHddTempService> _mockHddTempService;
    private readonly HddFancontrolApplication _hddFancontrolApplication;

    public HddFancontrolApplicationTests()
    {
        _mockPwmManager = new();
        _mockHddTempService = new();

        _hddFancontrolApplication = new HddFancontrolApplication(new NullLogger<HddFancontrolApplication>(), _mockPwmManager.Object, _mockHddTempService.Object);
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationExceptionWhenNoHddTempAvailable()
    {
        _mockHddTempService
            .Setup(x => x.GetAllHddTempsAsync())
            .ReturnsAsync([]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hddFancontrolApplication.RunAsync()
        );
    }

    [Fact]
    public async Task ShouldCalculatePwmBasedOnHighestTemp()
    {
        var temps = Enumerable.Range(30, 3).OrderByDescending(x => x);
        _mockHddTempService
            .Setup(x => x.GetAllHddTempsAsync())
            .ReturnsAsync(temps);

        await _hddFancontrolApplication.RunAsync();

        _mockPwmManager.Verify(x => x.CalculatePwms(temps.ElementAt(0)), Times.Once);
    }

    [Fact]
    public async Task ShouldUpdateAllPwmFiles()
    {
        var temps = Enumerable.Range(30, 3).OrderByDescending(x => x);
        _mockHddTempService
            .Setup(x => x.GetAllHddTempsAsync())
            .ReturnsAsync(temps);
        _mockPwmManager.Setup(x => x.CalculatePwms(It.IsAny<int>())).Returns(temps.Select(temp => temp + 10));

        await _hddFancontrolApplication.RunAsync();

        _mockPwmManager.Verify(x => x.UpdatePwmFileAsync(It.IsAny<int>(), It.Is<string>(x => Regex.IsMatch(x, @"^pwm\d"))), Times.Exactly(temps.Count()));
    }
}