using FoxTail.Chat;
using FoxTail.Chat.Console;
using FoxTail.Chat.Discord;
using FoxTail.Chat.Resonite;
using FoxTail.Common;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class InitializeChatTask : InitTask
{
    public override string Name => "Initialize Chatbot Clients";
    public override InitTaskStage Stage => InitTaskStage.Authenticated;
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        context.Logger.LogDebug(ResoCategory.Chat, "Initializing chat...");
        ChatCommandHelper chat = new(context);
        
        context.CommandHelper = chat;
        
        // always initialize console chat.
        chat.AddPlatform(new ConsoleChatPlatform(context));

        if (context.Config.DisableChatInitialization)
        {
            context.Logger.LogWarning(ResoCategory.Chat, "Skipping chat initialization as the option for it was enabled in the configuration. " +
                                                         "Chat commands will NOT work (except under console).");
            return;
        }

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
    }
}