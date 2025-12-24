using System.Diagnostics;
using System.Numerics;
using Hexa.NET.ImGui;
using MelonLoader.Utils;
using Phantom.GUI.Themes;

namespace Phantom.GUI;

public class Overlay
{
    private readonly Stopwatch _easterEggstopWatch = new();
    private readonly Stopwatch _stopwatch;
    private float _delayMs;
    private int _fpsLimit;
    private bool _initialized;
    private ImGuiIOPtr _io;
#pragma warning disable CA1859 // Impossible to know what theme will be used.
    // ReSharper disable once NullableWarningSuppressionIsUsed
    private ITheme _theme = null!;
#pragma warning restore CA1859

    public ImFontPtr MergedFont;
    public ImFontPtr NormalFont;

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

        NormalFont = _io.Fonts.AddFontFromFileTTF(normalFontPath, 16);
        MergedFont = _io.Fonts.AddFontFromFileTTF(emojisFontPath, 16, fontConfig);

        _io.ConfigDpiScaleFonts = true;
        _io.ConfigDpiScaleViewports = true;
        _io.FontDefault = MergedFont;
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

        ImGui.PushFont(MergedFont, 16);
        ImGui.ShowDemoWindow();
        if (ImGui.Button($"Toggle 30-60 FPS (Currently {FPSLimit}##ToggleFPS"))
            FPSLimit = FPSLimit == 60 ? 30 : 60;
        ImGui.Text("There is a secret in the main menu. Can you find it?");
        CreateEasterEgg();
        ImGui.PopFont();
    }

    private void CreateEasterEgg()
    {
        ImGui.Begin(
            "EasterEgg",
            ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoSavedSettings
        );
        var button = ImGui.InvisibleButton(" ##EasterEgg", new Vector2(20, 20));
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Is something here?..");
            ImGui.EndTooltip();
        }

        var elapsedMs = _easterEggstopWatch.Elapsed.TotalMilliseconds;
        if (elapsedMs is > 0 and < 200)
        {
            ImGui.Begin(
                "EasterEggFound",
                ImGuiWindowFlags.AlwaysAutoResize
                    | ImGuiWindowFlags.NoCollapse
                    | ImGuiWindowFlags.NoResize
                    | ImGuiWindowFlags.NoSavedSettings
            );
            ImGui.Text("YOU FOUND THE EASTER EGG 🎉");
            ImGui.Text("For this you win... Nothing! But congrats!");
            ImGui.PushFont(null, 8);
            ImGui.Text(
                "Fine... You get... Me to congratulate you personally, if you can catch a screenshot of me!"
            );
            ImGui.PopFont();
            ImGui.End();
        }
        else
        {
            _easterEggstopWatch.Reset();
            _easterEggstopWatch.Stop();
        }

        if (button)
            _easterEggstopWatch.Restart();

        ImGui.End();
    }
}
