using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.D3D12;
using Silk.NET.Core.Native;
using SilkD3D12 = Silk.NET.Direct3D12;
using SilkDXGI = Silk.NET.DXGI;

namespace DearImGuiInjection.Backends;

public static class ImGuiDX12Impl
{
    private static ComPtr<SilkD3D12.ID3D12Device> _device;
    private static ComPtr<SilkD3D12.ID3D12DescriptorHeap> _srvDescHeap;
    private static SilkDXGI.Format _rtvFormat;
    private static uint _numFramesInFlight;

    // Render function - delegates to built-in backend
    public static void RenderDrawData(
        ImDrawDataPtr draw_data,
        ComPtr<SilkD3D12.ID3D12GraphicsCommandList> commandList
    )
    {
        ImGuiImplD3D12.RenderDrawData(
            draw_data,
            Unsafe.As<ComPtr<SilkD3D12.ID3D12GraphicsCommandList>, ID3D12GraphicsCommandListPtr>(
                ref commandList
            )
        );
    }

    public static void CreateFontsTexture()
    {
        // The built-in backend handles this automatically during Init
    }

    public static bool CreateDeviceObjects()
    {
        // The built-in backend handles this automatically
        return true;
    }

    public static void InvalidateDeviceObjects()
    {
        // The built-in backend handles this automatically during shutdown
    }

    internal static unsafe void Init(
        void* device,
        uint numFramesInFlight,
        SilkDXGI.Format rtvFormat,
        void* cbvSrvHeap,
        void* fontSrvCpuDescHandle,
        void* fontSrvGpuDescHandle
    )
    {
        _device = new ComPtr<SilkD3D12.ID3D12Device>((SilkD3D12.ID3D12Device*)device);
        _srvDescHeap = new ComPtr<SilkD3D12.ID3D12DescriptorHeap>(
            (SilkD3D12.ID3D12DescriptorHeap*)cbvSrvHeap
        );
        _rtvFormat = rtvFormat;
        _numFramesInFlight = numFramesInFlight;

        ImGuiImplD3D12.SetCurrentContext(ImGui.GetCurrentContext());
        var info = new ImGuiImplDX12InitInfoPtr
        {
            Device = Unsafe.As<ComPtr<SilkD3D12.ID3D12Device>, ID3D12DevicePtr>(ref _device),
            NumFramesInFlight = (int)numFramesInFlight,
            RTVFormat = (int)rtvFormat,
            SrvDescriptorHeap = Unsafe.As<
                ComPtr<SilkD3D12.ID3D12DescriptorHeap>,
                ID3D12DescriptorHeapPtr
            >(ref _srvDescHeap),
        };

        if (!ImGuiImplD3D12.Init(info))
        {
            Log.Error("Failed to init ImGui Impl D3D12");
        }
    }

    internal static void Shutdown()
    {
        ImGuiImplD3D12.Shutdown();
        ImGuiImplD3D12.SetCurrentContext(ImGuiContextPtr.Null);

        _device.Dispose();
        _srvDescHeap.Dispose();
    }

    internal static void NewFrame()
    {
        ImGuiImplD3D12.NewFrame();
    }
}
