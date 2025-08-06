using MelonLoader;

namespace Phantom;

public static class PhantomLogger
{
  private static MelonLogger.Instance? _logger;

  internal static void Initialize(MelonLogger.Instance logger)
  {
    _logger = logger;
  }

  public static void Msg(string message)
  {
    _logger?.Msg(message);
  }

  public static void Warning(string message)
  {
    _logger?.Warning(message);
  }

  public static void Error(string message)
  {
    _logger?.Error(message);
  }
}
