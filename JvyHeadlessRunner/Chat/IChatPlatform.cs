using FrooxEngine;

namespace JvyHeadlessRunner.Chat;

public interface IChatPlatform
{
    public string Name { get; }
    public bool IsUserApproved(IChatUser user);
    public Task SendMessageAsync(IChatChannel channel, string message);
    public Task SendInviteAsync(IChatChannel channel, World world);
}