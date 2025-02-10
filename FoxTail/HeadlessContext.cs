using FoxTail.Chat;
using FoxTail.Configuration;
using FoxTail.EngineIntegration;
using FrooxEngine;
using HarmonyLib;
using NotEnoughLogs;

namespace FoxTail;

#nullable disable

public class HeadlessContext : IDisposable
{
    public static Harmony Harmony;

    public FoxTailConfig Config;

    public HeadlessRunner Runner;
    
    public Logger Logger;
    public Engine Engine;
    public StandaloneSystemInfo SystemInfo;
    
    public ChatCommandHelper CommandHelper;

    public void Dispose()
    {
        Logger?.Dispose();
        Engine?.Dispose();
    }
}