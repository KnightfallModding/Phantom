using System.Globalization;
using BepInEx;
using BepInEx.Logging;

namespace Phantom.Utils;

public static class ConsoleUtils
{
    public static void Log(LogLevel level, string source, string text)
    {
        DisplayInlineTimestamp();
        DisplayInlineSource(source);
        DisplayInlineText(level, text);
    }

    private static void SurroundWithBrackets(ConsoleColor color, string text)
    {
        ConsoleManager.SetConsoleColor(ConsoleColor.White);
        ConsoleManager.ConsoleStream.Write("[");
        ConsoleManager.SetConsoleColor(color);
        ConsoleManager.ConsoleStream.Write(text);
        ConsoleManager.SetConsoleColor(ConsoleColor.White);
        ConsoleManager.ConsoleStream.Write("]");
    }

    private static void DisplayInlineTimestamp()
    {
        SurroundWithBrackets(ConsoleColor.Green, DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture));
        ConsoleManager.ConsoleStream.Write(' ');
    }

    private static void DisplayInlineSource(string source)
    {
        SurroundWithBrackets(ConsoleColor.Magenta, source);
        ConsoleManager.ConsoleStream.Write(' ');
    }

    private static void DisplayInlineText(LogLevel level, string text)
    {
        ConsoleManager.SetConsoleColor(level.GetConsoleColor());
        ConsoleManager.ConsoleStream.Write(text);
    }
}
