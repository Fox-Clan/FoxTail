using FoxTail.Worlds;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class InitializeWorldManagerTask : InitTask
{
    public override string Name => "Initialize World Manager";
    public override InitTaskStage Stage => InitTaskStage.Authenticated;
    public override Task ExecuteAsync(HeadlessContext context)
    {
        FoxWorldManager manager = new(context);
        context.WorldManager = manager;

        return Task.CompletedTask;
    }
}