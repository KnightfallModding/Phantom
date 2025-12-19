using System;
using System.Runtime.InteropServices;
using DearImGuiInjection.RendererFinder.Renderers;
using DearImGuiInjection.Windows;
using Hexa.NET.ImGui;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D12.Device;
using Resource = SharpDX.Direct3D12.Resource;

namespace DearImGuiInjection.Backends;

internal static class ImGuiDX12
{
    private const int GWL_WNDPROC = -4;
    private static IntPtr _windowHandle;

    private static FrameContext[] _frameContexts;
    private static GraphicsCommandList _commandList;
    private static DescriptorHeap _shaderDescriptorHeap;
    private static CommandQueue _commandQueue;

    private static User32.WndProcDelegate _myWindowProc;
    private static IntPtr _originalWindowProc;

    internal static void Init()
    {
        Log.Info("ImGuiDX12.Init");

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

        Log.Info("ImGui.ImGuiImplDX12Shutdown()");
        ImGuiDX12Impl.Shutdown();

        _windowHandle = IntPtr.Zero;
    }

    private static bool RetrieveCommandQueue(CommandQueue commandQueue, uint arg2, IntPtr ptr)
    {
        if (commandQueue.Description.Type == CommandListType.Direct)
        {
            Log.Info("Retrieved the command queue.");
            _commandQueue = commandQueue;
            return true;
        }

        return false;
    }

    private static void InitImGui(
        SwapChain1 swapChain,
        uint syncInterval,
        uint flags,
        IntPtr presentParameters
    )
    {
        var windowHandle = swapChain.Description.OutputHandle;

        if (!ImGuiInjector.Initialized)
        {
            Log.Info("DearImGuiInjection.InitImGui()");
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

            Log.Info($"ImGuiImplWin32Init, Window Handle: {windowHandle:X}");
            ImGuiWin32Impl.Init(_windowHandle);

            _myWindowProc = WndProcHandler;
            _originalWindowProc = User32.SetWindowLong(
                windowHandle,
                GWL_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_myWindowProc)
            );
        }
    }

    private static unsafe void InitImGuiDX12(SwapChain swapChain)
    {
        using var device = InitImGuiDX12Internal(swapChain, out var bufferCount);

        ImGuiDX12Impl.Init(
            (void*)device.NativePointer,
            bufferCount,
            Format.R8G8B8A8_UNorm,
            (void*)_shaderDescriptorHeap.NativePointer,
            _shaderDescriptorHeap.CPUDescriptorHandleForHeapStart,
            _shaderDescriptorHeap.GPUDescriptorHandleForHeapStart
        );

        Log.Info("InitImGuiDX12 Finished.");
    }

    private static Device InitImGuiDX12Internal(SwapChain swapChain, out int bufferCount)
    {
        var device = swapChain.GetDevice<Device>();
        var swapChainDescription = swapChain.Description;

        bufferCount = swapChainDescription.BufferCount;
        _shaderDescriptorHeap = device.CreateDescriptorHeap(
            new DescriptorHeapDescription
            {
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                DescriptorCount = bufferCount,
                Flags = DescriptorHeapFlags.ShaderVisible,
            }
        );

        _frameContexts = new FrameContext[bufferCount];
        for (var i = 0; i < bufferCount; i++)
        {
            _frameContexts[i] = new FrameContext
            {
                CommandAllocator = device.CreateCommandAllocator(CommandListType.Direct),
            };
        }

        _commandList = device.CreateCommandList(
            CommandListType.Direct,
            _frameContexts[0].CommandAllocator,
            null
        );
        _commandList.Close();

        var descriptorBackBuffer = device.CreateDescriptorHeap(
            new DescriptorHeapDescription
            {
                Type = DescriptorHeapType.RenderTargetView,
                DescriptorCount = bufferCount,
                Flags = DescriptorHeapFlags.None,
                NodeMask = 1,
            }
        );

        var rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(
            DescriptorHeapType.RenderTargetView
        );
        var rtvHandle = descriptorBackBuffer.CPUDescriptorHandleForHeapStart;

        for (var i = 0; i < bufferCount; i++)
        {
            var backBuffer = swapChain.GetBackBuffer<Resource>(i);

            _frameContexts[i].MainRenderTargetDescriptor = rtvHandle;
            _frameContexts[i].MainRenderTargetResource = backBuffer;

            device.CreateRenderTargetView(backBuffer, null, rtvHandle);
            rtvHandle.Ptr += rtvDescriptorSize;
        }

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

    private static void RenderImGui(
        SwapChain3 swapChain,
        uint syncInterval,
        uint flags,
        IntPtr presentParameters
    )
    {
        if (_commandQueue == null)
        {
            return;
        }

        var windowHandle = swapChain.Description.OutputHandle;

        if (!IsTargetWindowHandle(windowHandle))
        {
            Log.Info($"[DX12] Discarding window handle {windowHandle:X} due to mismatch");
            return;
        }

        var currentFrameContext = _frameContexts[swapChain.CurrentBackBufferIndex];
        if (currentFrameContext.MainRenderTargetResource == null)
        {
            return;
        }

        ImGuiDX12Impl.NewFrame();

        NewFrame();

        currentFrameContext.CommandAllocator.Reset();

        const int D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES = unchecked((int)0xffffffff);
        var barrier = new ResourceBarrier
        {
            Type = ResourceBarrierType.Transition,
            Flags = ResourceBarrierFlags.None,
            Transition = new ResourceTransitionBarrier(
                currentFrameContext.MainRenderTargetResource,
                D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES,
                ResourceStates.Present,
                ResourceStates.RenderTarget
            ),
        };

        _commandList.Reset(currentFrameContext.CommandAllocator, null);
        _commandList.ResourceBarrier(barrier);
        _commandList.SetRenderTargets(currentFrameContext.MainRenderTargetDescriptor, null);
        _commandList.SetDescriptorHeaps(_shaderDescriptorHeap);

        var drawData = ImGui.GetDrawData();
        ImGuiDX12Impl.RenderDrawData(drawData, _commandList);

        barrier.Transition = new ResourceTransitionBarrier(
            currentFrameContext.MainRenderTargetResource,
            D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES,
            ResourceStates.RenderTarget,
            ResourceStates.Present
        );

        _commandList.ResourceBarrier(barrier);
        _commandList.Close();

        _commandQueue.ExecuteCommandList(_commandList);
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

        /*if ((DearImGuiInjection.IO.ConfigFlags & (int)ImGuiConfigFlags.ViewportsEnable) > 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault(IntPtr.Zero, IntPtr.Zero);
        }*/
    }

    private static void PreResizeBuffers(
        SwapChain3 swapChain,
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

        var windowHandle = swapChain.Description.OutputHandle;

        if (!IsTargetWindowHandle(windowHandle))
        {
            Log.Info(
                $"[DX12 ResizeBuffers] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        foreach (var frameContext in _frameContexts)
        {
            frameContext.MainRenderTargetResource?.Dispose();
            frameContext.MainRenderTargetResource = null;
        }
    }

    private static void PostResizeBuffers(
        SwapChain3 swapChain,
        int bufferCount,
        int width,
        int height,
        int newFormat,
        int swapchainFlags
    )
    {
        var windowHandle = swapChain.Description.OutputHandle;

        if (!IsTargetWindowHandle(windowHandle))
        {
            Log.Info(
                $"[DX12 ResizeBuffers] Discarding window handle {windowHandle:X} due to mismatch"
            );
            return;
        }

        var device = swapChain.GetDevice<Device>();
        for (var i = 0; i < swapChain.Description.BufferCount; i++)
        {
            var backBuffer = swapChain.GetBackBuffer<Resource>(i);
            device.CreateRenderTargetView(
                backBuffer,
                null,
                _frameContexts[i].MainRenderTargetDescriptor
            );
            _frameContexts[i].MainRenderTargetResource = backBuffer;
        }
    }

    internal class FrameContext
    {
        internal CommandAllocator CommandAllocator;
        internal CpuDescriptorHandle MainRenderTargetDescriptor;
        internal Resource MainRenderTargetResource;
    }
}
