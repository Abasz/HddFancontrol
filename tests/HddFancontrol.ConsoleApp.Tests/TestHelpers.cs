namespace HddFancontrol.ConsoleApp.Tests;

public static class TestHelpers
{
    public static IServiceCollection AddMockLogging(this IServiceCollection services, Mock<ILogger> mockLogger = null)
    {
        if (mockLogger is null)
            mockLogger = new Mock<ILogger>();

        var loggerFactory = new Mock<ILoggerFactory>();

        loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);
        mockLogger
            .Setup(x => x.IsEnabled(It.Is<LogLevel>(level => level == LogLevel.Debug)))
            .Returns(true);
        mockLogger
            .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);

        services.AddSingleton(sp => loggerFactory.Object);

        return services;
    }
}