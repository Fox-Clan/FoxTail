using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;

namespace FoxTail.Chat.Commands;

public class ShutdownEngineCommand : IChatCommand
{
    public string Name => "shutdown";
    public string HelpText => "Performs a full shutdown of the engine";
    public bool RequirePermission => true; // duh.
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        await channel.SendMessageAsync("Shutting down!");
        context.Runner.Exit();
    }
}