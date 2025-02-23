using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;
using FrooxEngine;

namespace FoxTail.Chat.Commands;

public class ChangeRoleCommand : IChatCommand
{
    public string Name => "role";
    public string HelpText => "Sets your role in the world, e.g. !role builder";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        ManagedWorld? world = args.WorldByIdOrUserFocused(context, user);
        if (world == null)
        {
            await channel.SendMessageAsync("I couldn't find the world you were in, so I can't set your role. Try joining/focusing the world.");
            return;
        }
                    
        string? roleName = args.GetArg("role");
        if (roleName == null)
        {
            await channel.SendMessageAsync("I need the role to set you to. For example, you can do \"!role admin\".");
            return;
        }
                    
        PermissionSet? role = world.World.Permissions.Roles.
            FirstOrDefault(r => r.RoleName.Value.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));

        if (role == null)
        {
            await channel.SendMessageAsync("That world doesn't have that role. Did you make a typo?");
            return;
        }
                    
        User worldUser = world.World.GetUserByUserId(user.UserId);

        world.World.RunSynchronously(() =>
        {
            worldUser.Role = role;
            worldUser.World.Permissions.AssignDefaultRole(worldUser, role);
        });
    }
}