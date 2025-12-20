using System.Diagnostics;
using Hexa.NET.ImGui;

namespace Phantom.GUI;

public class Overlay
{
    private readonly Stopwatch _stopwatch;
    private float _delayMs;
    private int _fpsLimit;

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
        {
            Thread.Sleep((int)(remainingMs - 2));
        }

        // Spin-wait for precise timing
        while (_stopwatch.Elapsed.TotalMilliseconds < _delayMs)
        {
            Thread.SpinWait(1);
        }

        _stopwatch.Restart();
    }

    public void Render()
    {
        ThrottleFps();

        ImGui.ShowDemoWindow();
    }
}
