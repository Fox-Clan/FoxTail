using FoxTail.Chat;
using FoxTail.Configuration;
using FoxTail.EngineIntegration;
using FoxTail.Worlds;
using FrooxEngine;
using HarmonyLib;
using NotEnoughLogs;

namespace FoxTail;

#nullable disable

public class HeadlessContext : IDisposable
{
    public static Harmony Harmony;

    public FoxTailConfig Config;
    public WorldConfig WorldConfig;

    public HeadlessRunner Runner;
    
    public Logger Logger;
    public Engine Engine;
    public FoxSystemInfo SystemInfo;
    
    public ChatCommandHelper CommandHelper;
    public FoxWorldManager WorldManager;

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        Logger.LogInfo(ResoCategory.Runner, "Disposing HeadlessContext.");
        
        Logger?.Dispose();
        Engine?.Dispose();
        CommandHelper?.Dispose();
    }
}