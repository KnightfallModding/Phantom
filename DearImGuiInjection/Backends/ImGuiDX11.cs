using System;
using System.Runtime.InteropServices;
using DearImGuiInjection.RendererFinder.Renderers;
using DearImGuiInjection.Windows;
using Hexa.NET.ImGui;
using Silk.NET.Core.Native;
using SilkD3D11 = Silk.NET.Direct3D11;
using SilkDXGI = Silk.NET.DXGI;

namespace DearImGuiInjection.Backends;

internal static class ImGuiDX11
{
    private const int GWL_WNDPROC = -4;
    private static IntPtr _windowHandle;

    private static ComPtr<SilkD3D11.ID3D11RenderTargetView> _renderTargetView;
    private static User32.WndProcDelegate _myWindowProc;
    private static IntPtr _originalWindowProc;

    private static ComPtr<SilkD3D11.ID3D11DeviceContext> _deviceContext;

    internal static void Init()
    {
        DX11Renderer.OnPresent += InitImGui;
        DX11Renderer.OnPresent += RenderImGui;
        DX11Renderer.PreResizeBuffers += PreResizeBuffers;
        DX11Renderer.PostResizeBuffers += PostResizeBuffers;
    }

    internal static void Dispose()
    {
        if (!ImGuiInjector.Initialized)
        {
            return;
        }

        DX11Renderer.PostResizeBuffers -= PostResizeBuffers;
        DX11Renderer.PreResizeBuffers -= PreResizeBuffers;
        DX11Renderer.OnPresent -= RenderImGui;

        User32.SetWindowLong(_windowHandle, GWL_WNDPROC, _originalWindowProc);

        ImGuiWin32Impl.Shutdown();

        _renderTargetView.Dispose();
        _renderTargetView = default;

        Log.Info("ImGui.ImGuiImplDX11Shutdown()");
        ImGuiDX11Impl.Shutdown();

        _windowHandle = IntPtr.Zero;
    }

    private static unsafe void InitImGui(
        ComPtr<SilkDXGI.IDXGISwapChain> swapChain,
        uint syncInterval,
        uint flags
    )
    {
        SilkDXGI.SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!ImGuiInjector.Initialized)
        {
            ImGuiInjector.InitImGui();
            InitImGuiWin32(windowHandle);
            InitImGuiDX11(swapChain);

            ImGuiInjector.Initialized = true;
        }

        DX11Renderer.OnPresent -= InitImGui;
    }

    private static void InitImGuiWin32(IntPtr windowHandle)
    {
        if (ImGuiInjector.Initialized)
        {
            return;
        }

        _windowHandle = windowHandle;
        if (_windowHandle == IntPtr.Zero)
        {
            return;
        }

        Log.Info($"ImGuiImplWin32Init, Window Handle: {windowHandle:X}");
        ImGuiWin32Impl.Init(_windowHandle);

        _myWindowProc = WndProcHandler;
        _originalWindowProc = User32.SetWindowLong(
            windowHandle,
            GWL_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(_myWindowProc)
        );
    }

    private static unsafe void InitImGuiDX11(ComPtr<SilkDXGI.IDXGISwapChain> swapChain)
    {
        var device = InitImGuiDX11Internal(swapChain);

        ImGuiDX11Impl.Init(device.Handle, _deviceContext.Handle);
    }

    private static unsafe ComPtr<SilkD3D11.ID3D11Device> InitImGuiDX11Internal(
        ComPtr<SilkDXGI.IDXGISwapChain> swapChain
    )
    {
        ComPtr<SilkD3D11.ID3D11Device> device = default;
        var deviceGuid = SilkD3D11.ID3D11Device.Guid;
        swapChain.Get().GetDevice(&deviceGuid, (void**)&device);

        device.Get().GetImmediateContext(_deviceContext.GetAddressOf());

        ComPtr<SilkD3D11.ID3D11Texture2D> backBuffer = default;
        fixed (Guid* guid = &SilkD3D11.ID3D11Device.Guid)
        {
            swapChain.Get().GetBuffer(0, guid, (void**)&backBuffer);
        }

        device
            .Get()
            .CreateRenderTargetView(
                (SilkD3D11.ID3D11Resource*)backBuffer.Handle,
                null,
                _renderTargetView.GetAddressOf()
            );
        backBuffer.Dispose();

        return device;
    }

    private static IntPtr WndProcHandler(
        IntPtr windowHandle,
        WindowMessage message,
        IntPtr wParam,
        IntPtr lParam
    )
    {
        if (
            ImGuiInjector.IsCursorVisible
            && ImGuiWin32Impl.WndProcHandler(windowHandle, message, wParam, lParam)
        )
        {
            return IntPtr.Zero;
        }

        return User32.CallWindowProc(_originalWindowProc, windowHandle, message, wParam, lParam);
    }

    private static unsafe void RenderImGui(
        ComPtr<SilkDXGI.IDXGISwapChain> swapChain,
        uint syncInterval,
        uint flags
    )
    {
        SilkDXGI.SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!IsTargetWindowHandle(windowHandle))
        {
            // Log.Info($"[DX11] Discarding window handle {windowHandle:X} due to mismatch");
            return;
        }

        ImGuiDX11Impl.NewFrame();

        NewFrame();

        _deviceContext.Get().OMSetRenderTargets(1, _renderTargetView.GetAddressOf(), null);

        var drawData = ImGui.GetDrawData();
        ImGuiDX11Impl.RenderDrawData(drawData);
    }

    private static bool IsTargetWindowHandle(IntPtr windowHandle)
    {
        if (windowHandle != IntPtr.Zero)
        {
            return windowHandle == _windowHandle || !ImGuiInjector.Initialized;
        }

        return false;
    }

    private static void NewFrame()
    {
        ImGuiWin32Impl.NewFrame();
        ImGui.NewFrame();

        if (ImGuiInjector.RenderAction != null)
        {
            foreach (Action item in ImGuiInjector.RenderAction.GetInvocationList())
            {
                try
                {
                    item();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        ImGui.EndFrame();
        ImGui.Render();

        /*if ((DearImGuiInjection.IO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) > 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault(IntPtr.Zero, IntPtr.Zero);
        }*/
    }

    private static unsafe void PreResizeBuffers(
        ComPtr<SilkDXGI.IDXGISwapChain> swapChain,
        uint bufferCount,
        uint width,
        uint height,
        SilkDXGI.Format newFormat,
        uint swapchainFlags
    )
    {
        if (!ImGuiInjector.Initialized)
        {
            return;
        }

        SilkDXGI.SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        Log.Info($"[DX11 ResizeBuffers] Window Handle {windowHandle:X}");

        if (!IsTargetWindowHandle(windowHandle))
        {
            Log.Info(
                $"[DX11 ResizeBuffers] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        _renderTargetView.Dispose();
        _renderTargetView = default;
        ImGuiDX11Impl.InvalidateDeviceObjects();
    }

    private static unsafe void PostResizeBuffers(
        ComPtr<SilkDXGI.IDXGISwapChain> swapChain,
        uint bufferCount,
        uint width,
        uint height,
        SilkDXGI.Format newFormat,
        uint swapchainFlags
    )
    {
        if (!ImGuiInjector.Initialized)
        {
            return;
        }

        SilkDXGI.SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!IsTargetWindowHandle(windowHandle))
        {
            Log.Info(
                $"[DX11 ResizeBuffers] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        ImGuiDX11Impl.CreateDeviceObjects();

        ComPtr<SilkD3D11.ID3D11Device> device = default;
        var deviceGuid = SilkD3D11.ID3D11Device.Guid;
        swapChain.Get().GetDevice(&deviceGuid, (void**)&device);

        ComPtr<SilkD3D11.ID3D11Texture2D> backBuffer = default;
        fixed (Guid* guid = &SilkD3D11.ID3D11Texture2D.Guid)
        {
            swapChain.Get().GetBuffer(0, guid, (void**)&backBuffer);
        }

        device
            .Get()
            .CreateRenderTargetView(
                (SilkD3D11.ID3D11Resource*)backBuffer.Handle,
                null,
                _renderTargetView.GetAddressOf()
            );
        backBuffer.Dispose();
        device.Dispose();
    }
}
