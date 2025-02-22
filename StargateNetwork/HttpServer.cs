using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

namespace StargateNetwork;

public class StargateBunkumServer
{
    private readonly BunkumServer _server;

    public StargateBunkumServer()
    {
        Environment.SetEnvironmentVariable("BUNKUM_DATA_FOLDER", "Stargate");
        
        BunkumServer server = new BunkumHttpServer(new LoggerConfiguration
        { 
            Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
            MaxLevel = LogLevel.Trace,
#else
            MaxLevel = LogLevel.Info,
#endif
        });

        server.Initialize = s =>
        {
            s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
        };

        this._server = server;
    }
    
    public void Start()
    {
        this._server.Start();
    }

    public void Stop()
    {
        this._server.Stop();
    }
}