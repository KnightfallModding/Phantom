using System.Collections.Concurrent;

namespace Phantom.Loader;

public static class PluginRegistry
{
  private static readonly ConcurrentDictionary<string, IBasePlugin> RegisteredPlugins = new();
  private static readonly ConcurrentDictionary<string, PluginMetadata> PluginMetadata = new();

  public static int Count => RegisteredPlugins.Count;

  public static bool RegisterPlugin(string id, IBasePlugin plugin, PluginMetadata? metadata = null)
  {
    if (RegisteredPlugins.TryAdd(id, plugin))
    {
      if (metadata is not null) PluginMetadata.TryAdd(id, metadata);
      Plugin.Logger.Msg($"Plugin registered: {id}");

      return true;
    }

    Plugin.Logger.Warning($"Plugin with ID '{id}' is already registered");

    return false;
  }

  public static bool UnregisterPlugin(string id)
  {
    var removed = RegisteredPlugins.TryRemove(id, out var plugin);
    if (!removed) return removed;

    PluginMetadata.TryRemove(id, out _);
    Plugin.Logger.Msg($"Plugin unregistered: {id}");

    return removed;
  }

  public static IBasePlugin? GetPlugin(string id)
  {
    RegisteredPlugins.TryGetValue(id, out var plugin);

    return plugin;
  }

  public static PluginMetadata? GetPluginMetadata(string id)
  {
    PluginMetadata.TryGetValue(id, out var metadata);
    return metadata;
  }

  public static IEnumerable<string> GetAllPluginIds()
  {
    return RegisteredPlugins.Keys.ToList();
  }

  public static IEnumerable<IBasePlugin> GetAllPlugins()
  {
    return RegisteredPlugins.Values.ToList();
  }

  public static void Clear()
  {
    RegisteredPlugins.Clear();
    PluginMetadata.Clear();
  }
}
