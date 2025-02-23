using FoxTail.Chat.Platforms;
using FrooxEngine;

namespace FoxTail.Chat;

public static class ChatExtensions
{
    public static Task SendMessageAsync(this IChatChannel channel, string message)
    {
        return channel.Platform.SendMessageAsync(channel, message);
    }
    
    public static Task SendInviteAsync(this IChatChannel channel, World world)
    {
        return channel.Platform.SendInviteAsync(channel, world);
    }
}