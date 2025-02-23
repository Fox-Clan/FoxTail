using FoxTail.Chat.Platforms.Resonite;
using FoxTail.Common;
using FrooxEngine;
using SkyFrost.Base;

namespace FoxTail.Chat.Platforms.Console;

public class ConsoleChatPlatform : IChatPlatform, IDisposable
{
    private readonly HeadlessContext _context;
    private readonly CancellationTokenSource _cts = new();
    
    public ConsoleChatPlatform(HeadlessContext context)
    {
        this._context = context;
        
        Thread thread = new(async () =>
        {
            using StreamReader reader = new(System.Console.OpenStandardInput(), System.Console.InputEncoding);
            while (!this._disposed && await reader.ReadLineAsync(this._cts.Token) is { } line)
            {
                Message message = new()
                {
                    Content = line,
                    SenderId = "console"
                };

                ResoniteChatChannel channel = new(this, message);
                ConsoleChatUser user = new(this);
                
                await _context.CommandHelper.ReceiveCommand(channel, user, ChatCommandHelper.ParseSimpleCommand(line));
            }
            
            this._context.Logger.LogTrace(ResoCategory.Chat, "Console input thread exited");
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
        this._cts.Cancel();
        GC.SuppressFinalize(this);
    }
}