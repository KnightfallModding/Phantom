using System;
using DearImGuiInjection.BepInEx;
using DearImGuiInjection.CppInterop;
using DearImGuiInjection.RendererFinder.Windows;
using Il2CppInterop.Runtime;
using MelonLoader.NativeUtils;
using Silk.NET.Core.Native;
using SilkD3D11 = Silk.NET.Direct3D11;
using SilkDXGI = Silk.NET.DXGI;

namespace DearImGuiInjection.RendererFinder.Renderers;

/// <summary>
///     Contains a full list of IDXGISwapChain functions to be used
///     as an indexer into the SwapChain Virtual Function Table entries.
/// </summary>
public enum IDXGISwapChain
{
    // IUnknown
    QueryInterface = 0,
    AddRef = 1,
    Release = 2,

    // IDXGIObject
    SetPrivateData = 3,
    SetPrivateDataInterface = 4,
    GetPrivateData = 5,
    GetParent = 6,

    // IDXGIDeviceSubObject
    GetDevice = 7,

    // IDXGISwapChain
    Present = 8,
    GetBuffer = 9,
    SetFullscreenState = 10,
    GetFullscreenState = 11,
    GetDesc = 12,
    ResizeBuffers = 13,
    ResizeTarget = 14,
    GetContainingOutput = 15,
    GetFrameStatistics = 16,
    GetLastPresentCount = 17,
}

public class DX11Renderer : IRenderer
{
    private static readonly CDXGISwapChainPresentDelegate _swapChainPresentHookDelegate =
        SwapChainPresentHook;

    private static NativeHook<CDXGISwapChainPresentDelegate> _swapChainPresentHook;
    private static Action<ComPtr<SilkDXGI.IDXGISwapChain>, uint, uint> _onPresentAction;

    private static readonly CDXGISwapChainResizeBuffersDelegate _swapChainResizeBuffersHookDelegate =
        SwapChainResizeBuffersHook;

    private static NativeHook<CDXGISwapChainResizeBuffersDelegate> _swapChainResizeBuffersHook;

    private static Action<
        ComPtr<SilkDXGI.IDXGISwapChain>,
        uint,
        uint,
        uint,
        SilkDXGI.Format,
        uint
    > _preResizeBuffers;

    private static Action<
        ComPtr<SilkDXGI.IDXGISwapChain>,
        uint,
        uint,
        uint,
        SilkDXGI.Format,
        uint
    > _postResizeBuffers;

    public unsafe bool Init()
    {
        Log.Info("DX11Renderer.Init()");

        var windowHandle = User32.CreateFakeWindow();

        var dxgiFactory = SilkDXGI.DXGI.GetApi(null);
        var d3d11 = SilkD3D11.D3D11.GetApi(null);

        SilkDXGI.SwapChainDesc desc = new()
        {
            BufferDesc = new SilkDXGI.ModeDesc
            {
                Width = 500,
                Height = 300,
                RefreshRate = new SilkDXGI.Rational { Numerator = 60, Denominator = 1 },
                Format = SilkDXGI.Format.FormatR8G8B8A8Unorm,
            },
            SampleDesc = new SilkDXGI.SampleDesc { Count = 1, Quality = 0 },
            BufferUsage = SilkDXGI.DXGI.UsageRenderTargetOutput,
            BufferCount = 1,
            OutputWindow = windowHandle,
            Windowed = 1,
        };

        ComPtr<SilkD3D11.ID3D11Device> device = default;
        ComPtr<SilkDXGI.IDXGISwapChain> swapChain = default;
        d3d11.CreateDeviceAndSwapChain(
            null,
            D3DDriverType.Hardware,
            0,
            0,
            null,
            0,
            SilkD3D11.D3D11.SdkVersion,
            &desc,
            swapChain.GetAddressOf(),
            device.GetAddressOf(),
            null,
            null
        );

        var swapChainVTable = VirtualFunctionTable.FromObject(
            (nuint)(nint)swapChain.Handle,
            (nuint)Enum.GetNames(typeof(IDXGISwapChain)).Length
        );
        var swapChainPresentFunctionPtr = swapChainVTable
            .TableEntries[(int)IDXGISwapChain.Present]
            .FunctionPointer;
        var swapChainResizeBuffersFunctionPtr = swapChainVTable
            .TableEntries[(int)IDXGISwapChain.ResizeBuffers]
            .FunctionPointer;

        swapChain.Dispose();
        device.Dispose();

        User32.DestroyWindow(windowHandle);

        _swapChainPresentHook = INativeDetour.CreateAndApply(
            (nint)swapChainPresentFunctionPtr,
            _swapChainPresentHookDelegate
        );

        _swapChainResizeBuffersHook = INativeDetour.CreateAndApply(
            (nint)swapChainResizeBuffersFunctionPtr,
            _swapChainResizeBuffersHookDelegate
        );

        Log.Info("DX11Renderer.Init() end");

        return true;
    }

    public void Dispose()
    {
        _swapChainResizeBuffersHook?.Detach();
        _swapChainResizeBuffersHook = null;

        _swapChainPresentHook?.Detach();
        _swapChainPresentHook = null;

        _onPresentAction = null;
    }

    public static void AttachThread()
    {
        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
    }

    public static event Action<ComPtr<SilkDXGI.IDXGISwapChain>, uint, uint> OnPresent
    {
        add => _onPresentAction += value;
        remove => _onPresentAction -= value;
    }

    public static event Action<
        ComPtr<SilkDXGI.IDXGISwapChain>,
        uint,
        uint,
        uint,
        SilkDXGI.Format,
        uint
    > PreResizeBuffers
    {
        add => _preResizeBuffers += value;
        remove => _preResizeBuffers -= value;
    }

    public static event Action<
        ComPtr<SilkDXGI.IDXGISwapChain>,
        uint,
        uint,
        uint,
        SilkDXGI.Format,
        uint
    > PostResizeBuffers
    {
        add => _postResizeBuffers += value;
        remove => _postResizeBuffers -= value;
    }

    private static unsafe IntPtr SwapChainPresentHook(IntPtr self, uint syncInterval, uint flags)
    {
        AttachThread();

        var swapChain = new ComPtr<SilkDXGI.IDXGISwapChain>((SilkDXGI.IDXGISwapChain*)self);

        if (_onPresentAction != null)
        {
            foreach (var @delegate in _onPresentAction.GetInvocationList())
            {
                var item = (Action<ComPtr<SilkDXGI.IDXGISwapChain>, uint, uint>)@delegate;
                try
                {
                    item(swapChain, syncInterval, flags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        return _swapChainPresentHook.Trampoline(self, syncInterval, flags);
    }

    private static unsafe IntPtr SwapChainResizeBuffersHook(
        IntPtr swapchainPtr,
        uint bufferCount,
        uint width,
        uint height,
        SilkDXGI.Format newFormat,
        uint swapchainFlags
    )
    {
        AttachThread();

        var swapChain = new ComPtr<SilkDXGI.IDXGISwapChain>((SilkDXGI.IDXGISwapChain*)swapchainPtr);

        if (_preResizeBuffers != null)
        {
            foreach (
                Action<
                    ComPtr<SilkDXGI.IDXGISwapChain>,
                    uint,
                    uint,
                    uint,
                    SilkDXGI.Format,
                    uint
                > item in _preResizeBuffers.GetInvocationList()
            )
                try
                {
                    item(swapChain, bufferCount, width, height, newFormat, swapchainFlags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
        }

        var result = _swapChainResizeBuffersHook.Trampoline(
            swapchainPtr,
            bufferCount,
            width,
            height,
            newFormat,
            swapchainFlags
        );

        if (_postResizeBuffers != null)
        {
            foreach (
                Action<
                    ComPtr<SilkDXGI.IDXGISwapChain>,
                    uint,
                    uint,
                    uint,
                    SilkDXGI.Format,
                    uint
                > item in _postResizeBuffers.GetInvocationList()
            )
                try
                {
                    item(swapChain, bufferCount, width, height, newFormat, swapchainFlags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
        }

        return result;
    }

    // SwapChainPresent hook
    private delegate IntPtr CDXGISwapChainPresentDelegate(
        IntPtr self,
        uint syncInterval,
        uint flags
    );

    // SwapChainResizeBuffer hook
    private delegate IntPtr CDXGISwapChainResizeBuffersDelegate(
        IntPtr self,
        uint bufferCount,
        uint width,
        uint height,
        SilkDXGI.Format newFormat,
        uint swapchainFlags
    );
}
