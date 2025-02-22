using StargateNetwork;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class InitializeStargateIntegrationTask : InitTask
{
    public override string Name => "Start Stargate Server";
    public override InitTaskStage Stage => InitTaskStage.Immediate;
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        if (!context.StargateConfig.StargateServerIntegration)
        {
            context.Logger.LogInfo("Stargate", "Stargate integration disabled, skipping init.");
            return;
        }

        StargateServer server = new(context.Logger, context.StargateConfig);
        server.Start();

        context.StargateServer = server;
    }
}