namespace Phantom.Loader;

public static class PluginManager
{
  public static void LoadAllPlugins()
  {
    Finder.LoadAllPlugins();
  }

  public static void UnloadAllPlugins()
  {
    Finder.UnloadAllPlugins();
  }

  public static IBasePlugin? GetPlugin(string id)
  {
    return PluginRegistry.GetPlugin(id);
  }

  public static PluginMetadata? GetPluginMetadata(string id)
  {
    return PluginRegistry.GetPluginMetadata(id);
  }

  public static IEnumerable<string> GetAllPluginIds()
  {
    return PluginRegistry.GetAllPluginIds();
  }

  private static IEnumerable<IBasePlugin> GetAllPlugins()
  {
    return PluginRegistry.GetAllPlugins();
  }

  public static void PrintPluginInfo()
  {
    var plugins = GetAllPlugins().ToList();
    PhantomLogger.Msg($"Loaded Plugins ({plugins.Count}):");

    foreach (var plugin in plugins)
    {
      var metadata = plugin.GetMetadata();
      PhantomLogger.Msg($"  - {plugin.PluginId}: {metadata.Name} v{metadata.Version} by {metadata.Author}");
      if (!string.IsNullOrEmpty(metadata.Description)) PhantomLogger.Msg($"    {metadata.Description}");
    }
  }

  public static int GetPluginCount()
  {
    return PluginRegistry.Count;
  }
}
