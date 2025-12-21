using DearImGuiInjection;
using HexaGen.Runtime;
using MelonLoader;
using MelonLoader.Utils;
using Phantom;
using Phantom.GUI;

[assembly: MelonGame("Landfall Games", "Knightfall")]
[assembly: MelonInfo(
    typeof(PhantomPlugin),
    PhantomInfo.Name,
    PhantomInfo.Version,
    PhantomInfo.Description
)]

namespace Phantom;

public class PhantomPlugin : MelonPlugin
{
    // ReSharper disable once NullableWarningSuppressionIsUsed
    public static MelonLogger.Instance Logger { get; private set; } = null!;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("Initializing Phantom Plugin");

        // DearImGuiInjection setup
        SetupOverlay();
    }

    private static void SetupOverlay()
    {
        // TODO: Replace `Paths` with `MelonEnvironments`
        // `Hexa.NET.ImGui` won't find `cimgui` without specifying the path
        var cimguiPath = Path.Join(MelonEnvironment.PluginsDirectory, "Libraries");
        LibraryLoader.CustomLoadFolders.Add(cimguiPath);

        var overlay = new Overlay();
        // TODO: Replace `Paths` with `MelonEnvironments`
        var imGuiConfigPath = MelonEnvironment.GameRootDirectory;
        var assetsFolder = Path.Join(MelonEnvironment.PluginsDirectory, PhantomInfo.Name, "Assets");
        ImGuiInjector.Init(imGuiConfigPath, assetsFolder, Logger);
        ImGuiInjector.Render += overlay.Render;
    }
}
