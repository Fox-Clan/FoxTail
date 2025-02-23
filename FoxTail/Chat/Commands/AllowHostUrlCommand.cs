using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using FrooxEngine;

namespace FoxTail.Chat.Commands;

public class AllowHostUrlCommand : IChatCommand
{
    public string Name => "allowUrl";
    public string HelpText => "Temporarily allows a URL across all worlds";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        Uri? uri = args.GetUri("hostUrl");
        if (uri == null)
        {
            await channel.SendMessageAsync("I need a valid URL to allow.");
            return;
        }

        await context.Engine.GlobalCoroutineManager.StartTask(async s =>
        {
            await new NextUpdate();
                        
            s.TemporarilyAllowHTTP(uri.Host);
            s.TemporarilyAllowWebsocket(uri.Host, uri.Port);
            s.TemporarilyAllowOSC_Sender(uri.Host, uri.Port);
                        
        }, context.Engine.Security);

        await channel.SendMessageAsync("Host successfully allowed.");
    }
}