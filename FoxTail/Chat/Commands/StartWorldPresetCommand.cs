using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;
using FrooxEngine;

namespace FoxTail.Chat.Commands;

public abstract class StartWorldPresetCommand : IChatCommand
{
    protected abstract WorldAction Action { get; } 
    public abstract string Name { get; }
    public string HelpText => $"Starts a {this.Name} world and invites you";
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        await channel.SendMessageAsync($"Starting a {this.Name} for you, expect an invite shortly!");
        ManagedWorld world = await context.WorldManager.StartWorld(this.Action, user);
        await world.InviteAndPromoteOwner(channel);
    }
}