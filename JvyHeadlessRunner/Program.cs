using Elements.Core;
using FrooxEngine;
using JvyHeadlessRunner;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using SkyFrost.Base;

internal static class Program
{
    public static Logger Logger;
    public static HeadlessRunner Runner;
    
    public static async Task Main(string[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("jvyden headless runner");
        Console.ForegroundColor = oldColor;

        Logger = new(new LoggerConfiguration
        {
            Behaviour = new QueueLoggingBehaviour(),
            MaxLevel = LogLevel.Trace,
        });
        
        UniLog.OnLog += s =>
        {
            Logger.LogDebug(ResoCategory.FrooxEngine, s);
        };

        UniLog.OnError += s =>
        {
            Logger.LogError(ResoCategory.FrooxEngine, s);
        };

        UniLog.OnWarning += s =>
        {
            Logger.LogWarning(ResoCategory.FrooxEngine, s);
        };

        Runner = new HeadlessRunner(Logger);
        await Runner.InitializeEngineAsync();

        await Task.Delay(-1);
    }
}