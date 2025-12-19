using BepInEx.Logging;
using HarmonyLib;
using Phantom.Utils;

namespace Phantom.Patches.BepInEx.Logging;

[HarmonyPatch(typeof(ConsoleLogListener))]
internal static class ConsoleLogListenerPatch
{
    [HarmonyPatch(nameof(ConsoleLogListener.LogEvent))]
    [HarmonyPrefix]
    internal static bool LogEvent(LogEventArgs eventArgs)
    {
        ConsoleUtils.Log(eventArgs.Level, eventArgs.Source.SourceName, eventArgs.ToStringLine());

        return false;
    }
}
