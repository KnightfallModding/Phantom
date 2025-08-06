using MelonLoader;
using Phantom;
using Phantom.Loader;

[assembly:
  MelonInfo(
    typeof(Plugin),
    PluginInfo.Name,
    PluginInfo.Version,
    "Aeryle",
    "https://github.com/KnightfallModding/Phantom"
  )]
[assembly: MelonColor(1, 16, 206, 168)]
[assembly: MelonAuthorColor(1, 206, 16, 70)]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace Phantom;

internal class Plugin : MelonPlugin
{
  public static MelonLogger.Instance Logger = null!;

  public override void OnInitializeMelon()
  {
    Logger = LoggerInstance;
    PhantomLogger.Initialize(LoggerInstance);
    LoggerInstance.Msg($"{PluginInfo.Name} loaded correctly.");

    PluginManager.LoadAllPlugins();
    PluginManager.PrintPluginInfo();
  }

  public override void OnDeinitializeMelon()
  {
    PluginManager.UnloadAllPlugins();
  }
}
