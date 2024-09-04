namespace HddFancontrol.ConsoleApp.Tests;

public class StartupTests
{
    private readonly Mock<IPwmManagerService> _mockPwmManager;
    private readonly Mock<IHddFancontrolApplication> _mockHddFancontrolApplication;
    private readonly Mock<IHostApplicationLifetime> _mockAppLifetime;
    private readonly List<PwmSettings> _mockPwmSettings;
    private readonly GeneralSettings _mockGeneralSettings;
    private readonly Mock<IOptionsMonitor<GeneralSettings>> _mockGeneralSettingsMonitor;
    private readonly Mock<IOptionsMonitor<List<PwmSettings>>> _mockPwmSettingsMonitor;
    private readonly Mock<ILogger<Startup>> _mockLogger;
    private readonly ServiceCollection _services;
    private readonly Startup _startup;

    public StartupTests()
    {
        _mockPwmSettings = [
            new PwmSettings
            {
                FanId = 1,
                MinTemp = 37,
                MaxTemp = 51,
                MinStart = 48,
                MinPwm = 0,
                MaxPwm = 255
            },
            new PwmSettings
            {
                FanId = 4,
                MinTemp = 37,
                MaxTemp = 51,
                MinStart = 48,
                MinPwm = 0,
                MaxPwm = 255
            },
            new PwmSettings
            {
                MinTemp = 37,
                MaxTemp = 51,
                MinStart = 48,
                MinPwm = 0,
                MaxPwm = 255
            }];
        _mockGeneralSettings = new()
        {
            DevPath = "./",
            Interval = 1
        };

        _mockGeneralSettingsMonitor = new();
        _mockGeneralSettingsMonitor
            .SetupGet(x => x.CurrentValue)
            .Returns(_mockGeneralSettings);

        _mockPwmSettingsMonitor = new();
        _mockPwmSettingsMonitor
            .SetupGet(x => x.CurrentValue)
            .Returns(_mockPwmSettings);

        _mockLogger = new();
        _mockPwmManager = new();
        _mockHddFancontrolApplication = new();
        _mockAppLifetime = new();

        _services = new ServiceCollection();
        _services.AddTransient(x => _mockPwmManager.Object);
        _services.AddScoped(x => _mockHddFancontrolApplication.Object);

        _startup = new Startup(
            _mockLogger.Object,
            _services.BuildServiceProvider(),
            _mockAppLifetime.Object,
            _mockGeneralSettingsMonitor.Object,
            _mockPwmSettingsMonitor.Object
        );
    }

    [Fact]
    public async Task ShouldStopInvokingHDDApplicationRunMethodOnCancellation()
    {
        var ct = new CancellationTokenSource();
        ct.Cancel();
        await _startup.StartAsync(ct.Token);

        _mockHddFancontrolApplication.Verify(x => x.RunAsync(), Times.Never);
    }

    [Fact]
    public async Task ShouldCallHDDApplicationRunMethodPeriodically()
    {
        await _startup.StartAsync(CancellationToken.None);
        await _startup.StopAsync(CancellationToken.None);

        _mockHddFancontrolApplication.Verify(x => x.RunAsync(), Times.Once);
        _mockLogger.Verify(x =>
            x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((x, _) => x!.ToString()!.Contains($"Waiting {_mockGeneralSettings.Interval} seconds")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }

    [Fact(DisplayName = "Should set max pwm when application exits")]
    public async Task ShouldSetMaxPwmOnExit()
    {
        await _startup.StartAsync(CancellationToken.None);
        await _startup.StopAsync(CancellationToken.None);

        var index = 1;
        Assert.All(_mockPwmSettings, setting =>
        {
            _mockPwmManager.Verify(x =>
                x.UpdatePwmFileAsync(
                    It.Is<int>(x => x == setting.MaxPwm),
                    It.Is<string>(x => x == $"pwm{setting.FanId ?? index}")));
            index++;
        });
    }

    [Fact(DisplayName = "Should log validation errors on invalid settings and exit application")]
    public async Task ShouldLogValidationErrorsOnInvalidSettingsAndExit()
    {
        var errorMessageKey = "DevPath";
        var errorMessageValue = "Path is required";
        var exception = new OptionsValidationException("GeneralSettings", typeof(GeneralSettings),
        [
            $"{errorMessageKey}|{errorMessageValue}"
        ]);
        _mockHddFancontrolApplication
            .Setup(x => x.RunAsync())
            .ThrowsAsync(exception);

        await _startup.StartAsync(CancellationToken.None);
        await _startup.StopAsync(CancellationToken.None);

        _mockLogger.Verify(x =>
            x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((x, _) => Regex.IsMatch(x!.ToString(), $@"^Settings validation error in {exception.OptionsName}:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        _mockAppLifetime.Verify(x => x.StopApplication());
    }

    [Fact(DisplayName = "Should log error and shutdown application on unhandled exception")]
    public async Task ShouldLogErrorAndShutdownOnUnhandledException()
    {
        var exception = new Exception("Test exception");
        _mockHddFancontrolApplication
            .Setup(x => x.RunAsync())
            .ThrowsAsync(exception);

        await _startup.StartAsync(CancellationToken.None);
        await _startup.StopAsync(CancellationToken.None);

        _mockLogger.Verify(x =>
            x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>((x) => x == exception),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        _mockAppLifetime.Verify(x => x.StopApplication(), Times.Once);
    }
}