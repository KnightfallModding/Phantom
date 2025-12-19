using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Phantom.Patches.BepInEx.Logging;

[HarmonyPatch(typeof(LogEventArgs))]
public static class LogEventArgsPatch
{
    [HarmonyPatch(nameof(LogEventArgs.ToStringLine))]
    [HarmonyPrefix]
    public static bool ToStringLinePatch(LogEventArgs __instance, ref string __result)
    {
        __result = $"{__instance.Data}{Environment.NewLine}";
        File.WriteAllText(Path.Combine(Paths.GameRootPath, "test.txt"), __result);

        return false;
    }
}
