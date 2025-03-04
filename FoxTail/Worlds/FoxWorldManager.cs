﻿using FoxTail.Chat.Platforms;
using FoxTail.Chat.Platforms.Resonite;
using FoxTail.Common;
using FrooxEngine;
using SkyFrost.Base;
using User = FrooxEngine.User;

namespace FoxTail.Worlds;

public class FoxWorldManager
{
    private readonly HeadlessContext _context;

    private uint _idIncrement;
    private uint LatestWorldId => Interlocked.Increment(ref _idIncrement);

    private readonly List<ManagedWorld> _worlds = new(1);

    public FoxWorldManager(HeadlessContext context)
    {
        this._context = context;
    }

    public IEnumerable<ManagedWorld> WorldsListForUser(IChatUser user)
    {
        if (this._context.CommandHelper.IsApproved(user))
            return this._worlds;

        return this._worlds.Where(w => w.IsVisibleForUser(user));
    }

    public async Task<ManagedWorld> StartWorld(FoxWorldStartSettings settings, IChatUser? owner = null)
    {
        settings.CreateLoadIndicator = false;

        this._context.Logger.LogInfo(ResoCategory.WorldInit, $"Starting world {settings.FriendlyName}...");
        World world = await Userspace.OpenWorld(settings);
        world.AccessLevel = settings.DefaultAccessLevel ?? SessionAccessLevel.Contacts;
        world.ForceFullUpdateCycle = true;

        if (settings.OverrideName != null)
            world.Name = settings.OverrideName;

        ManagedWorld managed = new(world, owner, LatestWorldId);

        this._context.Logger.LogDebug(ResoCategory.WorldInit, "Waiting for world first update...");
        await world.Coroutines.StartTask(async () =>
        {
            this._context.Logger.LogDebug(ResoCategory.WorldInit, "World is ticking, waiting for session URLs to populate...");
            while (!world.SessionURLs.Any())
                await new NextUpdate();

            this._context.Logger.LogInfo(ResoCategory.WorldInit, "World is up and advertising!");
            
            // Invite users in the background
            _ = Task.Run(async () =>
            {
                await managed.InviteUsersAsync(this._context, settings);
            });
        });

        this._context.Logger.LogInfo(ResoCategory.WorldManager, $"Registered world {managed}.");
        this._worlds.Add(managed);
        return managed;
    }
    
    public Task<ManagedWorld> StartWorld(KnownWorld world, IChatUser? owner = null)
        => StartWorld(world.Compile(), owner);
    
    public Task<ManagedWorld> StartWorld(Uri uri, IChatUser? owner = null)
        => StartWorld(new FoxWorldStartSettings
        {
            URIs = [uri],
            HideFromListing = false,
        }, owner);
    
    public Task<ManagedWorld> StartWorld(WorldAction action, IChatUser? owner = null)
        => StartWorld(new FoxWorldStartSettings
        {
            InitWorld = action,
            HideFromListing = false,
        }, owner);

    public async Task CloseWorld(ManagedWorld world)
    {
        this._context.Logger.LogTrace(ResoCategory.WorldManager, $"Closing {world.Name}...");
        if (world.World.UserCount > 1)
        {
            world.World.RunSynchronously((Action) (() =>
            {
                foreach (User user in world.World.AllUsers)
                {
                    this._context.Logger.LogTrace(ResoCategory.WorldManager, $"Kicking out {user.Name}...");
                    if (!user.IsLocalUser)
                        user.Disconnect();
                }
            }));

            this._context.Logger.LogTrace(ResoCategory.WorldManager, "Waiting for users to disconnect...");
            await world.World.Coroutines.StartTask(async w =>
            {
                DateTime start = DateTime.UtcNow;
                while (w.UserCount > 1 && (DateTime.UtcNow - start).TotalSeconds < 5.0)
                    await new NextUpdate();
            }, world.World);
        }
        
        this._context.Logger.LogTrace(ResoCategory.WorldManager, "Destroying world!");
        world.World.Destroy();
        
        this._worlds.Remove(world);
        this._context.Logger.LogInfo(ResoCategory.WorldManager, $"Unregistered world {world}.");
    }

    public async Task<bool> SaveWorld(ManagedWorld world)
    {
        if (!Userspace.CanSave(world.World) || !world.World.IsAllowedToSaveWorld())
            return false;

        await Userspace.SaveWorldAuto(world.World, SaveType.Overwrite, false);
        return true;
    }

    public ManagedWorld? FindWorldUserIn(IChatUser user)
    {
        if (user is not ResoniteChatUser)
            return null;

        return this._worlds
            .FirstOrDefault(w => w.World.AllUsers.FirstOrDefault(u => u.UserID == user.UserId && u.IsPresentInWorld) != null);
    }
    
    public ManagedWorld? FindWorldById(uint id)
    {
        return this._worlds.FirstOrDefault(w => w.Id == id);
    }

    public bool IsWorldOpen(Uri recordUrl)
    {
        return this._worlds.Any(w => w.World.RecordURL == recordUrl);
    }
}