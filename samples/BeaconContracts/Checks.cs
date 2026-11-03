using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Playbook.Samples;

internal static unsafe class Checks
{
    private static void Main()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows COM proof");
        var beacon = new Beacon();
        void* pointer = ComInterfaceMarshaller<IBeacon>.ConvertToUnmanaged(beacon);
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "beacon.dll"));
        try
        {
            var exercise = (delegate* unmanaged<void*, delegate* unmanaged<void*, void>, int>)NativeLibrary.GetExport(library, "exercise");
            int result = exercise(pointer, &Free);
            if (result != 0 || beacon.CurrentReading != 29 || beacon.CurrentCaption != "Quay Ω")
                throw new InvalidOperationException($"Native COM check failed: {result}, {beacon.CurrentReading}, {beacon.CurrentCaption}");
            Console.WriteLine($"PASS BeaconContracts .NET {Environment.Version}: native HRESULT get/set, UTF16 marshalling, ownership, managed state");
            Console.WriteLine($"CoreLib {typeof(object).Assembly.Location}");
        }
        finally
        {
            ComInterfaceMarshaller<IBeacon>.Free(pointer);
            NativeLibrary.Free(library);
        }
    }

    [UnmanagedCallersOnly]
    private static void Free(void* value) => Marshal.FreeCoTaskMem((nint)value);
}

