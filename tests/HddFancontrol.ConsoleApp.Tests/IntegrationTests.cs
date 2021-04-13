using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using HddFancontrol.ConsoleApp.Libs.ServiceExtentions;
using HddFancontrol.ConsoleApp.Services.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace HddFancontrol.ConsoleApp.Tests
{
    public class IntegrationTests
    {
        private readonly Mock<IPwmManagerService> _mockPwmManager;
        private readonly Mock<IHddFancontrolApplication> _mockHddFancontrolApplication;
        private readonly Mock<ILogger> _mockLogger;
        private readonly IHostBuilder _hostBuilder;
        private readonly List<KeyValuePair<string, string>> _settings =
            new()
            {
                new KeyValuePair<string, string>("generalSettings:Interval", "11"),
                new KeyValuePair<string, string>("generalSettings:DevPath", "./"),
                new KeyValuePair<string, string>("pwmSettings:0:MinTemp", "29"),
                new KeyValuePair<string, string>("pwmSettings:0:MaxTemp", "43"),
                new KeyValuePair<string, string>("pwmSettings:0:MinStart", "48"),
                new KeyValuePair<string, string>("pwmSettings:0:MinPwm", "0"),
                new KeyValuePair<string, string>("pwmSettings:0:MaxPwm", "255"),
                new KeyValuePair<string, string>("pwmSettings:1:MinTemp", "29"),
                new KeyValuePair<string, string>("pwmSettings:1:MaxTemp", "43"),
                new KeyValuePair<string, string>("pwmSettings:1:MinStart", "30"),
                new KeyValuePair<string, string>("pwmSettings:1:MinPwm", "0"),
                new KeyValuePair<string, string>("pwmSettings:1:MaxPwm", "255")
            };

        public IntegrationTests()
        {
            _mockPwmManager = new();
            _mockHddFancontrolApplication = new();
            _mockLogger = new();

            _hostBuilder = Program
                .CreateHostBuilder(Array.Empty<string>())
                .ConfigureAppConfiguration(
                    (hostContext, configApp) =>
                    {
                        configApp.Sources.Clear();
                        configApp.AddInMemoryCollection(_settings);
                    }
                )
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient(p => _mockPwmManager.Object);
                    services.AddScoped(p => _mockHddFancontrolApplication.Object);
                    services.AddMockLogging(_mockLogger);
                });
        }

        [Fact]
        public async void AppShouldStart()
        {
            var ct = new CancellationTokenSource(2000);
            var app = _hostBuilder.Build();

            var runningApp = app.RunAsync(ct.Token);
            ct.Cancel();
            await runningApp;

            _mockHddFancontrolApplication.Verify(x => x.RunAsync(), Times.Once);
            _mockLogger.Verify(x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((x, _) => x!.ToString() !.Contains("Hosting started")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public async void AppShouldStopAndSetMaxPwmWhenGracefullShutdown()
        {
            var ct = new CancellationTokenSource(2000);
            var app = _hostBuilder.Build();

            var runningApp = app.RunAsync(ct.Token);
            ct.Cancel();
            await runningApp;

            _mockPwmManager.Verify(x => x.UpdatePwmFileAsync(It.Is<int>(x => x == 255), It.Is<string>(x => x.Contains("pwm"))), Times.Exactly(2));
            _mockLogger.Verify(x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((x, _) => x!.ToString() !.Contains("Hosting stopped")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public async void AppShouldShutdownWithSettingMaxPwmWhenUnhandledException()
        {
            var app = _hostBuilder.Build();
            _mockHddFancontrolApplication.Setup(x => x.RunAsync()).ThrowsAsync(new Exception("Test unhandled exception"));

            var runningApp = app.RunAsync();
            await runningApp;

            _mockPwmManager.Verify(x => x.UpdatePwmFileAsync(It.Is<int>(x => x == 255), It.Is<string>(x => x.Contains("pwm"))), Times.Exactly(2));
            _mockLogger.Verify(x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((x, _) => x!.ToString() !.Contains("Hosting stopped")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public async void AppShouldShutdownWithoutSettingMaxPwmWhenInvalidSettings()
        {
            _settings[0] = new KeyValuePair<string, string>("generalSettings:Interval", "0");
            var app = _hostBuilder.Build();

            var runningApp = app.RunAsync();
            await runningApp;

            _mockPwmManager.Verify(x => x.UpdatePwmFileAsync(It.Is<int>(x => x == 255), It.Is<string>(x => x.Contains("pwm"))), Times.Never);
            _mockLogger.Verify(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((x, _) => x!.ToString() !.Contains("Settings validation error in")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
            _mockLogger.Verify(x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((x, _) => x!.ToString() !.Contains("Hosting stopped")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact(DisplayName = "Should set logging file directory according to appsettings")]
        public async void AppShouldSetLogDirectoryBasedOnSettings()
        {
            var logDirName = "CustomLogsDirectory";
            if (Directory.Exists(logDirName))
                Directory.Delete(logDirName, true);

            var ct = new CancellationTokenSource(2000);
            var app = _hostBuilder
                .UseCustomSerilog()
                .ConfigureAppConfiguration(
                    (hostContext, configApp) =>
                    {
                        _settings.Add(new KeyValuePair<string, string>("Serilog:LogDirectory", logDirName));
                        _settings.Add(new KeyValuePair<string, string>("Serilog:MinimumLevel:Default", "Debug"));
                        configApp.Sources.Clear();
                        configApp.AddInMemoryCollection(_settings);
                    }
                )
                .Build();

            var runningApp = app.RunAsync(ct.Token);
            ct.Cancel();
            await runningApp;

            Assert.True(Directory.Exists(logDirName));

            Directory.Delete(logDirName, true);
        }

        [Fact(DisplayName = "Should set logging file directory to default if no settings in  appsettings")]
        public async void AppShouldSetLogDirectoryToDefault()
        {
            if (Directory.Exists("Logs"))
                Directory.Delete("Logs", true);

            var ct = new CancellationTokenSource(2000);
            var app = _hostBuilder
                .UseCustomSerilog()
                .ConfigureAppConfiguration(
                    (hostContext, configApp) =>
                    {
                        _settings.Add(new KeyValuePair<string, string>("Serilog:MinimumLevel:Default", "Debug"));
                        configApp.Sources.Clear();
                        configApp.AddInMemoryCollection(_settings);
                    }
                )
                .Build();

            var runningApp = app.RunAsync(ct.Token);
            ct.Cancel();
            await runningApp;

            Assert.True(Directory.Exists("Logs"));

            Directory.Delete("Logs", true);
        }

        [Fact(DisplayName = "Should rotate the log file if exceeds limit")]
        public async void AppShouldRotateLogWhenExceedingLimit()
        {
            if (Directory.Exists("Logs"))
                Directory.Delete("Logs", true);

            await File.WriteAllBytesAsync($"{Directory.CreateDirectory("Logs").FullName}/hdd-fancontrol.log", new byte[2_000_000]);

            var ct = new CancellationTokenSource(2000);
            var app = _hostBuilder
                .UseCustomSerilog()
                .ConfigureAppConfiguration(
                    (hostContext, configApp) =>
                    {
                        _settings.Add(new KeyValuePair<string, string>("Serilog:MinimumLevel:Default", "Debug"));
                        configApp.Sources.Clear();
                        configApp.AddInMemoryCollection(_settings);
                    }
                )
                .Build();

            var runningApp = app.RunAsync(ct.Token);
            ct.Cancel();
            await runningApp;

            Assert.Collection(Directory.GetFiles("Logs"),
                fileName =>
                {
                    Assert.Contains("hdd-fancontrol", fileName);
                },
                fileName =>
                {
                    Assert.Contains("hdd-fancontrol", fileName);
                });

            Directory.Delete("Logs", true);
        }
    }
}