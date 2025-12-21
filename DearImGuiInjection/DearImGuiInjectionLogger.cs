using MelonLoader;

namespace DearImGuiInjection;

internal static class DearImGuiInjectionLogger
{
    private static MelonLogger.Instance _log;

    public static void Init(MelonLogger.Instance log) => _log = log;

    internal static void Debug(object data) => MelonDebug.Msg(data);

    internal static void Error(object data) => _log.Error(data);

    internal static void Fatal(object data) => _log.Error(data);

    internal static void Info(object data) => _log.Msg(data);

    internal static void Message(object data) => _log.Msg(data);

    internal static void Warning(object data) => _log.Warning(data);
}
