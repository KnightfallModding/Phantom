namespace Phantom.Loader;

public class PluginMetadata(string name, string version = "1.0.0", string author = "", string description = "")
{
  public string Name { get; } = name;
  public string Version { get; } = version;
  public string Author { get; } = author;
  public string Description { get; } = description;
}
