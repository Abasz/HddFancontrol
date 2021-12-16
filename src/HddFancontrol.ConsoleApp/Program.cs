using HddFancontrol.ConsoleApp.Libs.ServiceExtentions;
using HddFancontrol.ConsoleApp.Services.Classes;

using Microsoft.Extensions.Configuration;

namespace HddFancontrol.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .UseCustomSerilog()
        .ConfigureAppConfiguration((hostContext, configApp) =>
        {
            configApp.AddJsonFile("pwm.settings.json", optional : false, reloadOnChange : true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddOptions<GeneralSettings>()
                .Bind(hostContext.Configuration.GetSection(nameof(GeneralSettings)))
                .ValidateConfiguration();
            services.AddOptions<List<PwmSettings>>()
                .Bind(hostContext.Configuration.GetSection(nameof(PwmSettings)))
                .ValidateConfiguration();

            services.AddTransient<IPwmManagerService, PwmManagerService>();
            services.AddTransient<IHddTempService, HddTempService>();
            services.AddScoped<IHddFancontrolApplication, HddFancontrolApplication>();

            services.AddHostedService<Startup>();
        });
}