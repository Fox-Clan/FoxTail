using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using NYoutubeDL;
using NYoutubeDL.Helpers;

namespace FoxTail.Patches;

[HarmonyPatch]
public static class VideoTextureProviderPatches
{
    private static readonly MethodInfo GetFullPathInfo = AccessTools.Method(typeof(Extensions), "GetFullPath");
    
    [HarmonyPatch(typeof(VideoTextureProvider), "LoadFromVideoServiceIntern"), HarmonyPrefix]
    public static bool LoadFromVideoServiceInternPrefix(ref YoutubeDL ___youtubeDL)
    {
        if (___youtubeDL == null)
        {
            string? fullPath = (string?)GetFullPathInfo.Invoke(null, [new FileInfo("yt-dlp")]);
            if (fullPath == null)
            {
                Program.Context.Logger.LogWarning(ResoCategory.Runner, "yt-dlp not found!");
            }
            else
            {
                Program.Context.Logger.LogDebug(ResoCategory.Runner, "yt-dlp found at " + fullPath);
            }
            ___youtubeDL = new YoutubeDL(fullPath);
        }
        return true;
    }
}