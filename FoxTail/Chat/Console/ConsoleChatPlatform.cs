using FoxTail.Chat.Resonite;
using FrooxEngine;
using SkyFrost.Base;
using User = SkyFrost.Base.User;

namespace FoxTail.Chat.Console;

public class ConsoleChatPlatform : IChatPlatform, IDisposable
{
    private readonly HeadlessContext _context;
    
    public ConsoleChatPlatform(HeadlessContext context)
    {
        this._context = context;
        
        Thread thread = new(() =>
        {
            while (!this._disposed)
            {
                string? line = System.Console.ReadLine();
                if (line == null) break; // stop thread if no stdin

                Message message = new()
                {
                    Content = line,
                    SenderId = "console",
                };

                ResoniteChatChannel channel = new(this, message);
                ResoniteChatUser user = new(this, new User
                {
                    Username = "console",
                    Id = "console",
                });

                _context.CommandHelper.ReceiveCommand(channel, user, line).Wait();
            }
        });
        thread.Name = "ConsoleChatPlatform Input Thread";
        thread.Start();
    }

    private bool _disposed;
    
    public string Name => "Console";
    public Task SendMessageAsync(IChatChannel channel, string message)
    {
        ConsoleColor oldColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(message);
        System.Console.ForegroundColor = oldColor;
        
        return Task.CompletedTask;
    }

    public Task SendInviteAsync(IChatChannel channel, World world)
    {
        SessionInfo sessionInfo = world.GenerateSessionInfo();
        Uri url = sessionInfo.GetSessionURLs().First();
        return SendMessageAsync(channel, "INVITE: " + url);
    }

    public void Dispose()
    {
        this._disposed = true;
        GC.SuppressFinalize(this);
    }
}