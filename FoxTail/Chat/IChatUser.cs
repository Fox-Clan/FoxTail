namespace FoxTail.Chat;

public interface IChatUser
{
    public string Username { get; }
    public string UserId { get; }
    public IChatPlatform Platform { get; }
}