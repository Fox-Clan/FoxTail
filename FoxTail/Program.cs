using System.Diagnostics;
using Elements.Core;
using FoxTail.Configuration;
using FoxTail.EngineIntegration;
using FoxTail.EngineIntegration.LoadManagement.Tasks;
using HarmonyLib;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

namespace FoxTail;

internal static class Program
{
    public static HeadlessRunner Runner;

    public static HeadlessContext Context;
    
    public static async Task Main()
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("FoxTail for Resonite");
        Console.ForegroundColor = oldColor;
        
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        Context = new HeadlessContext();
        Context.Logger = new Logger(new LoggerConfiguration
        {
            Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
            MaxLevel = LogLevel.Trace,
#else
            MaxLevel = LogLevel.Debug
#endif
        });
        
        Context.Logger.LogInfo(ResoCategory.Config, "Loading configurations...");
        Context.Config = ConfigHelper.GetOrCreateConfig<FoxTailConfig>(Context, "foxtail.json");
        
        Context.Logger.LogInfo(ResoCategory.Harmony, "Initializing Harmony...");
        HeadlessContext.Harmony = new Harmony(nameof(FoxTail));
        Harmony.DEBUG = true;
        Stopwatch sw = Stopwatch.StartNew();
        HeadlessContext.Harmony.PatchAll();
        Context.Logger.LogInfo(ResoCategory.Harmony, $"Harmony patches took {sw.ElapsedMilliseconds}ms.");
        
        UniLog.OnLog += s =>
        {
            // HACK: capture bootstrap completion to avoid complicated harmony patch
            // ultimately, the harmony patch is probably the better solution but this works
            if (WaitForBootstrapTask.Instance != null && !WaitForBootstrapTask.Instance.Bootstrapped && s == "BOOTSTRAP: Bootstrap complete")
                WaitForBootstrapTask.Instance.Bootstrapped = true;

            Context.Logger.LogDebug(ResoCategory.FrooxEngine, s);
        };

        UniLog.OnError += s =>
        {
            Context.Logger.LogError(ResoCategory.FrooxEngine, s);
        };

        UniLog.OnWarning += s =>
        {
            Context.Logger.LogWarning(ResoCategory.FrooxEngine, s);
        };

        Runner = new HeadlessRunner(Context);
        await Runner.InitializeEngineAsync();
        
        await Runner.StartFullInitTasksAsync();

        await Task.Delay(-1);
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("UNHANDLED EXCEPTION!!!");
        Console.WriteLine("UNHANDLED EXCEPTION!!!");
        Console.WriteLine("UNHANDLED EXCEPTION!!!");
        
        Console.WriteLine("CAUGHT: " + e.ExceptionObject);
        Console.WriteLine("TERMINATING: " + e.IsTerminating);
        
        Console.Out.Flush();
        try
        {
            Context.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("CONTEXT DISPOSE FAILED:");
            Console.WriteLine(ex);
        }
    }
}