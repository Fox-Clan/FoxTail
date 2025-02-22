using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Newtonsoft.Json;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

public class BunKum
{
    public static async void StartBunKum()
    {
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
        server.Start();
        await Task.Delay(-1);
    }
}

