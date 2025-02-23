using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;

namespace FoxTail.Chat.Commands;

public class CloseWorldCommand : IChatCommand
{
    public string Name => "close";
    public string HelpText => "Closes the world you're currently focused onto";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        ManagedWorld? world = args.WorldByIdOrUserFocused(context, user);
        if (world == null)
        {
            await channel.SendMessageAsync("I couldn't find the world you were in, so I can't close it. Try giving me the ID of the world from !worlds.");
            return;
        }
        
        await context.WorldManager.CloseWorld(world);
        await channel.SendMessageAsync($"Closed {world.Name}.");
    }
}