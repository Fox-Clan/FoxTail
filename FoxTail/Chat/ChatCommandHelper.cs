using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
using Elements.Core;
using FoxTail.Chat.Resonite;
using FoxTail.Worlds;
using FrooxEngine;
using SkyFrost.Base;
using User = FrooxEngine.User;

namespace FoxTail.Chat;

public class ChatCommandHelper : IDisposable
{
    private readonly HeadlessContext _context;

    private readonly List<IChatPlatform> _platforms = [];

    private readonly FrozenSet<string> _approvedUserIds = new List<string>
    {
        "console",
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

    public bool IsApproved(IChatUser user) => _approvedUserIds.Contains(user.UserId);

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
        if (!message.StartsWith('!'))
            return;
        
        this._context.Logger.LogDebug(ResoCategory.Chat, $"[{channel.Platform.Name}/#{channel.Name}] {user.Username}: {message}");

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
                    ManagedWorld world = await _context.WorldManager.StartWorld(WorldPresets.Grid, user);
                    await world.InviteAndPromoteOwner(channel);
                    break;
                }
                case "platform":
                {
                    await Reply("Starting a platform for you, expect an invite shortly!");
                    ManagedWorld world = await _context.WorldManager.StartWorld(WorldPresets.SimplePlatform, user);
                    await world.InviteAndPromoteOwner(channel);
                    break;
                }
                case "start":
                {
                    if (!IsApproved(user))
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

                    string? knownUrlStr = _context.WorldConfig.GetKnownWorldUrlById(urlStr);
                    if (knownUrlStr != null)
                        urlStr = knownUrlStr;

                    if (urlStr.StartsWith('h'))
                        urlStr = urlStr.Replace("https://go.resonite.com/record", "resrec://");

                    bool urlValid = Uri.TryCreate(urlStr, UriKind.Absolute, out Uri? uri);
                    if (!urlValid || uri == null)
                    {
                        await Reply("I couldn't find that world alias. Try providing the record URL instead.");
                        break;
                    }
                    
                    await Reply("Starting that world for you, expect an invite shortly!");
                    ManagedWorld world = await _context.WorldManager.StartWorld(uri, user);
                    await world.InviteAndPromoteOwner(channel);
                    break;
                }
                case "close":
                {
                    if (!IsApproved(user))
                    {
                        await Deny();
                        break;
                    }
                    
                    ManagedWorld? world = this._context.WorldManager.FindWorldUserIn(user);
                    if (world == null)
                    {
                        await Reply("I couldn't find the world you were in, so I can't close it. Try joining/focusing the world.");
                        break;
                    }
                    
                    await this._context.WorldManager.CloseWorld(world);
                    await Reply("Closed " + world.Name + ".");
                    break;
                }
                case "save":
                {
                    if (!IsApproved(user))
                    {
                        await Deny();
                        return;
                    }
                    
                    ManagedWorld? world = this._context.WorldManager.FindWorldUserIn(user);
                    if (world == null)
                    {
                        await Reply("I couldn't find the world you were in, so I can't save it. Try joining/focusing the world.");
                        break;
                    }
                    
                    await Reply("Saving world...");

                    if (await this._context.WorldManager.SaveWorld(world))
                    {
                        await Reply("World saved and overwritten.");
                    }
                    else
                    {
                        await Reply("I can't save that world as I don't own that world. You can use 'Save As...' under Session to save it yourself.");
                    }
                    
                    break;
                }
                case "promote":
                case "admin":
                {
                    await ReceiveCommand(channel, user, "!role admin");
                    break;
                }
                case "role":
                {
                    if (!IsApproved(user))
                    {
                        await Deny();
                        break;
                    }
                    
                    ManagedWorld? world = this._context.WorldManager.FindWorldUserIn(user);
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
                    PermissionSet? role = world.World.Permissions.Roles.
                        FirstOrDefault(r => r.RoleName.Value.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));

                    if (role == null)
                    {
                        await Reply("That world doesn't have that role. Did you make a typo?");
                        break;
                    }
                    
                    User worldUser = world.World.GetUserByUserId(user.UserId);

                    world.World.RunSynchronously(() =>
                    {
                        worldUser.Role = role;
                        worldUser.World.Permissions.AssignDefaultRole(worldUser, role);
                    });
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
                    await Reply($"Garbage collection took {sw.ElapsedMilliseconds}ms.");
                    break;
                }
                case "allowurl":
                {
                    if (!IsApproved(user))
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
                case "shutdown":
                {
                    if (!IsApproved(user))
                    {
                        await Deny();
                        break;
                    }

                    await Reply("Shutting down!");
                    _context.Runner.Exit();
                    break;
                }
                case "friend":
                {
                    if (!IsApproved(user))
                    {
                        await Deny();
                        break;
                    }
                    
                    if (!args.MoveNext())
                    {
                        await Reply("I need the username of the friend to add.");
                        break;
                    }
                    
                    string friendName = messageSpan[args.Current].ToString();
                    SkyFrost.Base.User? cloudUser = (await _context.Engine.Cloud.Users.GetUserByName(friendName)).Entity;

                    if (cloudUser == null)
                    {
                        await Reply("I couldn't find that user.");
                        break;
                    }
                    
                    Contact? contact = _context.Engine.Cloud.Contacts.GetContact(cloudUser.Id);
                    if (contact != null && contact.ContactStatus == ContactStatus.Requested)
                    {
                        await _context.Engine.Cloud.Contacts.AddContact(contact);
                        await Reply("Request accepted!");
                    }
                    else if(contact?.ContactStatus != ContactStatus.Accepted)
                    {
                        await _context.Engine.Cloud.Contacts.AddContact(cloudUser.Id, cloudUser.Username);
                        await Reply("Sent a request.");
                    }
                    else if (contact.ContactStatus == ContactStatus.Accepted)
                    {
                        await Reply("I already have that person as friends.");
                    }
                    else
                    {
                        await Reply($"you did something impossible. contact:{contact} status:{contact.ContactStatus} user:{cloudUser}");
                    }
                    break;
                }
                case "worlds":
                {
                    List<ManagedWorld> worlds = this._context.WorldManager.WorldsListForUser(user).ToList();
                    StringBuilder sb = new();

                    sb.Append("<b>World Listing (");
                    sb.Append(worlds.Count);
                    sb.AppendLine(" worlds open)</b>");
                    foreach (ManagedWorld world in worlds)
                    {
                        sb.AppendLine($"ID {world.Id}: {world.Name}");
                    }

                    await Reply(sb.ToString());
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
    }

    public void Dispose()
    {
        foreach (IChatPlatform chatPlatform in this._platforms)
        {
            if(chatPlatform is IDisposable disposable)
                disposable.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}