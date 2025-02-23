using FoxTail.Common;
using FoxTail.Worlds;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class StartupWorldsTask : InitTask
{
    public override string Name => "Default World Startup";
    public override InitTaskStage Stage => InitTaskStage.WorldsInitialized;
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        foreach (FoxWorldStartSettings worldConfig in context.WorldConfig.CompileAutoLoadWorlds())
        {
            await context.WorldManager.StartWorld(worldConfig);
        }
        
        context.Logger.LogInfo(ResoCategory.WorldInit, "All worlds successfully started!");
    }
}