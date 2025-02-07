using FrooxEngine;

namespace JvyHeadlessRunner.Chat.Resonite;

public class ResoniteChatPlatform : IChatPlatform
{
    private readonly HeadlessContext _context;
    
    public ResoniteChatPlatform(HeadlessContext context)
    {
        this._context = context;
        
        context.Engine.Cloud.Messages.OnMessageReceived += async message =>
        {
            ResoniteChatChannel channel = new(this, message);
            ResoniteChatUser user = new(this, await this._context.Engine.Cloud.Users.GetUserCached(message.SenderId));

            await this._context.CommandHelper.ReceiveCommand(channel, user, message.Content);
        };
    }

    public string Name => "Resonite";

    public bool IsUserApproved(IChatUser user)
    {
        return false; // TODO
    }

    public async Task SendMessageAsync(IChatChannel channel, string message)
    {
        await this._context.Engine.Cloud.Messages.GetUserMessages(channel.ChannelId).SendTextMessage(message);
    }
}