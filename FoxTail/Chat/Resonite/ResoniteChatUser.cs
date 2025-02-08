using SkyFrost.Base;

namespace FoxTail.Chat.Resonite;

public class ResoniteChatUser : IChatUser
{
    public ResoniteChatUser(IChatPlatform platform, CloudResult<User> user)
    {
        this.Platform = platform;
        this.Username = user.Entity.Username;
        this.UserId = user.Entity.Id;
    }

    public string Username { get; }
    public string UserId { get; }
    public IChatPlatform Platform { get; }
}