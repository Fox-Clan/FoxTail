using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Http;
using FoxTail.LoveAuth.Services;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

BunkumServer server = new BunkumHttpServer(new LoggerConfiguration()
{
    Behaviour = new QueueLoggingBehaviour(),
    MaxLevel = LogLevel.Trace
});

server.Initialize = s =>
{
    s.AddService<LoveAuthService>();
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);