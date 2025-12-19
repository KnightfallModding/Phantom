using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

// [assembly: MelonGame("Landfall Games", "Knightfall")]
// [assembly: MelonInfo(
//     typeof(PhantomPlugin),
//     PhantomInfo.Name,
//     PhantomInfo.Version,
//     PhantomInfo.Author
// )]
// [assembly: MelonColor(1, 255, 102, 99)]

namespace Phantom;

[BepInPlugin(PhantomInfo.Id, PhantomInfo.Name, PhantomInfo.Version)]
public class PhantomPlugin : BasePlugin
{
    // ReSharper disable once NullableWarningSuppressionIsUsed
    public static ManualLogSource Logger { get; private set; } = null!;

    public override void Load()
    {
        Logger = Log;
        Logger.LogInfo("Initializing Phantom Plugin");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}
