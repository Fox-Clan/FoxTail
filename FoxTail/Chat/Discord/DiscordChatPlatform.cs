using Discord;
using Discord.WebSocket;
using FrooxEngine;
using NotEnoughLogs;
using SkyFrost.Base;
using UserStatus = Discord.UserStatus;

namespace FoxTail.Chat.Discord;

public class DiscordChatPlatform : IChatPlatform, IDisposable
{
    private readonly HeadlessContext _context;
    private readonly DiscordSocketClient _client;

    private string? _token;
    
    public DiscordChatPlatform(HeadlessContext context, string token)
    {
        this._token = token;
        this._context = context;
        this._client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.DirectMessages |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        });
        this._client.Log += message =>
        {
            LogLevel level = message.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Info,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => throw new ArgumentOutOfRangeException()
            };
            this._context.Logger.Log(level, ResoCategory.Discord, $"{message.Message}{message.Exception}");
            
            return Task.CompletedTask;
        };

        this._client.LatencyUpdated += async (_, _) => await UpdateStatus();
        
        this._client.MessageReceived += MessageReceived;
    }

    public async Task InitializeAsync()
    {
        await this._client.LoginAsync(TokenType.Bot, this._token);
        this._token = null;

#pragma warning disable CS4014
        Task.Factory.StartNew(this._client.StartAsync, TaskCreationOptions.LongRunning);
#pragma warning restore CS4014
    }

    public string Name => "Discord";

    public async Task SendMessageAsync(IChatChannel channel, string message)
    {
        if (this._client.GetChannel(ulong.Parse(channel.ChannelId)) is not SocketTextChannel discordChannel)
            throw new Exception($"Channel {channel.ChannelId} ({channel.Name}) not found");

        await discordChannel.SendMessageAsync(message);
    }

    public Task SendInviteAsync(IChatChannel channel, World world)
    {
        SessionInfo sessionInfo = world.GenerateSessionInfo();
        Uri url = sessionInfo.GetSessionURLs().First();
        return SendMessageAsync(channel, $"# {world.Name}\n`{url.ToString()}`\n-# Copy this link and paste it into Resonite to join!");
    }
    
    private async Task MessageReceived(SocketMessage message)
    {
        if (message.Author.Id == this._client.CurrentUser.Id)
            return;
        
        DiscordChatChannel channel = new(this, message.Channel);
        DiscordChatUser user = new(this, message.Author);

        _ = Task.Run(async () => await this._context.CommandHelper.ReceiveCommand(channel, user, message.Content));
    }

    private string _lastCustomStatus = "";
    private UserStatus _lastStatus = UserStatus.Online;

    private async Task UpdateStatus()
    {
        this._context.Logger.LogDebug(ResoCategory.Discord, "Updating Discord status...");
        WorldManager worlds = _context.Engine.WorldManager;
        int userCount = worlds.Worlds.Sum(w => w.AllUsers.Count(u => u.HeadDevice != HeadOutputDevice.Headless));
        int worldCount = worlds.WorldCount - 1;
        
        UserStatus status = userCount == 0 || worldCount == 0 ? UserStatus.Idle : UserStatus.Online;
        if (status != this._lastStatus)
        {
            this._context.Logger.LogTrace(ResoCategory.Discord, $"Setting UserStatus to {status}");
            await this._client.SetStatusAsync(status);
            this._lastStatus = status;
        }

        string customStatus = $"Hosting {worldCount} world{(worldCount == 1 ? "" : "s")} with {userCount} player{(userCount == 1 ? "" : "s")}";
        if (customStatus != this._lastCustomStatus)
        {
            this._context.Logger.LogTrace(ResoCategory.Discord, $"Setting CustomStatus to {customStatus}");
            await this._client.SetCustomStatusAsync(customStatus);
            this._lastCustomStatus = customStatus;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}