using System.Numerics;
using Hexa.NET.ImGui;

namespace Phantom.GUI.Themes;

public class Modern : ITheme
{
    // Base colors
    private static readonly Vector4 BackgroundDark = new(0.067f, 0.067f, 0.082f, 1.0f); // #111115
    private static readonly Vector4 BackgroundMid = new(0.098f, 0.098f, 0.118f, 1.0f); // #191920
    private static readonly Vector4 BackgroundLight = new(0.137f, 0.137f, 0.165f, 1.0f); // #23232a
    private static readonly Vector4 BackgroundLighter = new(0.180f, 0.180f, 0.216f, 1.0f); // #2e2e37

    // Accent colors (modern blue-violet gradient feel)
    private static readonly Vector4 AccentPrimary = new(0.416f, 0.467f, 0.996f, 1.0f); // #6a77fe
    private static readonly Vector4 AccentHover = new(0.518f, 0.561f, 1.0f, 1.0f); // #848fff
    private static readonly Vector4 AccentActive = new(0.329f, 0.380f, 0.918f, 1.0f); // #5461ea
    private static readonly Vector4 AccentSubtle = new(0.416f, 0.467f, 0.996f, 0.15f); // Accent with low alpha

    // Text colors
    private static readonly Vector4 TextPrimary = new(0.957f, 0.957f, 0.973f, 1.0f); // #f4f4f8
    private static readonly Vector4 TextSecondary = new(0.647f, 0.655f, 0.710f, 1.0f); // #a5a7b5
    private static readonly Vector4 TextDisabled = new(0.447f, 0.455f, 0.510f, 1.0f); // #727482

    // Semantic colors
    private static readonly Vector4 Success = new(0.298f, 0.788f, 0.529f, 1.0f); // #4cc987
    private static readonly Vector4 Warning = new(0.988f, 0.729f, 0.271f, 1.0f); // #fcba45
    private static readonly Vector4 Error = new(0.937f, 0.369f, 0.369f, 1.0f); // #ef5e5e

    // Border and separator
    private static readonly Vector4 Border = new(0.220f, 0.224f, 0.267f, 1.0f); // #383944
    private static readonly Vector4 BorderLight = new(0.280f, 0.286f, 0.337f, 1.0f); // #474956

    public string Name => "Modern";

    public void Setup()
    {
        Apply();
        AccentGold();
    }

    /// <summary>
    ///     Applies the modern theme to ImGui.
    ///     Call this once after ImGui context initialization.
    /// </summary>
    public static void Apply()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        // ═══════════════════════════════════════════════════════════════════════
        // STYLE SETTINGS - Modern spacing and rounding
        // ═══════════════════════════════════════════════════════════════════════

        // Window
        style.WindowPadding = new Vector2(16, 16);
        style.WindowRounding = 12.0f;
        style.WindowBorderSize = 1.0f;
        style.WindowMinSize = new Vector2(100, 100);
        style.WindowTitleAlign = new Vector2(0.5f, 0.5f); // Center title

        // Frame (buttons, inputs, etc.)
        style.FramePadding = new Vector2(12, 8);
        style.FrameRounding = 8.0f;
        style.FrameBorderSize = 0.0f;

        // Items
        style.ItemSpacing = new Vector2(10, 10);
        style.ItemInnerSpacing = new Vector2(8, 8);
        style.IndentSpacing = 24.0f;

        // Touch/Click extra padding
        style.TouchExtraPadding = new Vector2(0, 0);

        // Widgets
        style.GrabMinSize = 14.0f;
        style.GrabRounding = 6.0f;
        style.ScrollbarSize = 14.0f;
        style.ScrollbarRounding = 7.0f;

        // Tabs
        style.TabRounding = 8.0f;
        style.TabBorderSize = 0.0f;
        style.TabCloseButtonMinWidthSelected = 0.0f;
        style.TabCloseButtonMinWidthUnselected = 0.0f;
        style.TabBarBorderSize = 1.0f;

        // Child windows
        style.ChildRounding = 10.0f;
        style.ChildBorderSize = 1.0f;

        // Popup
        style.PopupRounding = 10.0f;
        style.PopupBorderSize = 1.0f;

        // Separators
        style.SeparatorTextBorderSize = 2.0f;
        style.SeparatorTextAlign = new Vector2(0.0f, 0.5f);
        style.SeparatorTextPadding = new Vector2(20, 4);

        // Cell padding (tables)
        style.CellPadding = new Vector2(10, 6);

        // Misc
        style.Alpha = 1.0f;
        style.DisabledAlpha = 0.5f;
        style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
        style.SelectableTextAlign = new Vector2(0.0f, 0.5f);

        // Anti-aliasing
        style.AntiAliasedLines = true;
        style.AntiAliasedLinesUseTex = true;
        style.AntiAliasedFill = true;

        // ═══════════════════════════════════════════════════════════════════════
        // COLORS - Modern dark theme with accent highlights
        // ═══════════════════════════════════════════════════════════════════════

        // Text
        colors[(int)ImGuiCol.Text] = TextPrimary;
        colors[(int)ImGuiCol.TextDisabled] = TextDisabled;

        // Windows
        colors[(int)ImGuiCol.WindowBg] = BackgroundMid;
        colors[(int)ImGuiCol.ChildBg] = BackgroundDark;
        colors[(int)ImGuiCol.PopupBg] = BackgroundMid;

        // Borders
        colors[(int)ImGuiCol.Border] = Border;
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0);

        // Frame backgrounds (inputs, combo boxes, etc.)
        colors[(int)ImGuiCol.FrameBg] = BackgroundLight;
        colors[(int)ImGuiCol.FrameBgHovered] = BackgroundLighter;
        colors[(int)ImGuiCol.FrameBgActive] = BackgroundLighter;

        // Title bar
        colors[(int)ImGuiCol.TitleBg] = BackgroundDark;
        colors[(int)ImGuiCol.TitleBgActive] = BackgroundDark;
        colors[(int)ImGuiCol.TitleBgCollapsed] = BackgroundDark;

        // Menu bar
        colors[(int)ImGuiCol.MenuBarBg] = BackgroundDark;

        // Scrollbar
        colors[(int)ImGuiCol.ScrollbarBg] = BackgroundDark;
        colors[(int)ImGuiCol.ScrollbarGrab] = BackgroundLighter;
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = BorderLight;
        colors[(int)ImGuiCol.ScrollbarGrabActive] = AccentPrimary;

        // Checkmark
        colors[(int)ImGuiCol.CheckMark] = AccentPrimary;

        // Slider
        colors[(int)ImGuiCol.SliderGrab] = AccentPrimary;
        colors[(int)ImGuiCol.SliderGrabActive] = AccentActive;

        // Buttons
        colors[(int)ImGuiCol.Button] = AccentPrimary;
        colors[(int)ImGuiCol.ButtonHovered] = AccentHover;
        colors[(int)ImGuiCol.ButtonActive] = AccentActive;

        // Headers (collapsing headers, tree nodes, selectable items)
        colors[(int)ImGuiCol.Header] = AccentSubtle;
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(
            AccentPrimary.X,
            AccentPrimary.Y,
            AccentPrimary.Z,
            0.35f
        );
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(
            AccentPrimary.X,
            AccentPrimary.Y,
            AccentPrimary.Z,
            0.50f
        );

        // Separator
        colors[(int)ImGuiCol.Separator] = Border;
        colors[(int)ImGuiCol.SeparatorHovered] = AccentPrimary;
        colors[(int)ImGuiCol.SeparatorActive] = AccentActive;

        // Resize grip
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(
            AccentPrimary.X,
            AccentPrimary.Y,
            AccentPrimary.Z,
            0.25f
        );
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(
            AccentPrimary.X,
            AccentPrimary.Y,
            AccentPrimary.Z,
            0.67f
        );
        colors[(int)ImGuiCol.ResizeGripActive] = AccentPrimary;

        // Tabs
        colors[(int)ImGuiCol.Tab] = BackgroundLight;
        colors[(int)ImGuiCol.TabHovered] = AccentPrimary;
        colors[(int)ImGuiCol.TabSelected] = AccentPrimary;
        colors[(int)ImGuiCol.TabSelectedOverline] = AccentPrimary;
        colors[(int)ImGuiCol.TabDimmed] = BackgroundDark;
        colors[(int)ImGuiCol.TabDimmedSelected] = BackgroundLight;
        colors[(int)ImGuiCol.TabDimmedSelectedOverline] = Border;

        // Docking
        colors[(int)ImGuiCol.DockingPreview] = new Vector4(
            AccentPrimary.X,
            AccentPrimary.Y,
            AccentPrimary.Z,
            0.7f
        );
        colors[(int)ImGuiCol.DockingEmptyBg] = BackgroundDark;

        // Plot
        colors[(int)ImGuiCol.PlotLines] = AccentPrimary;
        colors[(int)ImGuiCol.PlotLinesHovered] = AccentHover;
        colors[(int)ImGuiCol.PlotHistogram] = AccentPrimary;
        colors[(int)ImGuiCol.PlotHistogramHovered] = AccentHover;

        // Tables
        colors[(int)ImGuiCol.TableHeaderBg] = BackgroundLight;
        colors[(int)ImGuiCol.TableBorderStrong] = Border;
        colors[(int)ImGuiCol.TableBorderLight] = new Vector4(Border.X, Border.Y, Border.Z, 0.5f);
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0, 0, 0, 0);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1, 1, 1, 0.02f);

        // Text input
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(
            AccentPrimary.X,
            AccentPrimary.Y,
            AccentPrimary.Z,
            0.35f
        );

        // Drag and drop
        colors[(int)ImGuiCol.DragDropTarget] = AccentPrimary;

        // Nav (keyboard/gamepad navigation)
        colors[(int)ImGuiCol.NavCursor] = AccentPrimary;
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1, 1, 1, 0.7f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.2f);

        // Modal window dim background
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0, 0, 0, 0.6f);
    }

    /// <summary>
    ///     Applies a variant with a different accent color.
    ///     Call after Apply() to change just the accent.
    /// </summary>
    public static void SetAccentColor(Vector4 primary, Vector4 hover, Vector4 active)
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;
        var subtle = new Vector4(primary.X, primary.Y, primary.Z, 0.15f);

        // Update all accent-dependent colors
        colors[(int)ImGuiCol.CheckMark] = primary;
        colors[(int)ImGuiCol.SliderGrab] = primary;
        colors[(int)ImGuiCol.SliderGrabActive] = active;
        colors[(int)ImGuiCol.Button] = primary;
        colors[(int)ImGuiCol.ButtonHovered] = hover;
        colors[(int)ImGuiCol.ButtonActive] = active;
        colors[(int)ImGuiCol.Header] = subtle;
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(primary.X, primary.Y, primary.Z, 0.35f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(primary.X, primary.Y, primary.Z, 0.50f);
        colors[(int)ImGuiCol.SeparatorHovered] = primary;
        colors[(int)ImGuiCol.SeparatorActive] = active;
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(primary.X, primary.Y, primary.Z, 0.25f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(
            primary.X,
            primary.Y,
            primary.Z,
            0.67f
        );
        colors[(int)ImGuiCol.ResizeGripActive] = primary;
        colors[(int)ImGuiCol.TabHovered] = primary;
        colors[(int)ImGuiCol.TabSelected] = primary;
        colors[(int)ImGuiCol.TabSelectedOverline] = primary;
        colors[(int)ImGuiCol.ScrollbarGrabActive] = primary;
        colors[(int)ImGuiCol.DockingPreview] = new Vector4(primary.X, primary.Y, primary.Z, 0.7f);
        colors[(int)ImGuiCol.PlotLines] = primary;
        colors[(int)ImGuiCol.PlotLinesHovered] = hover;
        colors[(int)ImGuiCol.PlotHistogram] = primary;
        colors[(int)ImGuiCol.PlotHistogramHovered] = hover;
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(primary.X, primary.Y, primary.Z, 0.35f);
        colors[(int)ImGuiCol.DragDropTarget] = primary;
        colors[(int)ImGuiCol.NavCursor] = primary;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRESET ACCENT COLORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Ocean Blue accent</summary>
    public static void AccentOcean()
    {
        SetAccentColor(
            new Vector4(0.255f, 0.588f, 0.996f, 1.0f), // #4196fe
            new Vector4(0.369f, 0.675f, 1.0f, 1.0f), // #5eacff
            new Vector4(0.180f, 0.494f, 0.890f, 1.0f) // #2e7ee3
        );
    }

    /// <summary>Emerald Green accent</summary>
    public static void AccentEmerald()
    {
        SetAccentColor(
            new Vector4(0.200f, 0.780f, 0.545f, 1.0f), // #33c78b
            new Vector4(0.298f, 0.855f, 0.627f, 1.0f), // #4cdaa0
            new Vector4(0.141f, 0.678f, 0.467f, 1.0f) // #24ad77
        );
    }

    /// <summary>Sunset Orange accent</summary>
    public static void AccentSunset()
    {
        SetAccentColor(
            new Vector4(0.988f, 0.486f, 0.298f, 1.0f), // #fc7c4c
            new Vector4(1.0f, 0.588f, 0.416f, 1.0f), // #ff966a
            new Vector4(0.906f, 0.392f, 0.212f, 1.0f) // #e76436
        );
    }

    /// <summary>Rose Pink accent</summary>
    public static void AccentRose()
    {
        SetAccentColor(
            new Vector4(0.925f, 0.353f, 0.549f, 1.0f), // #ec5a8c
            new Vector4(0.969f, 0.471f, 0.639f, 1.0f), // #f778a3
            new Vector4(0.843f, 0.267f, 0.463f, 1.0f) // #d74476
        );
    }

    /// <summary>Purple Violet accent (default)</summary>
    public static void AccentViolet()
    {
        SetAccentColor(
            new Vector4(0.416f, 0.467f, 0.996f, 1.0f), // #6a77fe
            new Vector4(0.518f, 0.561f, 1.0f, 1.0f), // #848fff
            new Vector4(0.329f, 0.380f, 0.918f, 1.0f) // #5461ea
        );
    }

    /// <summary>Gold Yellow accent</summary>
    public static void AccentGold()
    {
        SetAccentColor(
            new Vector4(0.965f, 0.761f, 0.251f, 1.0f), // #f6c240
            new Vector4(0.988f, 0.827f, 0.380f, 1.0f), // #fcd361
            new Vector4(0.878f, 0.678f, 0.169f, 1.0f) // #e0ad2b
        );
    }

    public static void PushGradientButtonStyle(Vector4 baseColor)
    {
        var lighter = new Vector4(
            Math.Min(1.0f, baseColor.X * 1.15f),
            Math.Min(1.0f, baseColor.Y * 1.15f),
            Math.Min(1.0f, baseColor.Z * 1.15f),
            baseColor.W
        );
        var darker = new Vector4(
            baseColor.X * 0.85f,
            baseColor.Y * 0.85f,
            baseColor.Z * 0.85f,
            baseColor.W
        );

        ImGui.PushStyleColor(ImGuiCol.Button, baseColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, lighter);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, darker);
    }

    /// <summary>
    ///     Creates a "ghost" button style (transparent with border on hover).
    ///     Call PopStyleColor(3) and PopStyleVar(1) after the button.
    /// </summary>
    public static void PushGhostButtonStyle(Vector4 textColor)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(
            ImGuiCol.ButtonHovered,
            new Vector4(textColor.X, textColor.Y, textColor.Z, 0.1f)
        );
        ImGui.PushStyleColor(
            ImGuiCol.ButtonActive,
            new Vector4(textColor.X, textColor.Y, textColor.Z, 0.2f)
        );
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
    }

    /// <summary>
    ///     Pushes style for a "danger" action (red tones).
    ///     Call PopStyleColor(3) after the element.
    /// </summary>
    public static void PushDangerStyle()
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Error);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.969f, 0.471f, 0.471f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.843f, 0.282f, 0.282f, 1.0f));
    }

    /// <summary>
    ///     Pushes style for a "success" action (green tones).
    ///     Call PopStyleColor(3) after the element.
    /// </summary>
    public static void PushSuccessStyle()
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Success);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.400f, 0.855f, 0.612f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.224f, 0.702f, 0.455f, 1.0f));
    }

    /// <summary>
    ///     Helper to create consistent card-like child windows.
    /// </summary>
    public static bool BeginCard(string id, Vector2 size = default)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 12.0f);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, BackgroundLight);
        ImGui.PushStyleColor(ImGuiCol.Border, Border);

        return ImGui.BeginChild(id, size, ImGuiChildFlags.Borders);
    }

    /// <summary>
    ///     End a card created with BeginCard.
    /// </summary>
    public static void EndCard()
    {
        ImGui.EndChild();
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Renders a styled section header with separator.
    /// </summary>
    public static void SectionHeader(string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, TextSecondary);
        ImGui.TextUnformatted(text.ToUpperInvariant());
        ImGui.PopStyleColor();
        ImGui.Spacing();
    }

    /// <summary>
    ///     Adds vertical spacing (useful for section breaks).
    /// </summary>
    public static void VerticalSpace(float height = 16.0f)
    {
        ImGui.Dummy(new Vector2(0, height));
    }
}
