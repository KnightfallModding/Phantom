using System;
using System.Runtime.InteropServices;

namespace DearImGuiInjection.Windows;

public static class DwmApi
{
    [Flags]
    public enum DwmBb
    {
        Enable = 1,
        BlurRegion = 2,
        TransitionMaximized = 4,
    }

    [DllImport("DwmApi.dll")]
    public static extern int DwmGetColorizationColor(
        out uint colorizationColor,
        [MarshalAs(UnmanagedType.Bool)] out bool colorizationOpaqueBlend
    );

    [DllImport("DwmApi.dll")]
    public static extern int DwmIsCompositionEnabled(out bool enabled);

    [DllImport("DwmApi.dll")]
    public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DwmBlurBehind blurBehind);

    [StructLayout(LayoutKind.Sequential)]
    public struct DwmBlurBehind(bool enabled)
    {
        public DwmBb dwFlags = DwmBb.Enable;
        public bool fEnable = enabled;
        public IntPtr hRgnBlur = IntPtr.Zero;
        public bool fTransitionOnMaximized = false;

        public bool TransitionOnMaximized
        {
            get => fTransitionOnMaximized;
            set
            {
                fTransitionOnMaximized = value;
                dwFlags |= DwmBb.TransitionMaximized;
            }
        }
    }
}
