using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLoggerColorConsole;
using ZLogger;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;

namespace SampleConsoleApp;

internal class Program
{
    static ILogger logger;

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

        logger = AppHost.Services.GetService<ILogger<Program>>();
        logger.ZLogInformation($"Start");

        //var options = WindowOptions.DefaultVulkan with
        //{
        //    Size = new Vector2D<int>(800, 600),
        //};
        //var window = Window.Create(options);
        //window.Initialize();
        //var input = window.CreateInput();

        //for (int j = 0; j < input.Keyboards.Count; j++)
        //    input.Keyboards[j].KeyDown += KeyDown;

        //while (!window.IsClosing)
        //    window.DoEvents();


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

    static void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        Console.WriteLine(keyboard.Name);
        Console.WriteLine(key);
        Console.WriteLine(arg3);


        //logger.ZLogInformation($"keyboard {keyboard} key {key} {arg3}");
        logger.ZLogInformation($"k:{keyboard}");
        //logger.LogInformation("keyboard {keyboard} key {key} {arg3}", keyboard, key, arg3);
    }
}
