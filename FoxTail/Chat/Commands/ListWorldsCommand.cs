using System.Text;
using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Worlds;

namespace FoxTail.Chat.Commands;

public class ListWorldsCommand : IChatCommand
{
    public string Name => "worlds";
    public string HelpText => "List all worlds being hosted by the headless";
    public bool RequirePermission => false;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        List<ManagedWorld> worlds = context.WorldManager.WorldsListForUser(user).ToList();
        StringBuilder sb = new();

        sb.Append("<b>World Listing (");
        sb.Append(worlds.Count);
        sb.AppendLine(" worlds open)</b>");
        foreach (ManagedWorld world in worlds)
        {
            sb.AppendLine($"ID {world.Id}: {world.Name}");
        }

        await channel.SendMessageAsync(sb.ToString());
    }
}