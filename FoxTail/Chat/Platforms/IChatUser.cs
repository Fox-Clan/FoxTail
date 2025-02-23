namespace FoxTail.Chat.Platforms;

public interface IChatUser
{
    public string Username { get; }
    public string UserId { get; }
    public IChatPlatform Platform { get; }
}