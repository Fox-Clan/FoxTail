using FoxTail.Common;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class StartBunkumTask : InitTask
{
    public override string Name => "Startup Bunkum Server";
    public override InitTaskStage Stage => InitTaskStage.BunkumInit;
    public override Task ExecuteAsync(HeadlessContext context)
    {
        FoxBunkumServer.Initialize();
        FoxBunkumServer.Start();
        return Task.CompletedTask;
    }
}