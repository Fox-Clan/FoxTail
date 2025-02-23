using FoxTail.LoveAuth;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class InitializeFoxLoveAuthTask : InitTask
{
    public override string Name => "Initialize LoveAuth Integration";
    public override InitTaskStage Stage => InitTaskStage.Immediate;
    public override Task ExecuteAsync(HeadlessContext context)
    {
        FoxLoveAuth.SetupBunkum();
        return Task.CompletedTask;
    }
}