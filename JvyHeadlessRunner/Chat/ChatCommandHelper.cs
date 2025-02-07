using FrooxEngine;
using FrooxEngine.Headless;
using SkyFrost.Base;

namespace JvyHeadlessRunner.Chat;

public class ChatCommandHelper
{
    private readonly HeadlessContext _context;

    private readonly List<IChatPlatform> _platforms = [];

    public ChatCommandHelper(HeadlessContext context)
    {
        _context = context;
    }

    public void AddPlatform(IChatPlatform platform)
    {
        _platforms.Add(platform);
    }

    public async Task ReceiveCommand(IChatChannel channel, IChatUser user, string message)
    {
        this._context.Logger.LogDebug(ResoCategory.Chat, $"[{channel.Platform.Name}/#{channel.Name}] {user.Username}: {message}");

        if (!message.StartsWith('!'))
            return;

        ReadOnlySpan<char> messageSpan = message.AsSpan()[1..];
        int firstSpace = messageSpan.IndexOf(' ');

        string command;
        if (firstSpace == -1)
        {
            command = messageSpan.ToString();
            messageSpan = [];
        }
        else
        {
            command = messageSpan[..firstSpace].ToString();
            messageSpan = messageSpan[(command.Length + 1)..];
        }

        MemoryExtensions.SpanSplitEnumerator<char> args = messageSpan.Split(' ').GetEnumerator();
        
        // actually handle the command
        try
        {
            switch (command)
            {
                case "echo":
                    await Reply(messageSpan.ToString());
                    break;
                case "grid":
                    WorldStartSettings startInfo = new()
                    {
                        InitWorld = WorldPresets.Grid,
                        DefaultAccessLevel = SessionAccessLevel.Contacts,
                        CreateLoadIndicator = false,
                        HideFromListing = false,
                    };
                    World world = await Userspace.OpenWorld(startInfo);
                    await Reply(world.SessionURLs.First().ToString()); // FIXME: crashes
                    break;
                default:
                    if (channel.IsDirect)
                        await Reply("fennec no know that command :(");
                    break;
            }
        }
        catch (Exception e)
        {
            _context.Logger.LogError(ResoCategory.Chat, $"Exception while running command {command}: {e}");
            await Reply("fennec fucked up. sowwy");
            await Reply($"{e.GetType()}: {e.Message}");
        }

        return;
        Task Reply(string content)
        {
            return channel.Platform.SendMessageAsync(channel, content);
        }
    }
}