using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using StargateNetwork.Worlds.Dummy;

namespace StargateNetwork;

internal static class Program
{
    private static void Main()
    {
        if(!Directory.Exists("Stargate"))
            Directory.CreateDirectory("Stargate");
        
        //get env vars
        string? wsUri = Environment.GetEnvironmentVariable("WS_URI");
        if (string.IsNullOrEmpty(wsUri))
        {
            wsUri = "ws://192.168.1.14:27015";
        }

        StargateConfiguration config = new();
        config.WebsocketHostUrl = wsUri;
        config.WebsocketEnabled = true;
        config.BunkumEnabled = true;

        Logger logger = new(new LoggerConfiguration()
        {
            Behaviour = new DirectLoggingBehaviour(),
            MaxLevel = LogLevel.Trace,
        });

        StargateServer server = new(logger, config, new DummyStargateWorldManager());
        server.Start();

        Console.ReadKey();
        server.Stop();
    }
}