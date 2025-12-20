using System.Diagnostics;
using Hexa.NET.ImGui;

namespace Phantom.GUI;

public class Overlay
{
    private readonly float _delayMs;
    private readonly int _fpsLimit;
    private readonly Stopwatch _stopwatch;

    public Overlay(int fpsLimit = 60)
    {
        _fpsLimit = fpsLimit;
        _delayMs = 1000f / fpsLimit;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
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
        ImGui.ShowDemoWindow();

        ThrottleFps();
    }
}
