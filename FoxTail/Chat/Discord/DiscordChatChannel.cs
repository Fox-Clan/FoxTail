using Discord;
using Discord.WebSocket;

namespace FoxTail.Chat.Discord;

public class DiscordChatChannel : IChatChannel
{
    public DiscordChatChannel(IChatPlatform platform, ISocketMessageChannel channel)
    {
        this.Platform = platform;
        this.ChannelId = channel.Id.ToString();
        this.Name = channel.Name;
        this.IsDirect = channel.ChannelType == ChannelType.DM;
    }

    public string Name { get; }
    public string ChannelId { get; }
    public IChatPlatform Platform { get; }
    public bool IsDirect { get; }
}