using System.Diagnostics;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using JvyHeadlessRunner;
using JvyHeadlessRunner.EngineIntegration;
using JvyHeadlessRunner.EngineIntegration.LoadManagement.Tasks;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using SkyFrost.Base;

internal static class Program
{
    public static Logger Logger;
    public static HeadlessRunner Runner;
    public static Harmony Harmony;
    
    public static async Task Main(string[] args)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("jvyden headless runner");
        Console.ForegroundColor = oldColor;

        Logger = new(new LoggerConfiguration
        {
            Behaviour = new QueueLoggingBehaviour(),
            #if DEBUG
            MaxLevel = LogLevel.Trace,
            #else
            MaxLevel = LogLevel.Info
            #endif
        });
        
        Logger.LogInfo(ResoCategory.Harmony, "Initializing Harmony...");
        Harmony = new Harmony(nameof(JvyHeadlessRunner));
        Harmony.DEBUG = true;
        Stopwatch sw = Stopwatch.StartNew();
        Harmony.PatchAll();
        Logger.LogInfo(ResoCategory.Harmony, $"Harmony patches took {sw.ElapsedMilliseconds}ms.");
        
        UniLog.OnLog += s =>
        {
            // HACK: capture bootstrap completion to avoid complicated harmony patch
            // ultimately, the harmony patch is probably the better solution but this works
            if (WaitForBootstrapTask.Instance != null && !WaitForBootstrapTask.Instance.Bootstrapped && s == "BOOTSTRAP: Bootstrap complete")
                WaitForBootstrapTask.Instance.Bootstrapped = true;

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
        
        await Runner.StartFullInitTasksAsync();

        await Task.Delay(-1);
    }
}