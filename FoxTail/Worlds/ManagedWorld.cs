using FoxTail.Chat.Platforms;
using FoxTail.Common;
using FrooxEngine;
using SkyFrost.Base;

namespace FoxTail.Worlds;

public class ManagedWorld
{
    public readonly World World;
    public readonly IChatUser? Owner;
    public readonly uint Id;

    public string Name => World.Name;

    internal ManagedWorld(World world, IChatUser? owner, uint id)
    {
        World = world;
        Owner = owner;
        Id = id;
    }

    internal async Task InviteUsersAsync(HeadlessContext context, FoxWorldStartSettings settings)
    {
        foreach (string inviteUsername in settings.InviteUsernames)
        {
            context.Logger.LogInfo(ResoCategory.WorldInit, $"Inviting {inviteUsername} to {this.World.Name}");
            SkyFrost.Base.User user = (await context.Engine.Cloud.Users.GetUserByName(inviteUsername)).Entity;
            if (user == null)
            {
                context.Logger.LogWarning(ResoCategory.WorldInit,
                    $"Couldn't find user by name {inviteUsername}, can't invite");
                continue;
            }

            this.World.AllowUserToJoin(user.Id);

            UserMessages? userMessages = context.Engine.Cloud.Messages.GetUserMessages(user.Id);

            if (settings.InviteMessage != null)
            {
                context.Logger.LogTrace(ResoCategory.WorldInit, "Sending TextMessage...");
                await userMessages.SendTextMessage(settings.InviteMessage);
            }

            context.Logger.LogTrace(ResoCategory.WorldInit, "Sending InviteMessage...");
            await userMessages.SendInviteMessage(this.World.GenerateSessionInfo());

            context.Logger.LogTrace(ResoCategory.WorldInit, $"Successfully invited {inviteUsername} to {this.World.Name}");
        }
    }

    public async Task InviteAndPromoteOwner(IChatChannel channel)
    {
        if (this.Owner == null)
            throw new InvalidOperationException("This world does not have an owner.");

        await InviteUser(channel, this.Owner);
        await this.World.Coroutines.StartTask(async () =>
        {
            while (!this.World.Permissions.PermissionHandlingInitialized)
                await new NextUpdate();

            this.World.RunSynchronously(() =>
            {
                this.World.Permissions.DefaultUserPermissions[this.Owner.UserId] = this.World.Permissions.HighestRole;
            });
        });
    }

    public async Task InviteUser(IChatChannel channel, IChatUser user)
    {
        await this.World.Coroutines.StartTask(async () =>
        {
            this.World.AllowUserToJoin(user.UserId);
            while (!this.World.SessionURLs.Any())
                await new NextUpdate();
            
            _ = Task.Run(async () => await channel.Platform.SendInviteAsync(channel, this.World));
        });
    }

    public bool IsVisibleForUser(IChatUser user)
    {
        if (user.UserId == this.Owner?.UserId)
            return true;
        
        if (this.World.AccessLevel >= SessionAccessLevel.RegisteredUsers)
            return true;
        
        if (this.World.AllUsers.Any(u => u.UserID == user.UserId))
            return true;

        return false;
    }

    public override string ToString()
    {
        return $"ManagedWorld {Id}: '{Name}' (owned by: {Owner?.Username ?? "nobody"})";
    }

    public override int GetHashCode()
    {
        return (int)this.Id;
    }
}