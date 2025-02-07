namespace JvyHeadlessRunner.Chat;

public interface IChatChannel
{
    public string Name { get; }
    public string ChannelId { get; }
    public IChatPlatform Platform { get; }
    public bool IsDirect { get; }
}