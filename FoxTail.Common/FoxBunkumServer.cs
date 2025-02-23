using Bunkum.Core;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

namespace FoxTail.Common;

public static class FoxBunkumServer
{
    private static BunkumServer? _server;
    private static bool _initialized;

    private static readonly List<Action<BunkumServer>> SetupActions = [];

    public static void RegisterSetupAction(Action<BunkumServer> action)
    {
        if (_initialized)
            throw new InvalidOperationException("Already initialized.");
        SetupActions.Add(action);
    }

    public static void Initialize()
    {
        if (_initialized)
            throw new InvalidOperationException("Already initialized.");
        
        _initialized = true;
        
        if(Environment.GetEnvironmentVariable("BUNKUM_DATA_FOLDER") == null)
            Environment.SetEnvironmentVariable("BUNKUM_DATA_FOLDER", Path.Combine(Environment.CurrentDirectory, "Bunkum"));

        _server = new BunkumHttpServer(new LoggerConfiguration
        {
            Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
            MaxLevel = LogLevel.Trace,
#else
            MaxLevel = LogLevel.Debug
#endif
        });
        _server.Initialize = s =>
        {
            foreach (Action<BunkumServer> action in SetupActions)
            {
                action(s);
            }
        };
    }

    public static void Start()
    {
        if (!_initialized)
            Initialize();
        
        _server!.Start();
    }

    public static void Stop()
    {
        _server?.Stop();
    }
}