using System.Numerics;
using System.Runtime.CompilerServices;

namespace Playbook.Samples;

#region type-ledger
internal static class TypeLedger
{
    // Merewick's allocation belongs to the runtime type, not to this method.
    public static unsafe void Write(Type owner, int size, int alignment)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (!BitOperations.IsPow2(alignment)) throw new ArgumentException("Use a positive power of two.", nameof(alignment));
        nint address = RuntimeHelpers.AllocateTypeAssociatedMemory(owner, size, alignment);
        var bytes = new Span<byte>((void*)address, size);
        bytes.Fill(0x4D);
        GC.Collect();
        if ((nuint)address % (nuint)alignment != 0 || bytes[0] != 0x4D || bytes[^1] != 0x4D)
            throw new InvalidOperationException("Alignment or storage check failed.");
        Console.WriteLine($"STORAGE size={size} alignment={alignment} remainder={(nuint)address % (nuint)alignment} first={bytes[0]} last={bytes[^1]}");
        // A raw address/span does not root the collectible owner. Keep it alive through the final access.
        GC.KeepAlive(owner);
        // Do not call NativeMemory.AlignedFree/Free on runtime-owned memory.
    }
}
#endregion
