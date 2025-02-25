﻿using FoxTail.Chat;
using FoxTail.Common;
using FoxTail.Configuration;
using FoxTail.EngineIntegration;
using FoxTail.Worlds;
using FrooxEngine;
using HarmonyLib;
using NotEnoughLogs;
using StargateNetwork;

namespace FoxTail;

#nullable disable

public class HeadlessContext : IDisposable
{
    public static Harmony Harmony;

    public FoxTailConfig Config;
    public WorldConfig WorldConfig;
    public UserConfig UserConfig;
    public StargateConfiguration StargateConfig;

    public HeadlessRunner Runner;
    
    public Logger Logger;
    public Engine Engine;
    public FoxSystemInfo SystemInfo;
    
    public ChatCommandHelper CommandHelper;
    public FoxWorldManager WorldManager;

    public StargateServer StargateServer;

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        Logger.LogInfo(ResoCategory.Runner, "Disposing HeadlessContext.");
        
        FoxBunkumServer.Stop();
        StargateServer.Dispose();
        CommandHelper?.Dispose();
        Engine?.Dispose();
        Logger?.Dispose();
    }
}