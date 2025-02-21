using FrooxEngine;
using HarmonyLib;

// ReSharper disable all

namespace FoxTail.Patches;

[HarmonyPatch]
public static class WorldPresetsPatches
{
    [HarmonyPatch(typeof(WorldPresets), nameof(WorldPresets.LocalWorld)), HarmonyPrefix]
    public static bool LocalWorldPrefix(World w)
    {
        WorldPresets.BlankWorld(w);
        return false;
    }
}