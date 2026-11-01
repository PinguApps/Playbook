// snippet:type-ledger
using System.Numerics;
using System.Runtime.InteropServices;

namespace Playbook.Samples;

internal static class TypeLedger
{
    // Merewick's operation owns this allocation and always frees it before returning.
    public static unsafe void Write(Type owner, int size, int alignment)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (!BitOperations.IsPow2(alignment)) throw new ArgumentException("Use a positive power of two.", nameof(alignment));
        void* address = NativeMemory.AlignedAlloc((nuint)size, (nuint)alignment);
        try
        {
            var bytes = new Span<byte>(address, size);
            bytes.Fill(0x4D);
            GC.Collect();
            if ((nuint)address % (nuint)alignment != 0 || bytes[0] != 0x4D || bytes[^1] != 0x4D)
                throw new InvalidOperationException("Alignment or storage check failed.");
            Console.WriteLine($"STORAGE size={size} alignment={alignment} remainder={(nuint)address % (nuint)alignment} first={bytes[0]} last={bytes[^1]}");
        }
        finally { NativeMemory.AlignedFree(address); }
        GC.KeepAlive(owner);
    }
}
// endsnippet:type-ledger
