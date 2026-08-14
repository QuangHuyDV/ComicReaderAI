using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Avalonia.Controls;
using WinRT;

namespace Crai.Desktop.Feasibility;

public class CaptureProto
{
    // GUIDs cho COM interop
    private static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");
    private static readonly Guid IID_ID3D11Texture2D = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
    private static readonly Guid IID_IDXGIResource = new Guid("035f3ab4-482e-4e50-b41f-8a7f8b70227d");
    private static readonly Guid IID_ID3D11DeviceContext = new Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da");

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        IntPtr CreateForWindow([In] IntPtr window, [In] ref Guid iid);
        IntPtr CreateForMonitor([In] IntPtr monitor, [In] ref Guid iid);
    }

    [ComImport]
    [Guid("A9B3D012-3DF2-4EE3-B8D1-859536D002D9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDirect3DDxgiInterfaceAccess
    {
        IntPtr GetInterface([In] ref Guid iid);
    }

    [DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int D3D11CreateDevice(
        IntPtr pAdapter, int driverType, IntPtr software, int flags,
        IntPtr pFeatureLevels, int featureLevels, int sdkVersion,
        out IntPtr ppDevice, out int pFeatureLevel, out IntPtr ppImmediateContext);

    [DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern uint CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

    // D3D11 Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct D3D11_TEXTURE2D_DESC
    {
        public uint Width;
        public uint Height;
        public uint MipLevels;
        public uint ArraySize;
        public int Format; // DXGI_FORMAT
        public uint SampleDescCount;
        public uint SampleDescQuality;
        public int Usage; // D3D11_USAGE
        public uint BindFlags;
        public uint CPUAccessFlags;
        public uint MiscFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct D3D11_MAPPED_SUBRESOURCE
    {
        public IntPtr pData;
        public uint RowPitch;
        public uint DepthPitch;
    }

    // COM Interfaces
    [ComImport]
    [Guid("db6a6f3f-195b-4808-ab00-7d37aed78747")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ID3D11Device
    {
        void CreateTexture2D(ref D3D11_TEXTURE2D_DESC pDesc, IntPtr pInitialData, out IntPtr ppTexture2D);
        void GetImmediateContext(out IntPtr ppImmediateContext);
    }

    [ComImport]
    [Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ID3D11DeviceContext
    {
        void CopyResource(IntPtr pDstResource, IntPtr pSrcResource);
        int Map(IntPtr pResource, uint Subresource, int MapType, uint MapFlags, out D3D11_MAPPED_SUBRESOURCE pMappedResource);
        void Unmap(IntPtr pResource, uint Subresource);
    }

    [ComImport]
    [Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ID3D11Texture2D
    {
        void GetDesc(out D3D11_TEXTURE2D_DESC pDesc);
    }

    private static readonly Guid IID_IDXGIDevice = new Guid("54ec77f5-d307-4b66-bb58-3058564F6366");

    public static IDirect3DDevice CreateDirect3DDevice()
    {
        int hr = D3D11CreateDevice(IntPtr.Zero, 1, IntPtr.Zero, 0, IntPtr.Zero, 0, 7, out IntPtr d3d11Device, out _, out IntPtr context);
        if (hr != 0)
        {
            hr = D3D11CreateDevice(IntPtr.Zero, 5, IntPtr.Zero, 0, IntPtr.Zero, 0, 7, out d3d11Device, out _, out context);
            if (hr != 0) throw new Exception($"D3D11CreateDevice failed: 0x{hr:X}");
        }

        if (context != IntPtr.Zero) Marshal.Release(context);

        hr = Marshal.QueryInterface(d3d11Device, ref IID_IDXGIDevice, out IntPtr dxgiDevice);
        if (hr != 0) throw new Exception("QueryInterface IDXGIDevice failed");

        uint hr2 = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice, out IntPtr pUnknown);
        Marshal.Release(dxgiDevice);
        Marshal.Release(d3d11Device);

        if (hr2 != 0) throw new Exception($"CreateDirect3D11DeviceFromDXGIDevice failed: 0x{hr2:X}");

        var device = WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(pUnknown);
        Marshal.Release(pUnknown);
        return device;
    }

    public static GraphicsCaptureItem CreateItemForWindow(IntPtr hwnd)
    {
        var factory = WindowsRuntimeMarshal.GetActivationFactory(typeof(GraphicsCaptureItem));
        var interop = (IGraphicsCaptureItemInterop)factory;
        IntPtr itemPointer = interop.CreateForWindow(hwnd, GraphicsCaptureItemGuid);
        var item = Marshal.GetObjectForIUnknown(itemPointer) as GraphicsCaptureItem;
        Marshal.Release(itemPointer);
        return item ?? throw new Exception("Failed to create GraphicsCaptureItem");
    }

    /// <summary>
    /// Chụp 1 frame của window, tính latency và lưu thành PNG.
    /// </summary>
    public static async Task CaptureWindowAsync(IntPtr hwnd, string outputPath)
    {
        var item = CreateItemForWindow(hwnd);
        var device = CreateDirect3DDevice();

        // 87 = DXGI_FORMAT_B8G8R8A8_UNORM
        var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(device, (DirectXPixelFormat)87, 1, item.Size);
        var session = framePool.CreateCaptureSession(item);

        var tcs = new TaskCompletionSource<bool>();
        var startTime = DateTime.Now;

        framePool.FrameArrived += (sender, args) =>
        {
            try
            {
                using var frame = sender.TryGetNextFrame();
                if (frame == null) return;

                var duration = DateTime.Now - startTime;
                Console.WriteLine($"[Capture] Frame arrived in {duration.TotalMilliseconds}ms");

                // Copy texture data
                var surfaceAccess = frame.Surface.As<IDirect3DDxgiInterfaceAccess>();
                IntPtr pSurfaceUnknown = surfaceAccess.GetInterface(IID_ID3D11Texture2D);

                // Lấy ID3D11Texture2D từ frame surface
                var srcTexture = (ID3D11Texture2D)Marshal.GetObjectForIUnknown(pSurfaceUnknown);
                srcTexture.GetDesc(out var desc);

                // Lấy D3D11 Device từ surface interop
                // Lấy ID3D11Device từ texture
                IntPtr pDeviceUnknown = Marshal.GetIUnknownForObject(srcTexture);
                // Để lấy device từ texture, ta QueryInterface ID3D11Resource, rồi GetDevice. Hoặc tạo sẵn device
                // Vì ta đã tạo device ở đầu method, ta cast nó về ID3D11Device
                // Lấy COM pointer từ WinRT IDirect3DDevice
                var deviceAccess = device.As<IDirect3DDxgiInterfaceAccess>();
                IntPtr pD3DDeviceUnknown = deviceAccess.GetInterface(new Guid("db6a6f3f-195b-4808-ab00-7d37aed78747")); // IID_ID3D11Device
                var d3d11Device = (ID3D11Device)Marshal.GetObjectForIUnknown(pD3DDeviceUnknown);

                // Tạo staging texture
                var stagingDesc = new D3D11_TEXTURE2D_DESC
                {
                    Width = desc.Width,
                    Height = desc.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = desc.Format,
                    SampleDescCount = 1,
                    SampleDescQuality = 0,
                    Usage = 3, // D3D11_USAGE_STAGING
                    BindFlags = 0,
                    CPUAccessFlags = 0x20000, // D3D11_CPU_ACCESS_READ (0x20000)
                    MiscFlags = 0
                };

                d3d11Device.CreateTexture2D(ref stagingDesc, IntPtr.Zero, out IntPtr pStagingTexture);
                d3d11Device.GetImmediateContext(out IntPtr pContext);
                var context = (ID3D11DeviceContext)Marshal.GetObjectForIUnknown(pContext);

                // Copy resource GPU -> CPU Staging
                context.CopyResource(pStagingTexture, pSurfaceUnknown);

                // Map data
                // MapType: Read = 1
                int hr = context.Map(pStagingTexture, 0, 1, 0, out var mapped);
                if (hr == 0)
                {
                    int bytesPerPixel = 4;
                    int rowPitch = (int)mapped.RowPitch;
                    int width = (int)desc.Width;
                    int height = (int)desc.Height;

                    // Lưu BGRA data thành BMP/PNG thô
                    byte[] pixelBuffer = new byte[width * height * bytesPerPixel];
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr srcRow = mapped.pData + (y * rowPitch);
                        Marshal.Copy(srcRow, pixelBuffer, y * width * bytesPerPixel, width * bytesPerPixel);
                    }

                    // Save BMP file format để đơn giản không dùng library ngoài
                    SaveBmp(pixelBuffer, width, height, outputPath);

                    context.Unmap(pStagingTexture, 0);
                    Console.WriteLine($"[Capture] Saved image to: {outputPath}");
                }

                Marshal.Release(pStagingTexture);
                Marshal.Release(pContext);
                Marshal.Release(pSurfaceUnknown);
                Marshal.Release(pD3DDeviceUnknown);
                Marshal.Release(pDeviceUnknown);

                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        };

        session.StartCapture();
        await tcs.Task;
        session.Dispose();
        framePool.Dispose();
    }

    private static void SaveBmp(byte[] bgraData, int width, int height, string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        // BMP Header
        bw.Write((ushort)0x4D42); // BM
        uint fileSize = (uint)(54 + bgraData.Length);
        bw.Write(fileSize);
        bw.Write((uint)0); // Reserved
        bw.Write((uint)54); // Offset to pixel data

        // DIB Header
        bw.Write((uint)40); // Header size
        bw.Write(width);
        bw.Write(-height); // Top-down
        bw.Write((ushort)1); // Planes
        bw.Write((ushort)32); // Bits per pixel (RGBA)
        bw.Write((uint)0); // BI_RGB (uncompressed)
        bw.Write((uint)bgraData.Length); // Image size
        bw.Write(2835); // Pixels per meter X
        bw.Write(2835); // Pixels per meter Y
        bw.Write((uint)0); // Colors
        bw.Write((uint)0); // Important colors

        // Pixel data
        bw.Write(bgraData);
    }
}
