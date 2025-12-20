using System;
using System.Linq;
using BepInEx.Unity.IL2CPP.Hook;
using DearImGuiInjection.CppInterop;
using DearImGuiInjection.RendererFinder.Windows;
using Il2CppInterop.Runtime;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace DearImGuiInjection.RendererFinder.Renderers;

public class DX12Renderer : IRenderer
{
    private static readonly CDXGISwapChainPresent1Delegate _swapchainPresentHookDelegate =
        SwapChainPresentHook;

    private static CDXGISwapChainPresent1Delegate _swapchainPresentHookOriginal;
    private static INativeDetour _swapChainPresentHook;
    private static Action<ComPtr<IDXGISwapChain3>, uint, uint, IntPtr> _onPresentAction;

    private static readonly CDXGISwapChainResizeBuffersDelegate _swapChainResizeBufferHookDelegate =
        SwapChainResizeBuffersHook;

    private static CDXGISwapChainResizeBuffersDelegate _swapChainResizeBufferHookOriginal;
    private static INativeDetour _swapChainResizeBufferHook;
    private static Action<ComPtr<IDXGISwapChain3>, int, int, int, int, int> _preResizeBuffers;
    private static Action<ComPtr<IDXGISwapChain3>, int, int, int, int, int> _postResizeBuffers;

    private static readonly CommandQueueExecuteCommandListDelegate _commandQueueExecuteCommandListHookDelegate =
        CommandQueueExecuteCommandListHook;

    private static CommandQueueExecuteCommandListDelegate _commandQueueExecuteCommandListHookOriginal;
    private static INativeDetour _commandQueueExecuteCommandListHook;
    private static readonly object _onExecuteCommandListActionLock = new();

    public unsafe bool Init()
    {
        var windowHandle = User32.CreateFakeWindow();

        var dxgi = DXGI.GetApi(null);
        var d3d12 = D3D12.GetApi();

        ComPtr<IDXGIFactory4> factory = default;
        fixed (Guid* guid = &IDXGIFactory4.Guid)
        {
            dxgi.CreateDXGIFactory2(0, guid, (void**)&factory);
        }

        ComPtr<ID3D12Device> device = default;
        fixed (Guid* guid = &ID3D12Device.Guid)
        {
            d3d12.CreateDevice(null, D3DFeatureLevel.Level110, guid, (void**)&device);
        }

        CommandQueueDesc queueDesc = new() { Type = CommandListType.Direct, Flags = 0 };

        ComPtr<ID3D12CommandQueue> commandQueue = default;
        fixed (Guid* guid = &ID3D12CommandQueue.Guid)
        {
            device.Get().CreateCommandQueue(&queueDesc, guid, (void**)&commandQueue);
        }

        SwapChainDesc desc = new()
        {
            BufferCount = 2,
            BufferDesc = new ModeDesc
            {
                Width = 500,
                Height = 300,
                RefreshRate = new Rational { Numerator = 60, Denominator = 1 },
                Format = Format.FormatR8G8B8A8Unorm,
            },
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            OutputWindow = windowHandle,
            SampleDesc = new SampleDesc { Count = 1, Quality = 0 },
            Windowed = 1,
        };

        ComPtr<Silk.NET.DXGI.IDXGISwapChain> tempSwapChain = default;
        factory
            .Get()
            .CreateSwapChain((IUnknown*)commandQueue.Handle, &desc, tempSwapChain.GetAddressOf());

        ComPtr<IDXGISwapChain3> swapChain = default;
        fixed (Guid* guid = &IDXGISwapChain3.Guid)
        {
            tempSwapChain.Get().QueryInterface(guid, (void**)&swapChain);
        }

        tempSwapChain.Dispose();

        const int Present1MethodTableIndex = 22;
        const int ResizeBufferMethodTableIndex = 13;
        const int SwapChainFunctionCount = Present1MethodTableIndex + 1;
        var swapChainVTable = VirtualFunctionTable.FromObject(
            (nuint)(nint)swapChain.Handle,
            SwapChainFunctionCount
        );

        const int ExecuteCommandListTableIndex = 10;
        const int CommandListFunctionCount = ExecuteCommandListTableIndex + 1;
        var commandQueueVTable = VirtualFunctionTable.FromObject(
            (nuint)(nint)commandQueue.Handle,
            CommandListFunctionCount
        );

        swapChain.Dispose();
        commandQueue.Dispose();
        device.Dispose();
        factory.Dispose();

        User32.DestroyWindow(windowHandle);

        _swapChainPresentHook = INativeDetour.CreateAndApply(
            (nint)swapChainVTable.TableEntries[Present1MethodTableIndex].FunctionPointer,
            _swapchainPresentHookDelegate,
            out _swapchainPresentHookOriginal
        );

        _swapChainResizeBufferHook = INativeDetour.CreateAndApply(
            (nint)swapChainVTable.TableEntries[ResizeBufferMethodTableIndex].FunctionPointer,
            _swapChainResizeBufferHookDelegate,
            out _swapChainResizeBufferHookOriginal
        );

        _commandQueueExecuteCommandListHook = INativeDetour.CreateAndApply(
            (nint)commandQueueVTable.TableEntries[ExecuteCommandListTableIndex].FunctionPointer,
            _commandQueueExecuteCommandListHookDelegate,
            out _commandQueueExecuteCommandListHookOriginal
        );

        return true;
    }

    public void Dispose()
    {
        _swapChainResizeBufferHook?.Dispose();
        _swapChainResizeBufferHook = null;

        _commandQueueExecuteCommandListHook?.Dispose();
        _commandQueueExecuteCommandListHook = null;

        _swapChainPresentHook?.Dispose();
        _swapChainPresentHook = null;

        _onPresentAction = null;
    }

    public static void AttachThread()
    {
        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
    }

    public static event Action<ComPtr<IDXGISwapChain3>, uint, uint, IntPtr> OnPresent
    {
        add => _onPresentAction += value;
        remove => _onPresentAction -= value;
    }

    public static event Action<ComPtr<IDXGISwapChain3>, int, int, int, int, int> PreResizeBuffers
    {
        add => _preResizeBuffers += value;
        remove => _preResizeBuffers -= value;
    }

    public static event Action<ComPtr<IDXGISwapChain3>, int, int, int, int, int> PostResizeBuffers
    {
        add => _postResizeBuffers += value;
        remove => _postResizeBuffers -= value;
    }

    public static event Func<ComPtr<ID3D12CommandQueue>, uint, IntPtr, bool> OnExecuteCommandList
    {
        add
        {
            lock (_onExecuteCommandListActionLock)
            {
                OnExecuteCommandListAction += value;
            }
        }
        remove
        {
            lock (_onExecuteCommandListActionLock)
            {
                OnExecuteCommandListAction -= value;
            }
        }
    }

    private static event Func<
        ComPtr<ID3D12CommandQueue>,
        uint,
        IntPtr,
        bool
    > OnExecuteCommandListAction;

    private static unsafe IntPtr SwapChainPresentHook(
        IntPtr self,
        uint syncInterval,
        uint flags,
        IntPtr presentParameters
    )
    {
        AttachThread();

        var swapChain = new ComPtr<IDXGISwapChain3>((IDXGISwapChain3*)self);

        if (_onPresentAction != null)
        {
            foreach (
                var item in _onPresentAction
                    .GetInvocationList()
                    .Cast<Action<ComPtr<IDXGISwapChain3>, uint, uint, IntPtr>>()
            )
            {
                try
                {
                    item(swapChain, syncInterval, flags, presentParameters);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        return _swapchainPresentHookOriginal(self, syncInterval, flags, presentParameters);
    }

    private static unsafe IntPtr SwapChainResizeBuffersHook(
        IntPtr swapchainPtr,
        int bufferCount,
        int width,
        int height,
        int newFormat,
        int swapchainFlags
    )
    {
        AttachThread();

        var swapChain = new ComPtr<IDXGISwapChain3>((IDXGISwapChain3*)swapchainPtr);

        if (_preResizeBuffers != null)
        {
            foreach (
                var item in _preResizeBuffers
                    .GetInvocationList()
                    .Cast<Action<ComPtr<IDXGISwapChain3>, int, int, int, int, int>>()
            )
            {
                try
                {
                    item(swapChain, bufferCount, width, height, newFormat, swapchainFlags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        var result = _swapChainResizeBufferHookOriginal(
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
                var item in _postResizeBuffers
                    .GetInvocationList()
                    .Cast<Action<ComPtr<IDXGISwapChain3>, int, int, int, int, int>>()
            )
            {
                try
                {
                    item(swapChain, bufferCount, width, height, newFormat, swapchainFlags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        return result;
    }

    private static unsafe void CommandQueueExecuteCommandListHook(
        IntPtr self,
        uint numCommandLists,
        IntPtr ppCommandLists
    )
    {
        var executedThings = false;

        lock (_onExecuteCommandListActionLock)
        {
            if (OnExecuteCommandListAction != null)
            {
                var commandQueue = new ComPtr<ID3D12CommandQueue>((ID3D12CommandQueue*)self);

                foreach (
                    var item in OnExecuteCommandListAction
                        .GetInvocationList()
                        .Cast<Func<ComPtr<ID3D12CommandQueue>, uint, IntPtr, bool>>()
                )
                {
                    try
                    {
                        var res = item(commandQueue, numCommandLists, ppCommandLists);
                        if (res)
                        {
                            executedThings = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        _commandQueueExecuteCommandListHookOriginal(self, numCommandLists, ppCommandLists);

        // Investigate at some point why it's needed for unity...
        if (executedThings)
        {
            _commandQueueExecuteCommandListHook.Dispose();
        }
    }

    private delegate IntPtr CDXGISwapChainPresent1Delegate(
        IntPtr self,
        uint syncInterval,
        uint presentFlags,
        IntPtr presentParametersRef
    );

    private delegate IntPtr CDXGISwapChainResizeBuffersDelegate(
        IntPtr self,
        int bufferCount,
        int width,
        int height,
        int newFormat,
        int swapchainFlags
    );

    private delegate void CommandQueueExecuteCommandListDelegate(
        IntPtr self,
        uint numCommandLists,
        IntPtr ppCommandLists
    );
}
