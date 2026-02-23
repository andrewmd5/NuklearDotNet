#nullable enable
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ExampleShared;
using NuklearDotNet;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Example_DX11;

unsafe sealed class DX11Device : NuklearDeviceTex<ID3D11ShaderResourceView>, INuklearDeviceRenderHooks
{
    readonly ID3D11Device _device;
    readonly ID3D11DeviceContext _context;

    readonly ID3D11VertexShader _vertexShader;
    readonly ID3D11PixelShader _pixelShader;
    readonly ID3D11InputLayout _inputLayout;
    readonly ID3D11BlendState _blendState;
    readonly ID3D11RasterizerState _rasterizerState;
    readonly ID3D11DepthStencilState _depthStencilState;
    readonly ID3D11SamplerState _samplerState;
    readonly ID3D11Buffer _constantBuffer;

    ID3D11Buffer? _vertexBuffer;
    ID3D11Buffer? _indexBuffer;
    uint _vertexBufferSize;
    uint _indexBufferSize;

    public int Width { get; set; }
    public int Height { get; set; }
    public ID3D11RenderTargetView? RenderTarget { get; set; }

    const string VertexShaderSource = """
        cbuffer CB : register(b0) { float4x4 ProjectionMatrix; };
        struct VS_IN { float2 pos : POSITION; float2 uv : TEXCOORD; float4 col : COLOR; };
        struct PS_IN { float4 pos : SV_POSITION; float2 uv : TEXCOORD; float4 col : COLOR; };
        PS_IN main(VS_IN i) {
            PS_IN o;
            o.pos = mul(ProjectionMatrix, float4(i.pos, 0, 1));
            o.uv = i.uv;
            o.col = i.col;
            return o;
        }
        """;

    const string PixelShaderSource = """
        Texture2D tex : register(t0);
        SamplerState smp : register(s0);
        float4 main(float4 pos : SV_POSITION, float2 uv : TEXCOORD, float4 col : COLOR) : SV_TARGET {
            return col * tex.Sample(smp, uv);
        }
        """;

    public DX11Device(ID3D11Device device, ID3D11DeviceContext context, int width, int height)
    {
        _device = device;
        _context = context;
        Width = width;
        Height = height;

        ReadOnlyMemory<byte> vsBytes = Compiler.Compile(VertexShaderSource, "main", "vs.hlsl", "vs_5_0");
        ReadOnlyMemory<byte> psBytes = Compiler.Compile(PixelShaderSource, "main", "ps.hlsl", "ps_5_0");

        _vertexShader = _device.CreateVertexShader(vsBytes.Span);
        _pixelShader = _device.CreatePixelShader(psBytes.Span);

        // NkVertex layout: float2 pos (0), float2 uv (8), byte4 color (16) = 20 bytes
        InputElementDescription[] inputElements =
        [
            new("POSITION", 0, Format.R32G32_Float, 0, 0),
            new("TEXCOORD", 0, Format.R32G32_Float, 8, 0),
            new("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0),
        ];
        _inputLayout = _device.CreateInputLayout(inputElements, vsBytes.Span);

        // Blend factors match the official Nuklear D3D11 demo
        var blendDesc = new BlendDescription();
        blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
        {
            BlendEnable = true,
            SourceBlend = Blend.SourceAlpha,
            DestinationBlend = Blend.InverseSourceAlpha,
            BlendOperation = BlendOperation.Add,
            SourceBlendAlpha = Blend.InverseSourceAlpha,
            DestinationBlendAlpha = Blend.Zero,
            BlendOperationAlpha = BlendOperation.Add,
            RenderTargetWriteMask = ColorWriteEnable.All,
        };
        _blendState = _device.CreateBlendState(blendDesc);

        _rasterizerState = _device.CreateRasterizerState(new RasterizerDescription
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None,
            ScissorEnable = true,
            DepthClipEnable = true,
        });

        _depthStencilState = _device.CreateDepthStencilState(new DepthStencilDescription
        {
            DepthEnable = false,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunction.Always,
            StencilEnable = false,
        });

        _samplerState = _device.CreateSamplerState(new SamplerDescription
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
        });

        _constantBuffer = _device.CreateBuffer(new BufferDescription(
            64u, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
    }

    public override ID3D11ShaderResourceView CreateTexture(int W, int H, IntPtr Data)
    {
        var texDesc = new Texture2DDescription
        {
            Width = (uint)W,
            Height = (uint)H,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.R8G8B8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Immutable,
            BindFlags = BindFlags.ShaderResource,
        };

        var initData = new SubresourceData(Data, (uint)(W * 4));
        using var texture = _device.CreateTexture2D(texDesc, [initData]);
        return _device.CreateShaderResourceView(texture);
    }

    public override void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer)
    {
        uint vertBytes = (uint)(VertexBuffer.Length * sizeof(NkVertex));
        uint indBytes = (uint)(IndexBuffer.Length * sizeof(ushort));

        if (_vertexBuffer is null || _vertexBufferSize < vertBytes)
        {
            _vertexBuffer?.Dispose();
            _vertexBufferSize = vertBytes;
            _vertexBuffer = _device.CreateBuffer(new BufferDescription(
                vertBytes, BindFlags.VertexBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
        }

        if (_indexBuffer is null || _indexBufferSize < indBytes)
        {
            _indexBuffer?.Dispose();
            _indexBufferSize = indBytes;
            _indexBuffer = _device.CreateBuffer(new BufferDescription(
                indBytes, BindFlags.IndexBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
        }

        var mappedVert = _context.Map(_vertexBuffer, MapMode.WriteDiscard);
        fixed (NkVertex* src = VertexBuffer)
            Buffer.MemoryCopy(src, (void*)mappedVert.DataPointer, vertBytes, vertBytes);
        _context.Unmap(_vertexBuffer);

        var mappedInd = _context.Map(_indexBuffer, MapMode.WriteDiscard);
        fixed (ushort* src = IndexBuffer)
            Buffer.MemoryCopy(src, (void*)mappedInd.DataPointer, indBytes, indBytes);
        _context.Unmap(_indexBuffer);
    }

    public void BeginRender()
    {
        var proj = Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
        var mappedCb = _context.Map(_constantBuffer, MapMode.WriteDiscard);
        *(Matrix4x4*)mappedCb.DataPointer = proj;
        _context.Unmap(_constantBuffer);

        if (RenderTarget is not null)
            _context.OMSetRenderTargets(RenderTarget);
        _context.OMSetBlendState(_blendState);
        _context.OMSetDepthStencilState(_depthStencilState);

        _context.RSSetViewport(0, 0, Width, Height);
        _context.RSSetState(_rasterizerState);

        if (_vertexBuffer is not null)
            _context.IASetVertexBuffer(0, _vertexBuffer, (uint)sizeof(NkVertex));
        if (_indexBuffer is not null)
            _context.IASetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
        _context.IASetInputLayout(_inputLayout);
        _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

        _context.VSSetShader(_vertexShader);
        _context.VSSetConstantBuffer(0, _constantBuffer);
        _context.PSSetShader(_pixelShader);
        _context.PSSetSampler(0, _samplerState);
    }

    public void EndRender() { }

    public override void Render(NkHandle Userdata, ID3D11ShaderResourceView Texture, NkRect ClipRect, uint Offset, uint Count)
    {
        _context.PSSetShaderResource(0, Texture);
        _context.RSSetScissorRect(
            (int)ClipRect.X,
            (int)ClipRect.Y,
            (int)ClipRect.W,
            (int)ClipRect.H);

        _context.DrawIndexed(Count, Offset, 0);
    }
}

#region Win32 P/Invoke

static unsafe partial class Win32
{
    public const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    public const uint WS_VISIBLE = 0x10000000;
    public const int CW_USEDEFAULT = unchecked((int)0x80000000);
    public const int CS_HREDRAW = 0x0002;
    public const int CS_VREDRAW = 0x0001;
    public const int IDC_ARROW = 32512;

    public const uint WM_DESTROY = 0x0002;
    public const uint WM_SIZE = 0x0005;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_QUIT = 0x0012;
    public const uint WM_KEYDOWN = 0x0100;
    public const uint WM_KEYUP = 0x0101;
    public const uint WM_CHAR = 0x0102;
    public const uint WM_MOUSEMOVE = 0x0200;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_LBUTTONUP = 0x0202;
    public const uint WM_RBUTTONDOWN = 0x0204;
    public const uint WM_RBUTTONUP = 0x0205;
    public const uint WM_MBUTTONDOWN = 0x0207;
    public const uint WM_MBUTTONUP = 0x0208;
    public const uint WM_MOUSEWHEEL = 0x020A;

    public const int VK_BACK = 0x08;
    public const int VK_TAB = 0x09;
    public const int VK_RETURN = 0x0D;
    public const int VK_SHIFT = 0x10;
    public const int VK_CONTROL = 0x11;
    public const int VK_DELETE = 0x2E;
    public const int VK_LEFT = 0x25;
    public const int VK_UP = 0x26;
    public const int VK_RIGHT = 0x27;
    public const int VK_DOWN = 0x28;
    public const int VK_HOME = 0x24;
    public const int VK_END = 0x23;

    public const uint PM_REMOVE = 0x0001;

    public delegate nint WndProc(nint hWnd, uint msg, nuint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEXW
    {
        public int cbSize;
        public int style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public nint lpszMenuName;
        public char* lpszClassName;
        public nint hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public nint hwnd;
        public uint message;
        public nuint wParam;
        public nint lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial ushort RegisterClassExW(WNDCLASSEXW* wc);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial nint CreateWindowExW(
        uint dwExStyle, char* lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(nint hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PeekMessageW(MSG* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TranslateMessage(MSG* lpMsg);

    [LibraryImport("user32.dll")]
    public static partial nint DispatchMessageW(MSG* lpMsg);

    [LibraryImport("user32.dll")]
    public static partial nint DefWindowProcW(nint hWnd, uint msg, nuint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    public static partial void PostQuitMessage(int nExitCode);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint GetModuleHandleW(string? lpModuleName);

    [LibraryImport("user32.dll")]
    public static partial nint LoadCursorW(nint hInstance, nint lpCursorName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetClientRect(nint hWnd, RECT* lpRect);

    [LibraryImport("user32.dll")]
    public static partial short GetKeyState(int nVirtKey);

    [LibraryImport("user32.dll")]
    public static partial nint SetCapture(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

    public static bool IsKeyDown(int vk) => (GetKeyState(vk) & 0x8000) != 0;
    public static int GET_X_LPARAM(nint lp) => (short)(lp & 0xFFFF);
    public static int GET_Y_LPARAM(nint lp) => (short)((lp >> 16) & 0xFFFF);
    public static short GET_WHEEL_DELTA(nuint wp) => (short)((wp >> 16) & 0xFFFF);
}

#endregion

static unsafe class Program
{
    static DX11Device? s_dev;
    static IDXGISwapChain? s_swapChain;
    static ID3D11DeviceContext? s_context;
    static ID3D11Device? s_device;
    static ID3D11RenderTargetView? s_rtv;
    static nint s_hwnd;
    static bool s_running = true;

    [STAThread]
    static void Main()
    {
        nint hInstance = Win32.GetModuleHandleW(null);

        fixed (char* className = "NuklearDX11")
        {
            var wc = new Win32.WNDCLASSEXW
            {
                cbSize = sizeof(Win32.WNDCLASSEXW),
                style = Win32.CS_HREDRAW | Win32.CS_VREDRAW,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(s_wndProcDelegate),
                hInstance = hInstance,
                hCursor = Win32.LoadCursorW(0, Win32.IDC_ARROW),
                lpszClassName = className,
            };
            Win32.RegisterClassExW(&wc);

            s_hwnd = Win32.CreateWindowExW(
                0, className, "NuklearDotNet - DirectX 11 Example",
                Win32.WS_OVERLAPPEDWINDOW | Win32.WS_VISIBLE,
                Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT, 1280, 800,
                0, 0, hInstance, 0);
        }

        Win32.RECT clientRect;
        Win32.GetClientRect(s_hwnd, &clientRect);
        int w = clientRect.Right - clientRect.Left;
        int h = clientRect.Bottom - clientRect.Top;

        var swapChainDesc = new SwapChainDescription
        {
            BufferCount = 1,
            BufferDescription = new ModeDescription((uint)w, (uint)h, Format.R8G8B8A8_UNorm),
            BufferUsage = Usage.RenderTargetOutput,
            OutputWindow = s_hwnd,
            SampleDescription = new SampleDescription(1, 0),
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
        };

        D3D11.D3D11CreateDeviceAndSwapChain(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.None,
            [],
            swapChainDesc,
            out s_swapChain,
            out s_device,
            out _,
            out s_context).CheckError();

        CreateRenderTargetView();

        s_dev = new DX11Device(s_device!, s_context!, w, h);
        s_dev.RenderTarget = s_rtv;
        Shared.Init(s_dev);

        var sw = Stopwatch.StartNew();
        float dt = 0.016f;

        while (s_running)
        {
            Win32.MSG msg;
            while (Win32.PeekMessageW(&msg, 0, 0, 0, Win32.PM_REMOVE))
            {
                if (msg.message == Win32.WM_QUIT)
                {
                    s_running = false;
                    break;
                }
                Win32.TranslateMessage(&msg);
                Win32.DispatchMessageW(&msg);
            }

            if (!s_running) break;

            s_context!.ClearRenderTargetView(s_rtv!, new Color4(0.392f, 0.584f, 0.929f, 1.0f));

            Shared.DrawLoop(dt);

            s_swapChain!.Present(1, PresentFlags.None);

            dt = Math.Max((float)sw.Elapsed.TotalSeconds, 0.001f);
            sw.Restart();
        }

        s_rtv?.Dispose();
        s_swapChain?.Dispose();
        s_context?.Dispose();
        s_device?.Dispose();
    }

    // prevent delegate from being collected
    static readonly Win32.WndProc s_wndProcDelegate = WndProc;

    static nint WndProc(nint hWnd, uint msg, nuint wParam, nint lParam)
    {
        switch (msg)
        {
            case Win32.WM_DESTROY:
                Win32.PostQuitMessage(0);
                return 0;

            case Win32.WM_SIZE:
            {
                int w = Win32.GET_X_LPARAM(lParam);
                int h = Win32.GET_Y_LPARAM(lParam);
                if (w > 0 && h > 0 && s_swapChain is not null)
                {
                    s_rtv?.Dispose();
                    s_rtv = null;
                    s_swapChain.ResizeBuffers(0, (uint)w, (uint)h, Format.Unknown, SwapChainFlags.None);
                    CreateRenderTargetView();
                    if (s_dev is not null)
                    {
                        s_dev.Width = w;
                        s_dev.Height = h;
                        s_dev.RenderTarget = s_rtv;
                    }
                }
                return 0;
            }

            case Win32.WM_MOUSEMOVE:
                s_dev?.OnMouseMove(Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam));
                return 0;

            case Win32.WM_LBUTTONDOWN:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Left, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), true);
                Win32.SetCapture(hWnd);
                return 0;
            case Win32.WM_LBUTTONUP:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Left, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), false);
                Win32.ReleaseCapture();
                return 0;

            case Win32.WM_RBUTTONDOWN:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Right, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), true);
                Win32.SetCapture(hWnd);
                return 0;
            case Win32.WM_RBUTTONUP:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Right, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), false);
                Win32.ReleaseCapture();
                return 0;

            case Win32.WM_MBUTTONDOWN:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Middle, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), true);
                Win32.SetCapture(hWnd);
                return 0;
            case Win32.WM_MBUTTONUP:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Middle, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), false);
                Win32.ReleaseCapture();
                return 0;

            case Win32.WM_MOUSEWHEEL:
                s_dev?.OnScroll(0, Win32.GET_WHEEL_DELTA(wParam) / 120f);
                return 0;

            case Win32.WM_CHAR:
            {
                char c = (char)wParam;
                if (c >= 32)
                    s_dev?.OnText(c.ToString());
                return 0;
            }

            case Win32.WM_KEYDOWN:
            case Win32.WM_KEYUP:
            {
                bool down = msg == Win32.WM_KEYDOWN;
                int vk = (int)wParam;
                bool ctrl = Win32.IsKeyDown(Win32.VK_CONTROL);

                switch (vk)
                {
                    case Win32.VK_SHIFT: s_dev?.OnKey(NkKeys.Shift, down); break;
                    case Win32.VK_CONTROL: s_dev?.OnKey(NkKeys.Ctrl, down); break;
                    case Win32.VK_DELETE: s_dev?.OnKey(NkKeys.Del, down); break;
                    case Win32.VK_RETURN: s_dev?.OnKey(NkKeys.Enter, down); break;
                    case Win32.VK_TAB: s_dev?.OnKey(NkKeys.Tab, down); break;
                    case Win32.VK_BACK: s_dev?.OnKey(NkKeys.Backspace, down); break;
                    case Win32.VK_UP: s_dev?.OnKey(NkKeys.Up, down); break;
                    case Win32.VK_DOWN: s_dev?.OnKey(NkKeys.Down, down); break;
                    case Win32.VK_HOME: s_dev?.OnKey(NkKeys.LineStart, down); break;
                    case Win32.VK_END: s_dev?.OnKey(NkKeys.LineEnd, down); break;

                    case Win32.VK_LEFT:
                        if (ctrl && down) s_dev?.OnKey(NkKeys.TextWordLeft, true);
                        else s_dev?.OnKey(NkKeys.Left, down);
                        break;
                    case Win32.VK_RIGHT:
                        if (ctrl && down) s_dev?.OnKey(NkKeys.TextWordRight, true);
                        else s_dev?.OnKey(NkKeys.Right, down);
                        break;

                    case 'C' when ctrl && down: s_dev?.OnKey(NkKeys.Copy, true); break;
                    case 'V' when ctrl && down: s_dev?.OnKey(NkKeys.Paste, true); break;
                    case 'X' when ctrl && down: s_dev?.OnKey(NkKeys.Cut, true); break;
                    case 'A' when ctrl && down: s_dev?.OnKey(NkKeys.TextSelectAll, true); break;
                }
                return 0;
            }
        }

        return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    static void CreateRenderTargetView()
    {
        using var backBuffer = s_swapChain!.GetBuffer<ID3D11Texture2D>(0);
        s_rtv = s_device!.CreateRenderTargetView(backBuffer);
    }
}
