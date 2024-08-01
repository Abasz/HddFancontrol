namespace HddFancontrol.ConsoleApp.Tests;

public class PwmManagerServiceTests
{
    private readonly Mock<IOptionsSnapshot<GeneralSettings>> _mockGeneralSettingsOptions;
    private readonly Mock<IOptionsSnapshot<List<PwmSettings>>> _mockPwmSettingsOptions;
    private readonly List<PwmSettings> _pwmSettings =
    [
        new PwmSettings
        {
        MinTemp = 37,
        MaxTemp = 51,
        MinStart = 48,
        MinPwm = 0,
        MaxPwm = 255
        }
    ];

    public PwmManagerServiceTests()
    {
        _mockGeneralSettingsOptions = new Mock<IOptionsSnapshot<GeneralSettings>>();
        _mockGeneralSettingsOptions.Setup(m => m.Value).Returns(new GeneralSettings
        {
            DevPath = "./",
            Interval = 10
        });

        _mockPwmSettingsOptions = new Mock<IOptionsSnapshot<List<PwmSettings>>>();
        _mockPwmSettingsOptions.Setup(m => m.Value).Returns(_pwmSettings);
    }

    [Fact]
    public async Task ShouldGetCurrentPwms()
    {
        _mockGeneralSettingsOptions.Setup(m => m.Value).Returns(new GeneralSettings
        {
            DevPath = "/sys/class/hwmon/hwmon1/",
            Interval = 10
        });

        var pwmManagerService = new PwmManagerService(new NullLogger<PwmManagerService>(), _mockGeneralSettingsOptions.Object, _mockPwmSettingsOptions.Object);

        var pwms = await pwmManagerService.GetCurrentPwmsAsync();

        Assert.Equal(2, pwms.Count());
    }

    [Fact]
    public async Task ShouldUpdatePwm()
    {
        var pwmManagerService = new PwmManagerService(new NullLogger<PwmManagerService>(), _mockGeneralSettingsOptions.Object, _mockPwmSettingsOptions.Object);

        await pwmManagerService.UpdatePwmFileAsync(1, "pwm1");

        var testPwm = await File.ReadAllTextAsync("pwm1");
        File.Delete("pwm1");

        Assert.Equal(1, int.Parse(testPwm));
    }

    [Fact]
    public void ShouldCalculatePwmsWhenBelowMinTemp()
    {
        var pwmManagerService = new PwmManagerService(new NullLogger<PwmManagerService>(), _mockGeneralSettingsOptions.Object, _mockPwmSettingsOptions.Object);

        var pwms = pwmManagerService.CalculatePwms(_mockPwmSettingsOptions.Object.Value[0].MinTemp - 1);

        Assert.All(pwms, pwm =>
        {
            Assert.Equal(_mockPwmSettingsOptions.Object.Value[0].MinPwm, pwm);
        });
    }

    [Fact]
    public void ShouldCalculatePwmsWhenExactleyMinTemp()
    {
        var pwmManagerService = new PwmManagerService(new NullLogger<PwmManagerService>(), _mockGeneralSettingsOptions.Object, _mockPwmSettingsOptions.Object);

        var pwms = pwmManagerService.CalculatePwms(_mockPwmSettingsOptions.Object.Value[0].MinTemp);

        Assert.All(pwms, pwm =>
        {
            Assert.Equal(_mockPwmSettingsOptions.Object.Value[0].MinStart, pwm);
        });
    }

    [Fact]
    public void ShouldCalculatePwmsWhenMaxTemp()
    {
        var pwmManagerService = new PwmManagerService(new NullLogger<PwmManagerService>(), _mockGeneralSettingsOptions.Object, _mockPwmSettingsOptions.Object);

        var pwms = pwmManagerService.CalculatePwms(_mockPwmSettingsOptions.Object.Value[0].MaxTemp);

        Assert.All(pwms, pwm =>
        {
            Assert.Equal(_mockPwmSettingsOptions.Object.Value[0].MaxPwm, pwm);
        });
    }

    [Fact]
    public void ShouldCalculatePwmsWhenBetweenMinMax()
    {
        var pwmSetting = _mockPwmSettingsOptions.Object.Value[0];
        var hddTemp = pwmSetting.MinTemp + (pwmSetting.MaxTemp - pwmSetting.MinTemp) / 2;
        var pwmManagerService = new PwmManagerService(new NullLogger<PwmManagerService>(), _mockGeneralSettingsOptions.Object, _mockPwmSettingsOptions.Object);

        var pwms = pwmManagerService.CalculatePwms(hddTemp);

        Assert.All(pwms, pwm =>
        {
            var pwmStep = (pwmSetting.MaxPwm - pwmSetting.MinStart) / (pwmSetting.MaxTemp - pwmSetting.MinTemp);
            var expectedPwm = pwmSetting.MinStart + (hddTemp - pwmSetting.MinTemp) * pwmStep;
            Assert.Equal(expectedPwm, pwm);
        });
    }
}