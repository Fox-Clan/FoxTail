using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Elements.Core;
using FoxTail.Common;
using FoxTail.Common.Configuration;
using FoxTail.Configuration;
using FoxTail.EngineIntegration;
using FoxTail.EngineIntegration.LoadManagement.Tasks;
using HarmonyLib;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using NotEnoughLogs.Sinks;
using StargateNetwork;

namespace FoxTail;

internal static class Program
{
    private static HeadlessRunner _runner = null!;
    public static HeadlessContext Context = null!;
    
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static async Task Main()
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("FoxTail for Resonite");
        Console.ForegroundColor = oldColor;
        
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        Context = new HeadlessContext();
        Context.Logger = new Logger([new ConsoleSink(), new FileLoggerSink()], new LoggerConfiguration
        {
            Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
            MaxLevel = LogLevel.Trace,
#else
            MaxLevel = LogLevel.Debug
#endif
        });
        
        Context.Logger.LogInfo(ResoCategory.Config, "Loading configurations...");
        bool createdNewConfig = false;
        Context.Config = ConfigHelper.GetOrCreateConfig<FoxTailConfig>(Context.Logger, "foxtail.json", ref createdNewConfig);
        Context.WorldConfig = ConfigHelper.GetOrCreateConfig<WorldConfig>(Context.Logger, "worlds.json", ref createdNewConfig);
        Context.UserConfig = ConfigHelper.GetOrCreateConfig<UserConfig>(Context.Logger, "users.json", ref createdNewConfig);
        Context.StargateConfig = ConfigHelper.GetOrCreateConfig<StargateConfiguration>(Context.Logger, "stargate.json", ref createdNewConfig);

        if (createdNewConfig)
        {
            Context.Logger.LogWarning(ResoCategory.Config, "It is highly advisable that you stop the server now and modify the above configurations.");
            Context.Logger.LogWarning(ResoCategory.Config, "If not, startup will continue in 10 seconds.");
            Thread.Sleep(10_000);
            Context.Logger.LogWarning(ResoCategory.Config, "Continuing startup without config change!");
        }
        
        Context.Logger.LogInfo(ResoCategory.Harmony, "Initializing Harmony...");
        HeadlessContext.Harmony = new Harmony(nameof(FoxTail));
        // Harmony.DEBUG = true;
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

        _runner = new HeadlessRunner(Context);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => OnProgramExiting();
        Console.CancelKeyPress += (_, _) => OnProgramExiting();

        Context.Runner = _runner;
        
        await _runner.InitializeEngineAsync();
        await _runner.StartFullInitTasksAsync(); // engine is mostly up, setup foxtail stuff
        
        // since loading is complete, perform a big gc to clear out all the loading stuff
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

        // this blocks the thread until the engine exits.
        await _runner.WaitForEngineExitAsync();
        
        Context.Dispose();
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

    private static void OnProgramExiting()
    {
        Context.Logger.LogInfo(ResoCategory.Runner, "Exit has been requested by the host, beginning shutdown process.");
        _runner.Exit();
        _runner.WaitForEngineExit();
        Context.Dispose();
    }
}