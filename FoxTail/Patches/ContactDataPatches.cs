using System.Reflection;
using System.Reflection.Emit;
using Elements.Core;
using HarmonyLib;
using SkyFrost.Base;

namespace FoxTail.Patches;

[HarmonyPatch]
public static class ContactDataPatches
{
    private static readonly MethodInfo Warn = AccessTools.Method(typeof(UniLog), nameof(UniLog.Warning));
    
    [HarmonyPatch(typeof(ContactData), "UpdateStatus"), HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> UpdateStatusTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeMatcher codeMatcher = new(instructions, generator);

        codeMatcher.MatchStartForward(CodeMatch.Calls(Warn))
            .ThrowIfInvalid("Couldn't find call to UniLog.Warn")
            .Advance(-10)
            .RemoveInstructions(11);
        
        // removes:
        /*
         IL_000B: ldstr      "Received status update that\'s already expired:\n"
         IL_0010: ldarg.1
         IL_0011: dup
         IL_0012: brtrue =>  Label1
         IL_0017: pop
         IL_0018: ldnull
         IL_0019: br =>      Label2
         IL_001E: Label1
         IL_001E: callvirt   virtual System.String System.Object::ToString()
         IL_0023: Label2
         IL_0023: call       static System.String System.String::Concat(System.String str0, System.String str1)
         IL_0028: ldc.i4.1
         IL_0029: call       static System.Void Elements.Core.UniLog::Warning(System.String message, System.Boolean stackTrace)
         */

        return codeMatcher.Instructions();
    }
}