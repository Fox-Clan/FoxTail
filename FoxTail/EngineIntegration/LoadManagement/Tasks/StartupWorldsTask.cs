using FrooxEngine;
using SkyFrost.Base;
using User = SkyFrost.Base.User;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class StartupWorldsTask : InitTask
{
    private readonly List<FoxWorldStartSettings> _worlds =
    [
        new()
        {
            FriendlyName = "eepy world",
            URIs = [new Uri("resrec:///G-1UXaEEXzaEa/R-c061f0ff-dd34-41d4-9349-e8b3aed6b487")],
            #if DEBUG
            InviteUsernames = ["jvyden"],
            #else
            InviteUsernames = ["jvyden","Lyris","TheGuppy525","stephblackcat"],
            #endif
            InviteMessage = "<color=red>EEPY TIME!!!!!!!!</color>",
            HideFromListing = false,
        },
    ];
    
    public override string Name => "Default World Startup";
    public override InitTaskStage Stage => InitTaskStage.Authenticated;
    public override async Task ExecuteAsync(HeadlessContext context)
    {
        foreach (FoxWorldStartSettings worldConfig in this._worlds)
        {
            worldConfig.CreateLoadIndicator = false;
            
            context.Logger.LogInfo(ResoCategory.WorldInit, $"Starting world {worldConfig.FriendlyName}...");
            World world = await Userspace.OpenWorld(worldConfig);
            world.AccessLevel = worldConfig.DefaultAccessLevel ?? SessionAccessLevel.Contacts;

            if (worldConfig.OverrideName != null)
                world.Name = worldConfig.OverrideName;
            
            context.Logger.LogDebug(ResoCategory.WorldInit, "Waiting for world first update...");
            await world.Coroutines.StartTask(async () =>
            {
                context.Logger.LogDebug(ResoCategory.WorldInit, "World is ticking, waiting for session URLs to populate...");
                while (!world.SessionURLs.Any())
                    await new NextUpdate();
                
                context.Logger.LogDebug(ResoCategory.WorldInit, "World is up and advertising!");
                foreach (string inviteUsername in worldConfig.InviteUsernames)
                {
                    context.Logger.LogInfo(ResoCategory.WorldInit, $"Inviting {inviteUsername} to {world.Name}");
                    User? user = (await context.Engine.Cloud.Users.GetUserByName(inviteUsername)).Entity;
                    if (user == null)
                    {
                        context.Logger.LogWarning(ResoCategory.WorldInit, $"Couldn't find user by name {inviteUsername}, can't invite");
                        continue;
                    }
                    world.AllowUserToJoin(user.Id);

                    UserMessages? userMessages = context.Engine.Cloud.Messages.GetUserMessages(user.Id);
                    
                    if (worldConfig.InviteMessage != null)
                    {
                        context.Logger.LogTrace(ResoCategory.WorldInit, "Sending TextMessage...");
                        await userMessages.SendTextMessage(worldConfig.InviteMessage);
                    }
                    
                    context.Logger.LogTrace(ResoCategory.WorldInit, "Sending InviteMessage...");
                    await userMessages.SendInviteMessage(world.GenerateSessionInfo());
                    
                    context.Logger.LogTrace(ResoCategory.WorldInit, $"Successfully invited {inviteUsername} to {world.Name}");
                }
            });
        }
        
        context.Logger.LogInfo(ResoCategory.WorldInit, "All worlds successfully started!");
        this._worlds.Clear();
    }
}