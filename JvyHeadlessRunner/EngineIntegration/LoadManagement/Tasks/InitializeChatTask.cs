using JvyHeadlessRunner.Chat;
using JvyHeadlessRunner.Chat.Resonite;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement.Tasks;

public class InitializeChatTask : InitTask
{
    public override string Name => "Initialize Chatbot Clients";
    public override InitTaskStage Stage => InitTaskStage.Authenticated;
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        context.Logger.LogDebug(ResoCategory.Chat, "Initializing chat...");
        ChatCommandHelper chat = new(context);

        chat.AddPlatform(new ResoniteChatPlatform(context));

        context.CommandHelper = chat;
    }
}