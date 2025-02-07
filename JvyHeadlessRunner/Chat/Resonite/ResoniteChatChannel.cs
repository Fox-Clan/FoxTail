using SkyFrost.Base;

namespace JvyHeadlessRunner.Chat.Resonite;

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
}