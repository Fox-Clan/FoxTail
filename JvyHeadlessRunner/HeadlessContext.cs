using FrooxEngine;
using HarmonyLib;
using JvyHeadlessRunner.Chat;
using JvyHeadlessRunner.EngineIntegration;
using NotEnoughLogs;

namespace JvyHeadlessRunner;

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