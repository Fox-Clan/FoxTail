using SkyFrost.Base;

namespace FoxTail.Chat.Platforms.Resonite;

public class ResoniteChatChannel : IChatChannel
{
    public ResoniteChatChannel(IChatPlatform platform, Message message)
    {
        this.Platform = platform;
        this.Name = message.SenderId;
        this.ChannelId = message.SenderId;
    }

    public string Name { get; }
    public string ChannelId { get; }
    public IChatPlatform Platform { get; }
    public bool IsDirect => true;
}