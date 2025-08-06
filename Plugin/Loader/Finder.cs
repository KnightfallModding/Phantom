using System.Reflection;
using MelonLoader.Utils;

namespace Phantom.Loader;

internal static class Finder
{
  private static readonly string PluginsPath = Path.Join(MelonEnvironment.PluginsDirectory, PluginInfo.Name, "plugins");
  private static readonly List<IBasePlugin> LoadedPlugins = [];

  private static List<string> ListAllFiles()
  {
    if (Directory.Exists(PluginsPath)) return Directory.GetFiles(PluginsPath).Where(f => f.EndsWith(".dll")).ToList();

    Directory.CreateDirectory(PluginsPath);
    Plugin.Logger.Msg($"Created plugins directory: {PluginsPath}");

    return [];
  }

  public static void LoadAllPlugins()
  {
    var files = ListAllFiles();
    Plugin.Logger.Msg($"Found {files.Count} plugin files");

    foreach (var file in files)
      try
      {
        LoadPlugin(file);
      }
      catch (Exception ex)
      {
        Plugin.Logger.Error($"Failed to load plugin from {file}: {ex.Message}");
      }

    Plugin.Logger.Msg($"Successfully loaded {LoadedPlugins.Count} plugins");
  }

  private static void LoadPlugin(string filePath)
  {
    Plugin.Logger.Msg($"Loading plugin: {Path.GetFileName(filePath)}");

    var assembly = Assembly.LoadFile(filePath);
    var pluginTypes = assembly.GetTypes()
      .Where(t => typeof(IBasePlugin).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
      .ToList();

    if (pluginTypes.Count == 0)
    {
      Plugin.Logger.Warning($"No IBasePlugin implementations found in {Path.GetFileName(filePath)}");

      return;
    }

    foreach (var pluginType in pluginTypes)
      try
      {
        var pluginInstance = (IBasePlugin)Activator.CreateInstance(pluginType)!;
        var metadata = pluginInstance.GetMetadata();

        if (PluginRegistry.RegisterPlugin(pluginInstance.PluginId, pluginInstance, metadata))
        {
          pluginInstance.LoadPlugin();
          LoadedPlugins.Add(pluginInstance);

          Plugin.Logger.Msg($"Successfully loaded plugin: {pluginInstance.PluginId} ({pluginType.FullName})");
        }
        else
        {
          Plugin.Logger.Warning($"Failed to register plugin with duplicate ID: {pluginInstance.PluginId}");
        }
      }
      catch (Exception ex)
      {
        Plugin.Logger.Error($"Failed to instantiate plugin {pluginType.FullName}: {ex.Message}");
      }
  }

  public static void UnloadAllPlugins()
  {
    Plugin.Logger.Msg($"Unloading {LoadedPlugins.Count} plugins");

    foreach (var plugin in LoadedPlugins)
      try
      {
        plugin.UnloadPlugin();
        PluginRegistry.UnregisterPlugin(plugin.PluginId);
      }
      catch (Exception ex)
      {
        Plugin.Logger.Error($"Error unloading plugin {plugin.PluginId}: {ex.Message}");
      }

    LoadedPlugins.Clear();
    PluginRegistry.Clear();
    Plugin.Logger.Msg("All plugins unloaded");
  }

  public static List<IBasePlugin> GetLoadedPlugins()
  {
    return LoadedPlugins.ToList();
  }
}
