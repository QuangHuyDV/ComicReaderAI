using System;
using System.Runtime.InteropServices;

class D3DDebug
{
    [DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int D3D11CreateDevice(
        IntPtr pAdapter, int driverType, IntPtr software, int flags,
        IntPtr pFeatureLevels, int featureLevels, int sdkVersion,
        out IntPtr ppDevice, out int pFeatureLevel, out IntPtr ppImmediateContext);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int QueryInterfaceDelegate(IntPtr thisPtr, ref Guid riid, out IntPtr ppvObject);

    static void Main()
    {
        Console.WriteLine("Direct3D11 Device COM QueryInterface test:");
        
        int hr = D3D11CreateDevice(IntPtr.Zero, 1, IntPtr.Zero, 0, IntPtr.Zero, 0, 7, out IntPtr device, out int fl, out IntPtr context);
        Console.WriteLine($"  D3D11CreateDevice: HRESULT = 0x{hr:X}, DevicePtr = {device}, FeatureLevel = 0x{fl:X}");
        
        if (hr == 0 && device != IntPtr.Zero)
        {
            TestInterface(device, "IUnknown", new Guid("00000000-0000-0000-C000-000000000046"));
            TestInterface(device, "ID3D11Device", new Guid("db6a6f3f-195b-4808-ab00-7d37aed78747"));
            TestInterface(device, "IDXGIDevice", new Guid("54ec77f5-d307-4b66-bb58-3058564f6366"));
            TestInterface(device, "IDXGIDevice1", new Guid("77db97db-627f-400a-af58-2786553f024f"));
            
            Marshal.Release(device);
        }
        if (context != IntPtr.Zero) Marshal.Release(context);
    }

    static void TestInterface(IntPtr device, string name, Guid iid)
    {
        try
        {
            IntPtr vtable = Marshal.ReadIntPtr(device);
            IntPtr qiPtr = Marshal.ReadIntPtr(vtable, 0);
            var queryInterface = Marshal.GetDelegateForFunctionPointer<QueryInterfaceDelegate>(qiPtr);
            
            Guid tempIid = iid;
            int hr = queryInterface(device, ref tempIid, out IntPtr ppv);
            Console.WriteLine($"    QI for {name} ({iid}): HRESULT = 0x{hr:X}, Pointer = {ppv}");
            if (hr == 0 && ppv != IntPtr.Zero)
            {
                Marshal.Release(ppv);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    QI for {name} failed with exception: {ex.Message}");
        }
    }
}
