using Phantom.Loader;
using Phantom.Plugins.Cheats;

namespace Phantom.Cheats;

public class Plugin : IBasePlugin
{
  public string PluginId => "phantom.plugin.cheats";

  public PluginMetadata GetMetadata()
  {
    return new PluginMetadata(
      CheatsInfo.Name,
      CheatsInfo.Version,
      "Aeryle",
      "Core cheats plugin for Phantom"
    );
  }

  public void LoadPlugin()
  {
    PhantomLogger.Msg("Cheats plugin loaded!");
  }

  public void UnloadPlugin()
  {
    PhantomLogger.Msg("Cheats plugin unloaded!");
  }
}
