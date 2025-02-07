namespace JvyHeadlessRunner.Chat;

public class ChatCommandHelper
{
    private readonly HeadlessContext _context;

    private readonly List<IChatPlatform> _platforms = [];

    public ChatCommandHelper(HeadlessContext context)
    {
        _context = context;
    }

    public void AddPlatform(IChatPlatform platform)
    {
        _platforms.Add(platform);
    }

    public async Task ReceiveCommand(IChatChannel channel, IChatUser user, string message)
    {
        this._context.Logger.LogDebug(ResoCategory.Chat, $"[{channel.Platform.Name}/#{channel.Name}] {user.Username}: {message}");
        await channel.Platform.SendMessageAsync(channel, message);
    }
}