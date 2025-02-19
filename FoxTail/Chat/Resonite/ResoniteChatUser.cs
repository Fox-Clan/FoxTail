using SkyFrost.Base;

namespace FoxTail.Chat.Resonite;

public class ResoniteChatUser : IChatUser
{
    public ResoniteChatUser(IChatPlatform platform, User user)
    {
        this.Platform = platform;
        this.Username = user.Username;
        this.UserId = user.Id;
    }

    public string Username { get; }
    public string UserId { get; }
    public IChatPlatform Platform { get; }
}