using FoxTail.Chat;
using FoxTail.EngineIntegration;
using FrooxEngine;
using HarmonyLib;
using NotEnoughLogs;

namespace FoxTail;

#nullable disable

public class HeadlessContext
{
    public static Harmony Harmony;

    public HeadlessRunner Runner;
    
    public Logger Logger;
    public Engine Engine;
    public StandaloneSystemInfo SystemInfo;
    
    public ChatCommandHelper CommandHelper;
}