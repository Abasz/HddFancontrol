using Serilog;
using Serilog.Formatting.Compact;

namespace HddFancontrol.ConsoleApp.Libs.ServiceExtentions;

public static class AddLogging
{
    public static IHostBuilder UseCustomSerilog(this IHostBuilder host)
    {
        host.UseSerilog((hostingContext, services, loggerConfiguration) =>
        {
            var logDirectory = Directory.CreateDirectory(
                hostingContext.Configuration["Serilog:LogDirectory"] ??
                Path.Combine(
                    hostingContext.HostingEnvironment.ContentRootPath,
                    "Logs")
            ).FullName;

            loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .WriteTo.Console()
                .WriteTo.Async(asyncLogger => asyncLogger.File(
                    formatter: new CompactJsonFormatter(),
                    path: Path.Combine(logDirectory, "hdd-fancontrol.log"),
                    fileSizeLimitBytes : 2_000_000,
                    rollOnFileSizeLimit : true,
                    retainedFileCountLimit : 5,
                    flushToDiskInterval : TimeSpan.FromSeconds(1)));
        });

        return host;
    }
}