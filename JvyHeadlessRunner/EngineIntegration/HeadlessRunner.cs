using System.Diagnostics;
using FrooxEngine;
using JvyHeadlessRunner.EngineIntegration.LoadManagement;
using NotEnoughLogs;

namespace JvyHeadlessRunner.EngineIntegration;

public class HeadlessRunner
{
    private readonly HeadlessContext _context;
    private readonly IEngineInitProgress _progress;

    private float _tickRate = 1.0f;
    
    public HeadlessRunner(HeadlessContext context)
    {
        this._context = context;
        this._context.Logger.LogInfo(ResoCategory.Runner, "Creating HeadlessRunner");
        
        ResoniteDllResolver.Initialize();
        
        context.Engine = new Engine();
        context.SystemInfo = new StandaloneSystemInfo();
        this._progress = new LoggerEngineInitProgress(context);
        this._progress.SetFixedPhase("Waiting for Initialization");
        
        if(!Stopwatch.IsHighResolution)
            this._context.Logger.LogWarning(ResoCategory.Runner, "Stopwatch does not support high resolutions on this platform." +
                                                                 "Tick timings will be imprecise/jittery.");
    }

    public async Task InitializeEngineAsync()
    {
        this._context.Logger.LogInfo(ResoCategory.Runner, "Initializing FrooxEngine...");

        string path = Environment.CurrentDirectory;

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
            DisablePlatformInterfaces = true
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
        Stopwatch sw = new();
        sw.Start();
        
        this._dspStartTime = DateTime.UtcNow;

        while (true)
        {
            try
            {
                EngineTick();
            }
            catch (Exception e)
            {
                this._context.Logger.LogError(ResoCategory.Runner, "Error in engine update: {0}", e);
            }

            float requiredTicks = ((1000f / this._tickRate) * TimeSpan.TicksPerMillisecond);
            sw.Restart();
            SpinWait.SpinUntil(() => sw.ElapsedTicks >= requiredTicks);
        }
    }

    private void EngineTick()
    {
        this._context.Engine.RunUpdateLoop();
        this._context.SystemInfo.FrameFinished();
        
        float dspTickRate = 0f;
        if (this._tickRate > 0f)
        {
            dspTickRate = 1f / this._tickRate;
        }

        this._dspTime += dspTickRate * 48000f;
        if (this._dspTime >= 1024.0f)
        {
            this._dspTime = (this._dspTime - 1024.0f) % 1024.0f;
            DummyAudioConnector.UpdateCallback((DateTimeOffset.UtcNow - this._dspStartTime).TotalMilliseconds * 1000);
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
}