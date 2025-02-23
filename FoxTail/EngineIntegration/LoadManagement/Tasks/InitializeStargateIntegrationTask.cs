using FoxTail.Common;
using StargateNetwork;
using StargateNetwork.Worlds.Dummy;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class InitializeStargateIntegrationTask : InitTask
{
    public override string Name => "Start Stargate Server";
    public override InitTaskStage Stage => InitTaskStage.Immediate;
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        if (!context.StargateConfig.StargateServerIntegration)
        {
            context.Logger.LogInfo(ResoCategory.Stargate, "Stargate integration disabled, skipping init.");
            return;
        }

        StargateServer server = new(context.Logger, context.StargateConfig, new DummyStargateWorldManager());
        server.Start();

        context.StargateServer = server;
    }
}