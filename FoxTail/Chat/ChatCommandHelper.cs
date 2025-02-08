using System.Collections.Frozen;
using System.Diagnostics;
using FoxTail.Chat.Resonite;
using FrooxEngine;
using SkyFrost.Base;
using User = FrooxEngine.User;

namespace FoxTail.Chat;

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

    public World? GetWorldUserIn(IChatUser user)
    {
        if (user is not ResoniteChatUser)
            return null;

        return this._context.Engine.WorldManager.Worlds
            .FirstOrDefault(w => w.AllUsers.FirstOrDefault(u => u.UserID == user.UserId && u.IsPresentInWorld) != null);
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
                        break;
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
                case "close":
                {
                    if (!CheckPerms(user))
                    {
                        await Deny();
                        break;
                    }
                    
                    World? world = GetWorldUserIn(user);
                    if (world == null)
                    {
                        await Reply("I couldn't find the world you were in, so I can't close it. Try joining/focusing the world.");
                        break;
                    }
                    
                    world.Destroy();
                    await Reply("Closed " + world.Name + ".");
                    break;
                }
                case "save":
                {
                    if (!CheckPerms(user))
                    {
                        await Deny();
                        return;
                    }
                    
                    World? world = GetWorldUserIn(user);
                    if (world == null)
                    {
                        await Reply("I couldn't find the world you were in, so I can't save it. Try joining/focusing the world.");
                        break;
                    }

                    if (!Userspace.CanSave(world))
                    {
                        await Reply("I can't save that world as I don't own that world. You can use 'Save As...' under Session to save it yourself.");
                        break;
                    }

                    await Userspace.SaveWorldAuto(world, SaveType.Overwrite, false);
                    await Reply("World saved and overwritten.");
                    break;
                }
                case "promote":
                case "admin":
                    await ReceiveCommand(channel, user, "!role admin");
                    break;
                case "role":
                {
                    if (!CheckPerms(user))
                    {
                        await Deny();
                        break;
                    }
                    
                    World? world = GetWorldUserIn(user);
                    if (world == null)
                    {
                        await Reply("I couldn't find the world you were in, so I can't set your role. Try joining/focusing the world.");
                        break;
                    }
                    
                    if (!args.MoveNext())
                    {
                        await Reply("I need the role to set you to. For example, you can do \"!role admin\".");
                        break;
                    }

                    string roleName = messageSpan[args.Current].ToString();
                    PermissionSet? role = world.Permissions.Roles.
                        FirstOrDefault(r => r.RoleName.Value.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));

                    if (role == null)
                    {
                        await Reply("That world doesn't have that role. Did you make a typo?");
                        break;
                    }
                    
                    User worldUser = world.GetUserByUserId(user.UserId);

                    await world.Coroutines.StartTask(async u =>
                    {
                        await new NextUpdate();
                        u.Role = role;
                        u.World.Permissions.AssignDefaultRole(u, role);
                    }, worldUser);
                    break;
                }
                case "gc":
                {
                    // restrict to those who know what this means
                    if (user.UserId != "U-1XNdZruECCu" && user.UserId != "U-1YIjc7KyPL6")
                    {
                        await Deny();
                        return;
                    }

                    await Reply("Collecting...");
                    Stopwatch sw = Stopwatch.StartNew();
                    GC.Collect();
                    sw.Stop();
                    await Reply($"Garbage collection took {sw.ElapsedMilliseconds}.");
                    break;
                }
                case "allowurl":
                {
                    if (!CheckPerms(user))
                    {
                        await Deny();
                        break;
                    }
                    
                    if (!args.MoveNext())
                    {
                        await Reply("I need the URL to allow.");
                        break;
                    }
                    
                    string host = messageSpan[args.Current].ToString();
                    
                    if (!Uri.TryCreate(host, UriKind.Absolute, out Uri? uri))
                    {
                        await Reply($"Couldn't allow host {host} as it's an invalid URL");
                        break;
                    }

                    await this._context.Engine.GlobalCoroutineManager.StartTask(async s =>
                    {
                        await new NextUpdate();
                        
                        s.TemporarilyAllowHTTP(uri.Host);
                        s.TemporarilyAllowWebsocket(uri.Host, uri.Port);
                        s.TemporarilyAllowOSC_Sender(uri.Host, uri.Port);
                        
                    }, this._context.Engine.Security);

                    await Reply("Host successfully allowed.");
                    
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