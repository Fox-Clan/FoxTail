using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FoxTail.Chat.Platforms.Console;
using FoxTail.Common;
using FoxTail.Worlds;
using FrooxEngine;
using SkyFrost.Base;
using User = FrooxEngine.User;

namespace FoxTail.Chat;

public class ChatCommandHelper : IDisposable
{
    private readonly HeadlessContext _context;

    private readonly List<IChatPlatform> _platforms = [];
    private readonly FrozenSet<IChatCommand> _commands;
    

    public ChatCommandHelper(HeadlessContext context)
    {
        this._context = context;
        this._commands = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IChatCommand)))
            .Select(t => (IChatCommand)Activator.CreateInstance(t)!)
            .ToFrozenSet();
    }

    public void AddPlatform(IChatPlatform platform)
    {
        this._platforms.Add(platform);
    }
    
    public bool IsApproved(IChatUser user)
    {
        if (user is ConsoleChatUser)
            return true;

        if (_context.UserConfig.Owner.Ids.Contains(user.UserId))
            return true;

        if (_context.UserConfig.Users.Any(u => u.Ids.Contains(user.UserId)))
            return true;

        return false;
    }

    public static (string command, EnumeratingArgContainer args) ParseSimpleCommand(string line)
    {
        ReadOnlySpan<char> messageSpan = line;
        if (messageSpan.StartsWith('!') || messageSpan.StartsWith('/'))
            messageSpan = messageSpan[1..];
                
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

        return (command, new EnumeratingArgContainer(messageSpan.ToString()));
    }

    public async Task ReceiveCommand(IChatChannel channel, IChatUser user, string command, ArgContainer args)
    {
        this._context.Logger.LogDebug(ResoCategory.Chat, $"[{channel.Platform.Name}/#{channel.Name}] {user.Username}: !{command} {args.GetAllArgs()}");
        
        // actually handle the command
        try
        {
            IChatCommand? chatCommand = this._commands.FirstOrDefault(c => c.Name.Equals(command, StringComparison.InvariantCultureIgnoreCase));
            if (chatCommand != null)
            {
                if (chatCommand.RequirePermission && !this.IsApproved(user))
                {
                    await Deny();
                    return;
                }
                await chatCommand.InvokeAsync(this._context, channel, user, args);
                return;
            }
            // else // TODO: uncomment when all commands ported
            // {
            //     if (channel.IsDirect)
            //         await Reply("fennec no know that command :(");
            //     return;
            // }

            switch (command)
            {
                case "promote": // TODO: aliases
                case "admin":
                {
                    await ReceiveCommand(channel, user, "role", new EnumeratingArgContainer("admin"));
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
    
    public Task ReceiveCommand(IChatChannel channel, IChatUser user, (string command, EnumeratingArgContainer args) command)
    {
        return ReceiveCommand(channel, user, command.command, command.args);
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