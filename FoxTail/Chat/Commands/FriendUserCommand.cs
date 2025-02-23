using FoxTail.Chat.CommandSupport;
using FoxTail.Chat.Platforms;
using SkyFrost.Base;

namespace FoxTail.Chat.Commands;

public class FriendUserCommand : IChatCommand
{
    public string Name => "friend";
    public string HelpText => "Tells the headless user to add a user as a contact";
    public bool RequirePermission => true;
    public async Task InvokeAsync(HeadlessContext context, IChatChannel channel, IChatUser user, ArgContainer args)
    {
        User? cloudUser = await args.GetCloudUserAsync("username", context);
        if (cloudUser == null)
        {
            await channel.SendMessageAsync("I couldn't find that user. Have you specified the username?");
            return;
        }
                    
        Contact? contact = context.Engine.Cloud.Contacts.GetContact(cloudUser.Id);
        if (contact is { ContactStatus: ContactStatus.Requested })
        {
            await context.Engine.Cloud.Contacts.AddContact(contact);
            await channel.SendMessageAsync("Request accepted!");
        }
        else if(contact?.ContactStatus != ContactStatus.Accepted)
        {
            await context.Engine.Cloud.Contacts.AddContact(cloudUser.Id, cloudUser.Username);
            await channel.SendMessageAsync("Sent a request.");
        }
        else if (contact.ContactStatus == ContactStatus.Accepted)
        {
            await channel.SendMessageAsync("I already have that person as friends.");
        }
        else
        {
            await channel.SendMessageAsync($"you did something impossible. contact:{contact} status:{contact.ContactStatus} user:{cloudUser}");
        }
    }
}