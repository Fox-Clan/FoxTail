using FoxTail.Common;
using FoxTail.Worlds.Stargate;
using StargateNetwork;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class InitializeStargateIntegrationTask : InitTask
{
    public override string Name => "Start Stargate Server";
    public override InitTaskStage Stage => InitTaskStage.Immediate;
    public override Task ExecuteAsync(HeadlessContext context)
    {
        if (!context.StargateConfig.StargateServerIntegration)
        {
            context.Logger.LogInfo(ResoCategory.Stargate, "Stargate integration disabled, skipping init.");
            return Task.CompletedTask;
        }

        StargateServer server = new(context.Logger, context.StargateConfig, new FoxStargateWorldManager(context));
        server.Start();

        context.StargateServer = server;
        return Task.CompletedTask;
    }
}