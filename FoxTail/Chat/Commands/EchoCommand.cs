using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;

namespace FoxTail.Chat.Commands;

public class EchoCommand : IChatCommand
{
    public string Name => "echo";
    public string HelpText => "Echos the message back to you";
    public Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        return channel.SendMessageAsync(args.GetAllArgs());
    }
}