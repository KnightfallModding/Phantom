using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.D3D11;
using Silk.NET.Core.Native;
using SilkD3D11 = Silk.NET.Direct3D11;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace DearImGuiInjection.Backends;

public static class ImGuiDX11Impl
{
    private static ComPtr<SilkD3D11.ID3D11Device> _device;
    private static ComPtr<SilkD3D11.ID3D11DeviceContext> _deviceContext;

    // Render function - delegates to built-in backend
    public static void RenderDrawData(ImDrawDataPtr draw_data)
    {
        ImGuiImplD3D11.RenderDrawData(draw_data);
    }

    public static void NewFrame()
    {
        ImGuiImplD3D11.NewFrame();
    }

    internal static unsafe void Init(void* device, void* deviceContext)
    {
        _device = new ComPtr<SilkD3D11.ID3D11Device>((SilkD3D11.ID3D11Device*)device);
        _deviceContext = new ComPtr<SilkD3D11.ID3D11DeviceContext>(
            (SilkD3D11.ID3D11DeviceContext*)deviceContext
        );

        ImGuiImplD3D11.SetCurrentContext(ImGui.GetCurrentContext());
        if (
            !ImGuiImplD3D11.Init(
                Unsafe.As<ComPtr<SilkD3D11.ID3D11Device>, ID3D11DevicePtr>(ref _device),
                Unsafe.As<ComPtr<SilkD3D11.ID3D11DeviceContext>, ID3D11DeviceContextPtr>(
                    ref _deviceContext
                )
            )
        )
        {
            Log.Error("Failed to init ImGui Impl D3D11");
        }
    }

    public static void Shutdown()
    {
        ImGuiImplD3D11.Shutdown();
        ImGuiImplD3D11.SetCurrentContext(ImGuiContextPtr.Null);

        _device.Dispose();
        _deviceContext.Dispose();
    }

    public static void InvalidateDeviceObjects()
    {
        // The built-in backend handles this automatically during shutdown
    }

    public static void CreateDeviceObjects()
    {
        // The built-in backend handles this automatically
    }

    public static void CreateFontsTexture()
    {
        // The built-in backend handles this automatically during Init
    }
}
