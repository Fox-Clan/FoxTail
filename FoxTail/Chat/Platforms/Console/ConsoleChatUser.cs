namespace FoxTail.Chat.Platforms.Console;

public class ConsoleChatUser : IChatUser
{
    public ConsoleChatUser(IChatPlatform platform)
    {
        this.Platform = platform;
    }

    public string Username => "console";
    public string UserId => "console";
    public IChatPlatform Platform { get; }
}