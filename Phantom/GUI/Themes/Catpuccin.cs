using System.Numerics;
using Hexa.NET.ImGui;

namespace Phantom.GUI.Themes;

public class Catpuccin : ITheme
{
    public string Name => "Catpuccin";

    public void Setup()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        var moccha = new Vector4(0.117f, 0.117f, 0.172f, 1.0f); // #1e1e2e
        var mantle = new Vector4(0.109f, 0.109f, 0.156f, 1.0f); // #181825
        var surface0 = new Vector4(0.200f, 0.207f, 0.286f, 1.0f); // #313244
        var surface1 = new Vector4(0.247f, 0.254f, 0.337f, 1.0f); // #3f4056
        var surface2 = new Vector4(0.290f, 0.301f, 0.388f, 1.0f); // #4a4d63
        var overlay0 = new Vector4(0.396f, 0.403f, 0.486f, 1.0f); // #65677c
        var overlay2 = new Vector4(0.576f, 0.584f, 0.654f, 1.0f); // #9399b2
        var text = new Vector4(0.803f, 0.815f, 0.878f, 1.0f); // #cdd6f4
        var subtext0 = new Vector4(0.639f, 0.658f, 0.764f, 1.0f); // #a3a8c3
        var mauve = new Vector4(0.796f, 0.698f, 0.972f, 1.0f); // #cba6f7
        var peach = new Vector4(0.980f, 0.709f, 0.572f, 1.0f); // #fab387
        var yellow = new Vector4(0.980f, 0.913f, 0.596f, 1.0f); // #f9e2af
        var green = new Vector4(0.650f, 0.890f, 0.631f, 1.0f); // #a6e3a1
        var teal = new Vector4(0.580f, 0.886f, 0.819f, 1.0f); // #94e2d5
        var sapphire = new Vector4(0.458f, 0.784f, 0.878f, 1.0f); // #74c7ec
        var blue = new Vector4(0.533f, 0.698f, 0.976f, 1.0f); // #89b4fa
        var lavender = new Vector4(0.709f, 0.764f, 0.980f, 1.0f); // #b4befe

        // Main window and backgrounds
        colors[(int)ImGuiCol.WindowBg] = moccha;
        colors[(int)ImGuiCol.ChildBg] = moccha;
        colors[(int)ImGuiCol.PopupBg] = surface0;
        colors[(int)ImGuiCol.Border] = surface1;
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        colors[(int)ImGuiCol.FrameBg] = surface0;
        colors[(int)ImGuiCol.FrameBgHovered] = surface1;
        colors[(int)ImGuiCol.FrameBgActive] = surface2;
        colors[(int)ImGuiCol.TitleBg] = mantle;
        colors[(int)ImGuiCol.TitleBgActive] = surface0;
        colors[(int)ImGuiCol.TitleBgCollapsed] = mantle;
        colors[(int)ImGuiCol.MenuBarBg] = mantle;
        colors[(int)ImGuiCol.ScrollbarBg] = surface0;
        colors[(int)ImGuiCol.ScrollbarGrab] = surface2;
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = overlay0;
        colors[(int)ImGuiCol.ScrollbarGrabActive] = overlay2;
        colors[(int)ImGuiCol.CheckMark] = green;
        colors[(int)ImGuiCol.SliderGrab] = sapphire;
        colors[(int)ImGuiCol.SliderGrabActive] = blue;
        colors[(int)ImGuiCol.Button] = surface0;
        colors[(int)ImGuiCol.ButtonHovered] = surface1;
        colors[(int)ImGuiCol.ButtonActive] = surface2;
        colors[(int)ImGuiCol.Header] = surface0;
        colors[(int)ImGuiCol.HeaderHovered] = surface1;
        colors[(int)ImGuiCol.HeaderActive] = surface2;
        colors[(int)ImGuiCol.Separator] = surface1;
        colors[(int)ImGuiCol.SeparatorHovered] = mauve;
        colors[(int)ImGuiCol.SeparatorActive] = mauve;
        colors[(int)ImGuiCol.ResizeGrip] = surface2;
        colors[(int)ImGuiCol.ResizeGripHovered] = mauve;
        colors[(int)ImGuiCol.ResizeGripActive] = mauve;
        colors[(int)ImGuiCol.Tab] = surface0;
        colors[(int)ImGuiCol.TabHovered] = surface2;
        colors[(int)ImGuiCol.TabSelected] = surface1;
        colors[(int)ImGuiCol.TabDimmed] = surface0;
        colors[(int)ImGuiCol.TabDimmedSelected] = surface1;
        colors[(int)ImGuiCol.DockingPreview] = sapphire;
        colors[(int)ImGuiCol.DockingEmptyBg] = moccha;
        colors[(int)ImGuiCol.PlotLines] = blue;
        colors[(int)ImGuiCol.PlotLinesHovered] = peach;
        colors[(int)ImGuiCol.PlotHistogram] = teal;
        colors[(int)ImGuiCol.PlotHistogramHovered] = green;
        colors[(int)ImGuiCol.TableHeaderBg] = surface0;
        colors[(int)ImGuiCol.TableBorderStrong] = surface1;
        colors[(int)ImGuiCol.TableBorderLight] = surface0;
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.06f);
        colors[(int)ImGuiCol.TextSelectedBg] = surface2;
        colors[(int)ImGuiCol.DragDropTarget] = yellow;
        colors[(int)ImGuiCol.NavCursor] = lavender;
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.7f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.2f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.35f);
        colors[(int)ImGuiCol.Text] = text;
        colors[(int)ImGuiCol.TextDisabled] = subtext0;

        // Rounded corners
        style.WindowRounding = 6.0f;
        style.ChildRounding = 6.0f;
        style.FrameRounding = 4.0f;
        style.PopupRounding = 4.0f;
        style.ScrollbarRounding = 9.0f;
        style.GrabRounding = 4.0f;
        style.TabRounding = 4.0f;

        // Padding and spacing
        style.WindowPadding = new Vector2(8.0f, 8.0f);
        style.FramePadding = new Vector2(5.0f, 3.0f);
        style.ItemSpacing = new Vector2(8.0f, 4.0f);
        style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
        style.IndentSpacing = 21.0f;
        style.ScrollbarSize = 14.0f;
        style.GrabMinSize = 10.0f;

        // Borders
        style.WindowBorderSize = 1.0f;
        style.ChildBorderSize = 1.0f;
        style.PopupBorderSize = 1.0f;
        style.FrameBorderSize = 0.0f;
        style.TabBorderSize = 0.0f;
    }
}
