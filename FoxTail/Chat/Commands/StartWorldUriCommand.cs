using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;

namespace FoxTail.Chat.Commands;

public class StartWorldUriCommand : IChatCommand
{
    public string Name => "start";
    public string HelpText => "Starts a particular world either by an alias or a record url and invites you";
    public bool RequirePermission => true;

    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        Uri? url = args.GetRecordUrl("url", context);
        if (url == null)
        {
            await channel.SendMessageAsync("I couldn't find that world alias. Try providing the record URL instead.");
            return;
        }
                    
        await channel.SendMessageAsync("Starting that world for you, expect an invite shortly!");
        ManagedWorld world = await context.WorldManager.StartWorld(url, user);
        await world.InviteAndPromoteOwner(channel);
    }
}