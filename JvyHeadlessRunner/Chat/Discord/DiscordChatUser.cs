using Discord.WebSocket;

namespace JvyHeadlessRunner.Chat.Discord;

public class DiscordChatUser : IChatUser
{
    public DiscordChatUser(IChatPlatform platform, SocketUser user)
    {
        this.Platform = platform;
        this.Username = user.Username;
        this.UserId = user.Id.ToString();
    }

    public IChatPlatform Platform { get; }
    public string Username { get; }
    public string UserId { get; }
}