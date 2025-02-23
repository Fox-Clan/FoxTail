using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;

namespace FoxTail.Chat.Commands;

public class InviteCommand : IChatCommand
{
    public string Name => "invite";
    public string HelpText => "Invites you to a given world";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        ManagedWorld? world = args.WorldByIdOrUserFocused(context, user);
        if (world == null)
        {
            await channel.SendMessageAsync("I couldn't find the world specified, so I can't invite you. Try giving me the ID of the world from !worlds.");
            return;
        }
        
        await world.InviteUser(channel, user);
    }
}