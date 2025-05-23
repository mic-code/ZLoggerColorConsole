using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLoggerColorConsole;
using ZLogger;

namespace SampleConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var AppHost = Host.CreateDefaultBuilder()
        .ConfigureLogging(ColorConsole.ConfigOption)
        .UseContentRoot(AppContext.BaseDirectory)
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton(typeof(ILogger<>), typeof(LoggerEx<>));
        })
        .Build();


        var logger = AppHost.Services.GetService<ILogger<Program>>();
        logger.ZLogInformation($"Start");

        var i = 123;
        var t = true;
        var f = false;
        var d = 0.9999999;
        var s = "hihilol";
        object n = null;
        logger.ZLogTrace($"true  {t}");
        logger.ZLogDebug($"false {f}");
        logger.ZLogWarning($"hihi {i}");
        logger.ZLogError($"double {d:N2}");
        logger.ZLogError($"null {n}");
        logger.ZLogCritical($"string is {s}");

        logger.ZLogInformation($"End");
    }
}
