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

    public void Render()
    {
        // Render first
        ImGui.ShowDemoWindow();

        // Calculate how long to wait
        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        var targetMs = _delayMs;

        // Spin-wait for precision (or use a hybrid approach)
        while (_stopwatch.Elapsed.TotalMilliseconds < targetMs)
        {
            Thread.SpinWait(1);
        }

        _stopwatch.Restart();
    }
}
