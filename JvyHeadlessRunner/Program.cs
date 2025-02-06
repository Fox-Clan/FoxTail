using Elements.Core;
using FrooxEngine;
using JvyHeadlessRunner;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;


ConsoleColor oldColor = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("jvyden headless runner");
Console.ForegroundColor = oldColor;

Logger logger = new(new LoggerConfiguration
{
    Behaviour = new QueueLoggingBehaviour(),
    MaxLevel = LogLevel.Trace,
});

ResoniteDllResolver.Initialize();

UniLog.OnLog += s =>
{
    logger.LogInfo(ResoCategory.FrooxEngine, s);
};

UniLog.OnError += s =>
{
    logger.LogError(ResoCategory.FrooxEngine, s);
};

UniLog.OnWarning += s =>
{
    logger.LogWarning(ResoCategory.FrooxEngine, s);
};

Console.WriteLine("Initializing FrooxEngine...");
StandaloneFrooxEngineRunner runner = new();
await runner.Initialize(new LaunchOptions
{
    VerboseInit = true,
    OutputDevice = HeadOutputDevice.Headless,
    DisablePlatformInterfaces = true,
});

Console.WriteLine("FrooxEngine is up!");

await Task.Delay(-1);