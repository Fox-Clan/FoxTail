using System.Diagnostics;
using Elements.Core;
using FoxTail.Common;
using FoxTail.Common.Timing;
using FoxTail.Configuration;
using FoxTail.EngineIntegration.LoadManagement;
using FrooxEngine;

namespace FoxTail.EngineIntegration;

public class HeadlessRunner
{
    private readonly HeadlessContext _context;
    private readonly LoggerEngineInitProgress _progress;

    private readonly ThrottledClock _clock;

    private readonly string[] _requiredDirectories =
        [
            "Data",
            "Cache",
            "Logs",
            "RuntimeData",
            "Stargate"
        ];
    
    public bool ExitComplete { get; private set; }
    private bool _exitRequested = false;

    private float TickRate => _context.Config.TickRate;
    
    public HeadlessRunner(HeadlessContext context)
    {
        this._context = context;
        this._context.Logger.LogInfo(ResoCategory.Runner, "Creating HeadlessRunner");
        
        ResoniteDllResolver.Initialize();
        
        this._clock = new ThrottledClock();
        
        context.Engine = new Engine();
        context.SystemInfo = new FoxSystemInfo(context, this._clock);
        this._progress = new LoggerEngineInitProgress(context);
        this._progress.SetFixedPhase("Waiting for Initialization");
        
        if(!Stopwatch.IsHighResolution)
            this._context.Logger.LogWarning(ResoCategory.Runner, "Stopwatch does not support high resolutions on this platform." +
                                                                 "Tick timings will be imprecise/jittery.");
        
        this._context.Logger.LogTrace(ResoCategory.Runner, $"Stopwatch precision: {Stopwatch.Frequency}hz");

        context.Engine.EnvironmentShutdownCallback = () =>
        {
            context.Logger.LogInfo(ResoCategory.Runner, "Environment has shutdown. Exit is complete!");
            this.ExitComplete = true;
        };
    }

    public async Task InitializeEngineAsync()
    {
        this._context.Logger.LogInfo(ResoCategory.Runner, "Initializing FrooxEngine...");

        string path = Environment.CurrentDirectory;
        
        foreach (string dir in this._requiredDirectories)
        {
            string dirPath = Path.Combine(path, dir);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        LaunchOptions options = new()
        {
            DataDirectory = Path.Combine(path, "Data"),
            CacheDirectory = Path.Combine(path, "Cache"),
            LogsDirectory = Path.Combine(path, "Logs"),
            OutputDevice = HeadOutputDevice.Headless,
            #if DEBUG
            VerboseInit = true,
            #else
            VerboseInit = false,
            #endif
            DoNotAutoLoadHome = true,
            DisablePlatformInterfaces = true,
        };
        
        Stopwatch sw = Stopwatch.StartNew(); 
        await _context.Engine.Initialize(path, options, _context.SystemInfo, null, this._progress);
        this._context.Logger.LogInfo(ResoCategory.Runner, $"Engine initialized after {sw.ElapsedMilliseconds}ms.");
        this._context.Logger.LogInfo(ResoCategory.Runner, "Starting userspace...");
        sw.Restart();

        World userspace = Userspace.SetupUserspace(_context.Engine);
        this._context.Logger.LogInfo(ResoCategory.Runner, $"Userspace set up after {sw.ElapsedMilliseconds}ms, starting engine loop.");
        
        StartEngineThread();
        
        // wait for world to init
        await userspace.Coroutines.StartTask(static async () => await default(ToWorld));
        
        // wait a couple frames
        for (int i = 0; i < 5; i++)
        {
            await userspace.Coroutines.StartTask(static async () => await default(NextUpdate));
        }
        
        this._context.Logger.LogInfo(ResoCategory.Runner, "Allowing hosts... please review this list carefully to ensure you aren't exposing any security holes.");
        await _context.Engine.GlobalCoroutineManager.StartTask(async c =>
        {
            await new NextUpdate();
            SecurityManager security = c.Engine.Security;
            FoxTailConfig config = c.Config;
            
            AllowHosts(c, config.AllowedHttpHosts, security.TemporarilyAllowHTTP);
            AllowHosts(c, config.AllowedWebsocketHosts, security.TemporarilyAllowWebsocket);
            AllowHosts(c, config.AllowedOSCSenderHosts, security.TemporarilyAllowOSC_Sender);
            
            foreach (int port in config.AllowedOSCReceiverPorts)
            {
                security.TemporarilyAllowOSC_Receiver(port);
            }
        }, _context);
        this._context.Logger.LogInfo(ResoCategory.Runner, "Allowed all hosts!");
    }

    private static void AllowHosts(HeadlessContext c, IEnumerable<string> list, Action<string> allowFunc)
    {
        foreach (string host in list)
        {
            if (!Uri.TryCreate(host, UriKind.Absolute, out Uri? uri))
            {
                c.Logger.LogWarning(ResoCategory.Runner, $"Couldn't allow host {host} as it's an invalid URL");
                continue;
            }

            c.Logger.LogInfo(ResoCategory.Runner, $"ALLOWING HOST {host}!");
            allowFunc(uri.Host);
        }
    }
    
    private static void AllowHosts(HeadlessContext c, IEnumerable<string> list, Action<string, int> allowFunc)
    {
        foreach (string host in list)
        {
            if (!Uri.TryCreate(host, UriKind.Absolute, out Uri? uri))
            {
                c.Logger.LogWarning(ResoCategory.Runner, $"Couldn't allow host {host} as it's an invalid URL");
                continue;
            }

            c.Logger.LogInfo(ResoCategory.Runner, $"ALLOWING HOST {host}!");
            allowFunc(uri.Host, uri.Port);
        }
    }
    
    private DateTime _dspStartTime = DateTime.UtcNow;
    private float _dspTime = 0f;

    private void StartEngineThread()
    {
        Thread thread = new(EnterEngineLoop)
        {
            Name = "FrooxEngine Loop",
            Priority = ThreadPriority.Normal,
            IsBackground = false,
        };
        thread.Start();
    }

    private void EnterEngineLoop()
    {
        this._dspStartTime = DateTime.UtcNow;

        while (!ExitComplete)
        {
            try
            {
                EngineTick();
            }
            catch (Exception e)
            {
                this._context.Logger.LogError(ResoCategory.Runner, "Error in engine update: {0}", e);
                bool shouldCrash = this._context.SystemInfo.HandleException();

                if (shouldCrash)
                {
                    this._context.Logger.LogError(ResoCategory.Runner, "Too many exceptions in a short period of time, crashing.");
                    this.Exit();
                    throw;
                }
            }

            this._clock.Wait();
        }
        
        this._context.Logger.LogInfo(ResoCategory.Runner, "Engine thread exiting!");
    }

    private void EngineTick()
    {
        this._context.Engine.RunUpdateLoop();
        this._context.SystemInfo.FrameFinished();
        
        float dspTickRate = 0f;
        if (this.TickRate > 0f)
        {
            dspTickRate = 1f / this.TickRate;
        }

        this._dspTime += dspTickRate * 48000f;
        if (this._dspTime >= 1024.0f)
        {
            this._dspTime = (this._dspTime - 1024.0f) % 1024.0f;
            DummyAudioConnector.UpdateCallback((DateTimeOffset.UtcNow - this._dspStartTime).TotalMilliseconds * 1000);
        }

        if (this._exitRequested)
        {
            Userspace.ExitApp(false);
        }
    }

    public async Task StartFullInitTasksAsync()
    {
        this._progress.SetFixedPhase("Full Init Tasks");
        InitTaskManager theLoader = new(this._context, this._progress);

        await theLoader.DoAllTasksAsync();
        this._context.Logger.LogInfo(ResoCategory.EngineInit, "Full init tasks complete!");
        this._progress.SetFixedPhase("Awaiting EngineReady");
    }

    public bool Exit()
    {
        if (this._exitRequested)
            return false;
        
        this._context.Logger.LogDebug(ResoCategory.Runner, "Signalling exit request...");
        this._exitRequested = true;
        return true;
    }

    public async Task WaitForEngineExitAsync()
    {
        while (!this.ExitComplete)
        {
            await Task.Delay(1000);
        }
    }
    
    public void WaitForEngineExit()
    {
        while (!this.ExitComplete)
        {
            Thread.Sleep(25);
        }
    }
}