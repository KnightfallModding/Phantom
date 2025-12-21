using System;
using System.Runtime.InteropServices;
using DearImGuiInjection.RendererFinder.Renderers;
using DearImGuiInjection.Windows;
using Hexa.NET.ImGui;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace DearImGuiInjection.Backends;

internal static class ImGuiDX12
{
    private const int GWL_WNDPROC = -4;
    private static IntPtr _windowHandle;

    private static FrameContext[] _frameContexts;
    private static ComPtr<ID3D12GraphicsCommandList> _commandList;
    private static ComPtr<ID3D12DescriptorHeap> _shaderDescriptorHeap;
    private static ComPtr<ID3D12CommandQueue> _commandQueue;

    private static User32.WndProcDelegate _myWindowProc;
    private static IntPtr _originalWindowProc;

    internal static void Init()
    {
        DearImGuiInjectionLogger.Info("ImGuiDX12.Init");

        DX12Renderer.OnPresent += InitImGui;

        DX12Renderer.OnPresent += RenderImGui;

        DX12Renderer.OnExecuteCommandList += RetrieveCommandQueue;

        DX12Renderer.PreResizeBuffers += PreResizeBuffers;
        DX12Renderer.PostResizeBuffers += PostResizeBuffers;
    }

    internal static void Dispose()
    {
        if (!ImGuiInjector.Initialized)
        {
            return;
        }

        DX12Renderer.PostResizeBuffers -= PostResizeBuffers;
        DX12Renderer.PreResizeBuffers -= PreResizeBuffers;

        DX12Renderer.OnExecuteCommandList -= RetrieveCommandQueue;
        DX12Renderer.OnPresent -= RenderImGui;

        User32.SetWindowLong(_windowHandle, GWL_WNDPROC, _originalWindowProc);

        ImGuiWin32Impl.Shutdown();

        DearImGuiInjectionLogger.Info("ImGui.ImGuiImplDX12Shutdown()");
        ImGuiDX12Impl.Shutdown();

        _windowHandle = IntPtr.Zero;
    }

    private static bool RetrieveCommandQueue(
        ComPtr<ID3D12CommandQueue> commandQueue,
        uint arg2,
        IntPtr ptr
    )
    {
        var desc = commandQueue.Get().GetDesc();

        if (desc.Type != CommandListType.Direct)
        {
            return false;
        }

        DearImGuiInjectionLogger.Info("Retrieved the command queue.");
        _commandQueue = commandQueue;
        return true;
    }

    private static unsafe void InitImGui(
        ComPtr<IDXGISwapChain3> swapChain,
        uint syncInterval,
        uint flags,
        IntPtr presentParameters
    )
    {
        SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!ImGuiInjector.Initialized)
        {
            DearImGuiInjectionLogger.Info("DearImGuiInjection.InitImGui()");
            ImGuiInjector.InitImGui();

            InitImGuiWin32(windowHandle);

            InitImGuiDX12(swapChain);

            ImGuiInjector.Initialized = true;
        }

        DX12Renderer.OnPresent -= InitImGui;
    }

    private static void InitImGuiWin32(IntPtr windowHandle)
    {
        if (!ImGuiInjector.Initialized)
        {
            _windowHandle = windowHandle;
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            DearImGuiInjectionLogger.Info($"ImGuiImplWin32Init, Window Handle: {windowHandle:X}");
            ImGuiWin32Impl.Init(_windowHandle);

            _myWindowProc = WndProcHandler;
            _originalWindowProc = User32.SetWindowLong(
                windowHandle,
                GWL_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_myWindowProc)
            );
        }
    }

    private static unsafe void InitImGuiDX12(ComPtr<IDXGISwapChain3> swapChain)
    {
        var device = InitImGuiDX12Internal(
            swapChain,
            out var bufferCount,
            out var cpuHandle,
            out var gpuHandle
        );

        ImGuiDX12Impl.Init(
            device.Handle,
            bufferCount,
            Format.FormatR8G8B8A8Unorm,
            _shaderDescriptorHeap.Handle,
            &cpuHandle,
            &gpuHandle
        );

        DearImGuiInjectionLogger.Info("InitImGuiDX12 Finished.");
    }

    private static unsafe ComPtr<ID3D12Device> InitImGuiDX12Internal(
        ComPtr<IDXGISwapChain3> swapChain,
        out uint bufferCount,
        out CpuDescriptorHandle cpuHandle,
        out GpuDescriptorHandle gpuHandle
    )
    {
        ComPtr<ID3D12Device> device = default;
        var deviceGuid = ID3D12Device.Guid;
        swapChain.Get().GetDevice(&deviceGuid, (void**)&device);

        SwapChainDesc swapChainDescription;
        swapChain.Get().GetDesc(&swapChainDescription);

        bufferCount = swapChainDescription.BufferCount;

        DescriptorHeapDesc heapDesc = new()
        {
            Type = DescriptorHeapType.CbvSrvUav,
            NumDescriptors = bufferCount,
            Flags = DescriptorHeapFlags.ShaderVisible,
        };
        fixed (Guid* guid = &ID3D12DescriptorHeap.Guid)
        {
            fixed (ComPtr<ID3D12DescriptorHeap>* shaderDescriptorHeap = &_shaderDescriptorHeap)
            {
                device.Get().CreateDescriptorHeap(&heapDesc, guid, (void**)shaderDescriptorHeap);
            }
        }

        _frameContexts = new FrameContext[bufferCount];
        for (var i = 0; i < bufferCount; i++)
        {
            _frameContexts[i] = new FrameContext();
            fixed (Guid* guid = &ID3D12CommandAllocator.Guid)
            {
                fixed (
                    ComPtr<ID3D12CommandAllocator>* commandAllocator = &_frameContexts[
                        i
                    ].CommandAllocator
                )
                {
                    device
                        .Get()
                        .CreateCommandAllocator(
                            CommandListType.Direct,
                            guid,
                            (void**)commandAllocator
                        );
                }
            }
        }

        fixed (Guid* guid = &ID3D12GraphicsCommandList.Guid)
        {
            fixed (ComPtr<ID3D12GraphicsCommandList>* commandList = &_commandList)
            {
                device
                    .Get()
                    .CreateCommandList(
                        0,
                        CommandListType.Direct,
                        _frameContexts[0].CommandAllocator.Handle,
                        null,
                        guid,
                        (void**)&commandList
                    );
            }
        }

        _commandList.Get().Close();

        ComPtr<ID3D12DescriptorHeap> descriptorBackBuffer = default;
        DescriptorHeapDesc rtvHeapDesc = new()
        {
            Type = DescriptorHeapType.Rtv,
            NumDescriptors = bufferCount,
            Flags = DescriptorHeapFlags.None,
            NodeMask = 1,
        };
        fixed (Guid* guid = &ID3D12DescriptorHeap.Guid)
        {
            device.Get().CreateDescriptorHeap(&rtvHeapDesc, guid, (void**)&descriptorBackBuffer);
        }

        var rtvDescriptorSize = device
            .Get()
            .GetDescriptorHandleIncrementSize(DescriptorHeapType.Rtv);
        var rtvHandle = descriptorBackBuffer.Get().GetCPUDescriptorHandleForHeapStart();

        for (var i = 0; i < bufferCount; i++)
        {
            ComPtr<ID3D12Resource> backBuffer = default;
            fixed (Guid* guid = &ID3D12Resource.Guid)
            {
                swapChain.Get().GetBuffer((uint)i, guid, (void**)&backBuffer);
            }

            _frameContexts[i].RenderTargetCpuDescriptors = rtvHandle;
            _frameContexts[i].RenderTarget = backBuffer;

            device.Get().CreateRenderTargetView(backBuffer.Handle, null, rtvHandle);
            rtvHandle.Ptr += rtvDescriptorSize;
        }

        cpuHandle = descriptorBackBuffer.Get().GetCPUDescriptorHandleForHeapStart();
        gpuHandle = _shaderDescriptorHeap.Get().GetGPUDescriptorHandleForHeapStart();

        descriptorBackBuffer.Dispose();

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
        ComPtr<IDXGISwapChain3> swapChain,
        uint syncInterval,
        uint flags,
        IntPtr presentParameters
    )
    {
        if (_commandQueue.Handle == null)
        {
            return;
        }

        SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!IsTargetWindowHandle(windowHandle))
        {
            DearImGuiInjectionLogger.Info(
                $"[DX12] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        var currentBackBufferIndex = swapChain.Get().GetCurrentBackBufferIndex();
        var currentFrameContext = _frameContexts[currentBackBufferIndex];
        if (currentFrameContext.RenderTarget.Handle == null)
        {
            return;
        }

        ImGuiDX12Impl.NewFrame();

        NewFrame();

        currentFrameContext.CommandAllocator.Get().Reset();

        const uint D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES = 0xffffffff;
        ResourceBarrier barrier = new()
        {
            Type = ResourceBarrierType.Transition,
            Flags = ResourceBarrierFlags.None,
        };
        barrier.Anonymous.Transition.PResource = currentFrameContext.RenderTarget.Handle;
        barrier.Anonymous.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
        barrier.Anonymous.Transition.StateBefore = (uint)ResourceStates.Present;
        barrier.Anonymous.Transition.StateAfter = ResourceStates.RenderTarget;

        fixed (
            CpuDescriptorHandle* cpuDescriptorHandle =
                &currentFrameContext.RenderTargetCpuDescriptors
        )
        {
            _commandList.Get().Reset(currentFrameContext.CommandAllocator.Handle, null);
            _commandList.Get().ResourceBarrier(1, &barrier);
            _commandList.Get().OMSetRenderTargets(1, cpuDescriptorHandle, 0, null);
        }

        var heaps = _shaderDescriptorHeap.Handle;
        _commandList.Get().SetDescriptorHeaps(1, &heaps);

        var drawData = ImGui.GetDrawData();
        ImGuiDX12Impl.RenderDrawData(drawData, _commandList);

        barrier.Anonymous.Transition.StateBefore = ResourceStates.RenderTarget;
        barrier.Anonymous.Transition.StateAfter = (uint)ResourceStates.Present;

        _commandList.Get().ResourceBarrier(1, &barrier);
        _commandList.Get().Close();

        var cmdList = (ID3D12CommandList*)_commandList.Handle;
        _commandQueue.Get().ExecuteCommandLists(1, &cmdList);
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
                    DearImGuiInjectionLogger.Error(e);
                }
            }
        }

        ImGui.EndFrame();

        ImGui.Render();

        /*if ((DearImGuiInjection.IO.ConfigFlags & (int)ImGuiConfigFlags.ViewportsEnable) > 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault(IntPtr.Zero, IntPtr.Zero);
        }*/
    }

    private static unsafe void PreResizeBuffers(
        ComPtr<IDXGISwapChain3> swapChain,
        int bufferCount,
        int width,
        int height,
        int newFormat,
        int swapchainFlags
    )
    {
        if (!ImGuiInjector.Initialized)
        {
            return;
        }

        SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!IsTargetWindowHandle(windowHandle))
        {
            DearImGuiInjectionLogger.Info(
                $"[DX12 ResizeBuffers] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        foreach (var frameContext in _frameContexts)
        {
            frameContext.RenderTarget.Dispose();
            frameContext.RenderTarget = default;
        }
    }

    private static unsafe void PostResizeBuffers(
        ComPtr<IDXGISwapChain3> swapChain,
        int bufferCount,
        int width,
        int height,
        int newFormat,
        int swapchainFlags
    )
    {
        SwapChainDesc desc;
        swapChain.Get().GetDesc(&desc);
        var windowHandle = desc.OutputWindow;

        if (!IsTargetWindowHandle(windowHandle))
        {
            DearImGuiInjectionLogger.Info(
                $"[DX12 ResizeBuffers] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        ComPtr<ID3D12Device> device = default;
        var deviceGuid = ID3D12Device.Guid;
        swapChain.Get().GetDevice(&deviceGuid, (void**)&device);

        for (var i = 0; i < desc.BufferCount; i++)
        {
            ComPtr<ID3D12Resource> backBuffer = default;
            fixed (Guid* guid = &ID3D12Resource.Guid)
            {
                swapChain.Get().GetBuffer((uint)i, guid, (void**)&backBuffer);
            }

            device
                .Get()
                .CreateRenderTargetView(
                    backBuffer.Handle,
                    null,
                    _frameContexts[i].RenderTargetCpuDescriptors
                );
            _frameContexts[i].RenderTarget = backBuffer;
        }

        device.Dispose();
    }

    private class FrameContext
    {
        public ComPtr<ID3D12CommandAllocator> CommandAllocator;
        public ComPtr<ID3D12Resource> RenderTarget;
        public CpuDescriptorHandle RenderTargetCpuDescriptors;
    }
}
