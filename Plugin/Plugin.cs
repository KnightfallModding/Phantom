using MelonLoader;
using Phantom;

[assembly:
  MelonInfo(
    typeof(Plugin),
    PluginInfo.NAME,
    PluginInfo.VERSION,
    "Aeryle",
    "https://github.com/KnightfallModding/Phantom"
  )]
[assembly: MelonColor(1, 16, 206, 168)]
[assembly: MelonAuthorColor(1, 206, 16, 70)]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace Phantom;

internal class Plugin : MelonPlugin
{
  public override void OnInitializeMelon()
  {
    LoggerInstance.Msg($"{PluginInfo.NAME} loaded correctly.");
  }
}
