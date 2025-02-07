using System.Collections.Frozen;
using FrooxEngine;
using SkyFrost.Base;

namespace JvyHeadlessRunner.Chat;

public class ChatCommandHelper
{
    private readonly HeadlessContext _context;

    private readonly List<IChatPlatform> _platforms = [];

    private readonly FrozenSet<string> _approvedUserIds = new List<string>
    {
        // Discord
        "956347815267827713", // jvyden
        "329085556791443459", // beyley
        "193490239539511296", // steph
        "288074994188156928", // guppy
        // Resonite
        "U-1YIjc7KyPL6", // jvyden
        "U-1XNdZruECCu", // beyley
        "U-1UOo6cljQsy", // steph
        "U-TheGuppy525", // guppy
    }.ToFrozenSet();

    private bool CheckPerms(IChatUser user) => _approvedUserIds.Contains(user.UserId);

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
                {
                    await Reply(messageSpan.ToString());
                    break;
                }
                case "grid":
                {
                    await Reply("Starting a grid for you, expect an invite shortly!");
                    WorldStartSettings startInfo = new()
                    {
                        InitWorld = WorldPresets.Grid,
                        CreateLoadIndicator = false,
                        HideFromListing = false,
                    };
                    World world = await Userspace.OpenWorld(startInfo);
                    world.Name = $"Fennec Grid (opened by {user.Username})";
                    world.AccessLevel = SessionAccessLevel.Contacts;
                    await world.Coroutines.StartTask(async () =>
                    {
                        await InviteSender(world);
                        while (!world.Permissions.PermissionHandlingInitialized)
                            await new NextUpdate();

                        world.Permissions.DefaultUserPermissions[user.UserId] = world.Permissions.HighestRole;
                    });
                    break;
                }
                case "start":
                {
                    if (!CheckPerms(user))
                    {
                        await Deny();
                        return;
                    }
                        
                    if (!args.MoveNext())
                    {
                        await Reply("I need the record URL to start the world.");
                        break;
                    }

                    string urlStr = messageSpan[args.Current].ToString();

                    urlStr = urlStr switch
                    {
                        "eepy" => "resrec:///G-1UXaEEXzaEa/R-c061f0ff-dd34-41d4-9349-e8b3aed6b487",
                        "cuddly" => "resrec:///U-AlphaNeon/R-b4a66dd6-b7da-424a-9538-0207cbb8cc59",
                        "selfstudy" => "resrec:///G-1UXaEEXzaEa/R-42bbb839-2f3e-45bc-884b-4932e0acc201",
                        "classroom" => "resrec:///G-1UXaEEXzaEa/R-c61252d9-333f-45f7-ae0b-2961fe58ddaf",
                        "sgc" => "resrec:///G-1UXaEEXzaEa/R-0ece8266-41ef-4837-8e2a-3249e858bc34",
                        _ => urlStr,
                    };
                    
                    await Reply("Starting that world for you, expect an invite shortly!");
                    WorldStartSettings startInfo = new()
                    {
                        URIs = [
                            new Uri(urlStr),
                        ],
                        CreateLoadIndicator = false,
                        HideFromListing = false,
                    };
                    
                    World world = await Userspace.OpenWorld(startInfo);
                    world.AccessLevel = SessionAccessLevel.Contacts;
                    await world.Coroutines.StartTask(async () =>
                    {
                        await InviteSender(world);
                        while (!world.Permissions.PermissionHandlingInitialized)
                            await new NextUpdate();

                        world.Permissions.DefaultUserPermissions[user.UserId] = world.Permissions.HighestRole;
                    });
                    break;
                }
                default:
                {
                    if (channel.IsDirect)
                        await Reply("fennec no know that command :(");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            _context.Logger.LogError(ResoCategory.Chat, $"Exception while running command {command}: {e}");
            await Reply("fennec fucked up. sowwy");
            await Reply($"{e.GetType()}: {e.Message}");
        }

        return;

        Task Deny()
        {
            return Reply("Ah-ah-ah, you didn't say the magic word!");
        }
        
        Task Reply(string content)
        {
            return channel.Platform.SendMessageAsync(channel, content);
        }

        async Task InviteSender(World world)
        {
            world.AllowUserToJoin(user.UserId);
            while (!world.SessionURLs.Any())
                await new NextUpdate();
            
            await channel.Platform.SendInviteAsync(channel, world);
        }
    }
}