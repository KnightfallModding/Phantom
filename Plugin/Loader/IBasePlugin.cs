namespace Phantom.Loader;

public interface IBasePlugin
{
  string PluginId { get; }

  PluginMetadata GetMetadata();
  void LoadPlugin();
  void UnloadPlugin();
}
