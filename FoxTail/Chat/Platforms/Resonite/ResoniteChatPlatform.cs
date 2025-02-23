using FrooxEngine;
using SkyFrost.Base;

namespace FoxTail.Chat.Platforms.Resonite;

public class ResoniteChatPlatform : IChatPlatform
{
    private readonly HeadlessContext _context;
    
    public ResoniteChatPlatform(HeadlessContext context)
    {
        this._context = context;
        
        context.Engine.Cloud.Messages.OnMessageReceived += async message =>
        {
            ResoniteChatChannel channel = new(this, message);
            ResoniteChatUser user = new(this, (await this._context.Engine.Cloud.Users.GetUserCached(message.SenderId)).Entity);

            await this._context.CommandHelper.ReceiveCommand(channel, user, message.Content);
        };
    }

    public string Name => "Resonite";

    public async Task SendMessageAsync(IChatChannel channel, string message)
    {
        await this._context.Engine.Cloud.Messages
            .GetUserMessages(channel.ChannelId)
            .SendTextMessage(message);
    }

    public async Task SendInviteAsync(IChatChannel channel, World world)
    {
        UserMessages? userMessages = this._context.Engine.Cloud.Messages
            .GetUserMessages(channel.ChannelId);

        Message inviteMessage = await userMessages.CreateInviteMessage(world);
        await userMessages.SendMessage(inviteMessage);
    }
}