using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;

namespace FoxTail.Chat.Commands;

public class SaveWorldCommand : IChatCommand
{
    public string Name => "save";
    public string HelpText => "Saves the world you're currently focused onto";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        ManagedWorld? world = args.WorldByIdOrUserFocused(context, user);
        if (world == null)
        {
            await channel.SendMessageAsync("I couldn't find the world you were in, so I can't save it. Try giving me the ID of the world from !worlds.");
            return;
        }
                    
        await channel.SendMessageAsync("Saving world...");

        if (await context.WorldManager.SaveWorld(world))
        {
            await channel.SendMessageAsync("World saved and overwritten.");
        }
        else
        {
            await channel.SendMessageAsync("I can't save that world as I don't own that world. You can use 'Save As...' under Session to save it yourself.");
        }
    }
}