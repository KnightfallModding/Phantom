using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DearImGuiInjection;
using HarmonyLib;
using HexaGen.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Phantom.GUI;
using Phantom.MonoBehaviours;

namespace Phantom;

[BepInPlugin(PhantomInfo.Id, PhantomInfo.Name, PhantomInfo.Version)]
public class PhantomPlugin : BasePlugin
{
    // ReSharper disable once NullableWarningSuppressionIsUsed
    public static ManualLogSource Logger { get; private set; } = null!;

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static new T AddComponent<T>()
        where T : Il2CppObjectBase
    {
        return IL2CPPChainloader.AddUnityComponent<T>();
    }

    // public static AddComponent

    public override void Load()
    {
        Logger = Log;
        Logger.LogInfo("Initializing Phantom Plugin");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        SetupOverlay();
        AddComponent<QuitHandler>();
    }

    private static void SetupOverlay()
    {
        // `Hexa.NET.ImGui` won't find `cimgui` without specifying the path
        var cimguiPath = Path.Join(Paths.PluginPath, "Libraries");
        LibraryLoader.CustomLoadFolders.Add(cimguiPath);

        var overlay = new Overlay();
        var imGuiConfigPath = Paths.GameRootPath;
        var assetsFolder = Path.Join(Paths.PluginPath, PhantomInfo.Name, "Assets");
        ImGuiInjector.Init(imGuiConfigPath, assetsFolder, Logger);
        ImGuiInjector.Render += overlay.Render;
    }
}
