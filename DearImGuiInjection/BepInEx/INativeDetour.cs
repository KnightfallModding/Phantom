using System;
using System.Runtime.InteropServices;
using MelonLoader.NativeUtils;
using MonoMod.RuntimeDetour;

namespace DearImGuiInjection.BepInEx;

public interface INativeDetour : IDetour
{
    public static NativeHook<T> CreateAndApply<T>(nint from, T to)
        where T : Delegate
    {
        var hook = new NativeHook<T>(from, Marshal.GetFunctionPointerForDelegate(to));
        hook.Attach();

        return hook;
    }
}
