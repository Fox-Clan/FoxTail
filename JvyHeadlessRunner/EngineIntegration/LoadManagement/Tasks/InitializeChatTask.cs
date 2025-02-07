using JvyHeadlessRunner.Chat;
using JvyHeadlessRunner.Chat.Discord;
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
        
        context.Logger.LogDebug(ResoCategory.Chat, "Initializing Discord integrations...");
        string? token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (token == null)
        {
            context.Logger.LogWarning(ResoCategory.Chat, "Discord token was not found, skipping bot setup. You can set this with the DISCORD_TOKEN environment variable.");
        }
        else
        {
            DiscordChatPlatform discord = new(context, token);
            await discord.InitializeAsync();
            
            chat.AddPlatform(discord);
        }

        context.CommandHelper = chat;
    }
}