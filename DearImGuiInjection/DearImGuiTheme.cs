using System.Numerics;
using Hexa.NET.ImGui;

namespace DearImGuiInjection;

public static class DearImGuiTheme
{
    private static readonly Vector4 BGColor = new(0.117f, 0.113f, 0.172f, .9375f);
    private static readonly Vector4 Primary = new(0.172f, 0.380f, 0.909f, 1f);
    private static readonly Vector4 Secondary = new(0.443f, 0.654f, 0.819f, 1f);
    private static readonly Vector4 WhiteBroken = new(0.792f, 0.784f, 0.827f, 1f);
    public static ImGuiStylePtr Style { get; private set; }

    internal static void Init()
    {
        // Custom theme provided not used yet
        // SetupStyle();

        SetupCustomFont();
    }

    private static void SetupStyle()
    {
        Style = ImGui.GetStyle();
        Style.WindowPadding = new Vector2(10f, 10f);
        Style.PopupRounding = 0f;
        Style.FramePadding = new Vector2(8f, 4f);
        Style.ItemSpacing = new Vector2(10f, 8f);
        Style.ItemInnerSpacing = new Vector2(6f, 6f);
        Style.TouchExtraPadding = new Vector2(0f, 0f);
        Style.IndentSpacing = 21f;
        Style.ScrollbarSize = 15f;
        Style.GrabMinSize = 8f;
        Style.WindowBorderSize = 0f;
        Style.ChildBorderSize = 1f;
        Style.PopupBorderSize = 0f;
        Style.FrameBorderSize = 0f;
        Style.TabBorderSize = 0f;
        Style.WindowRounding = 5f;
        Style.ChildRounding = 2f;
        Style.FrameRounding = 3f;
        Style.ScrollbarRounding = 3f;
        Style.GrabRounding = 0f;
        Style.TabRounding = 3f;
        Style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
        Style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
        Style.DisplaySafeAreaPadding = new Vector2(3f, 3f);

        var colors = Style.Colors;
        colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(1.00f, 0.90f, 0.19f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] = BGColor;
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.20f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.30f, 0.30f, 0.30f, 0.50f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.21f, 0.21f, 0.21f, 0.54f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.21f, 0.21f, 0.21f, 0.78f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.28f, 0.27f, 0.27f, 0.54f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.17f, 0.17f, 0.17f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg] = colors[(int)ImGuiCol.WindowBg];
        colors[(int)ImGuiCol.ScrollbarGrab] = Primary;
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = Secondary;
        colors[(int)ImGuiCol.ScrollbarGrabActive] = Primary;
        colors[(int)ImGuiCol.CheckMark] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 1.00f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.39f, 0.38f, 0.38f, 1.00f);
        colors[(int)ImGuiCol.Button] = Primary;
        colors[(int)ImGuiCol.ButtonHovered] = Secondary;
        colors[(int)ImGuiCol.ButtonActive] = colors[(int)ImGuiCol.ButtonHovered];
        colors[(int)ImGuiCol.Header] = new Vector4(0.37f, 0.37f, 0.37f, 0.31f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.38f, 0.38f, 0.38f, 0.37f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.37f, 0.37f, 0.37f, 0.51f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.38f, 0.38f, 0.38f, 0.50f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.46f, 0.46f, 0.46f, 0.50f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.46f, 0.46f, 0.46f, 0.64f);
        colors[(int)ImGuiCol.ResizeGrip] = WhiteBroken;
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1f, 1f, 1f, 1.00f);
        colors[(int)ImGuiCol.ResizeGripActive] = WhiteBroken;
        colors[(int)ImGuiCol.Tab] = new Vector4(0.21f, 0.21f, 0.21f, 0.86f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.27f, 0.27f, 0.27f, 0.86f);
        colors[(int)ImGuiCol.TabSelected] = new Vector4(0.34f, 0.34f, 0.34f, 0.86f);
        colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.10f, 0.10f, 0.10f, 0.97f);
        colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int)ImGuiCol.NavCursor] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
    }

    private static void SetupCustomFont()
    {
        var config = ImGui.ImFontConfig();
        config.MergeMode = false;
        ImGui.GetIO().Fonts.AddFontDefault();
        config.MergeMode = true;
        ImGui.GetIO().Fonts.Build();
    }
}
