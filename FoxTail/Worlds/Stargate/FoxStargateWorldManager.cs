﻿using StargateNetwork.Worlds;

namespace FoxTail.Worlds.Stargate;

public class FoxStargateWorldManager : IStargateWorldManager
{
    private readonly FoxWorldManager _worldManager;

    public FoxStargateWorldManager(HeadlessContext context)
    {
        this._worldManager = context.WorldManager;
    }

    public bool IsWorldRunning(Uri recordUrl)
    {
        return this._worldManager.IsWorldOpen(recordUrl);
    }

    public Task StartWorld(Uri recordUrl)
    {
        return this._worldManager.StartWorld(recordUrl);
    }
}