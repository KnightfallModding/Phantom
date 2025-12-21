using System.Diagnostics;
using Hexa.NET.ImGui;
using MelonLoader.Utils;
using Phantom.GUI.Themes;

namespace Phantom.GUI;

public class Overlay
{
    private readonly Stopwatch _stopwatch;
    private float _delayMs;
    private int _fpsLimit;
    private bool _initialized;
    private ImGuiIOPtr _io;
#pragma warning disable CA1859 // Impossible to know what theme will be used.
    private ITheme _theme;
#pragma warning restore CA1859

    public ImFontPtr mergedFont;
    public ImFontPtr normalFont;

    public Overlay(int fpsLimit = 60)
    {
        _fpsLimit = fpsLimit;
        _delayMs = 1000f / fpsLimit;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public int FPSLimit
    {
        get => _fpsLimit;
        set
        {
            _fpsLimit = value;
            _delayMs = 1000f / value;
            _stopwatch.Restart();
        }
    }

    private void ThrottleFps()
    {
        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        var remainingMs = _delayMs - elapsedMs;

        // Sleep for the bulk of the wait (minus a buffer for imprecision)
        if (remainingMs > 2)
            Thread.Sleep((int)(remainingMs - 2));

        // Spin-wait for precise timing
        while (_stopwatch.Elapsed.TotalMilliseconds < _delayMs)
            Thread.SpinWait(1);

        _stopwatch.Restart();
    }

    public unsafe void LoadFonts()
    {
        var fontsPath = Path.Join(MelonEnvironment.PluginsDirectory, "Phantom", "Fonts");
        var normalFontPath = Path.Join(fontsPath, "Comfortaa-Medium.ttf");
        var emojisFontPath = Path.Join(fontsPath, "Twemoji.ttf");

        var fontConfig = ImGui.ImFontConfig();
        fontConfig.MergeMode = true;
        fontConfig.FontLoaderFlags = (uint)ImGuiFreeTypeLoaderFlags.LoadColor;

        normalFont = _io.Fonts.AddFontFromFileTTF(normalFontPath, 16);
        mergedFont = _io.Fonts.AddFontFromFileTTF(emojisFontPath, 16, fontConfig);

        _io.ConfigDpiScaleFonts = true;
        _io.ConfigDpiScaleViewports = true;
        _io.FontDefault = mergedFont;
    }

    public void LoadDefaultTheme()
    {
        // TODO: Use config to dynamically load saved theme
        _theme = new Modern();
        _theme.Setup();
    }

    public void Init()
    {
        _io = ImGui.GetIO();
        LoadFonts();
        LoadDefaultTheme();
        _initialized = true;
    }

    public void Render()
    {
        ThrottleFps();

        if (!_initialized)
            Init();

        ImGui.PushFont(mergedFont, 16);
        ImGui.ShowDemoWindow();
        ImGui.Text("Test: 😀");
        if (ImGui.Button($"Toggle 30-60 FPS (Currently {FPSLimit}##ToggleFPS"))
            FPSLimit = FPSLimit == 60 ? 30 : 60;
        ImGui.PopFont();
    }
}
