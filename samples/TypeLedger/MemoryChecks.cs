using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Playbook.Samples;

internal static class MemoryChecks
{
    public static void Run()
    {
        if (!RuntimeFeature.IsDynamicCodeSupported) throw new PlatformNotSupportedException("Collectible type proof needs dynamic code.");
        WeakReference released = ExerciseCollectibleType();
        for (int i = 0; i < 30 && released.IsAlive; i++) Collect();
        Require(!released.IsAlive, "Collectible type remained rooted after all operations ended.");
        Console.WriteLine("LIFETIME collectible type collected after owner release; no pointer used afterward");
        Throws<ArgumentOutOfRangeException>(() => TypeLedger.Write(typeof(MemoryChecks), -1, 64));
        foreach (int alignment in new[] { -8, 0, 3, 48 })
            Throws<ArgumentException>(() => TypeLedger.Write(typeof(MemoryChecks), 64, alignment));
#if NET11_0
        Throws<ArgumentException>(() => RuntimeHelpers.AllocateTypeAssociatedMemory(null!, 64, 64));
        Throws<ArgumentException>(() => RuntimeHelpers.AllocateTypeAssociatedMemory(new TypeDelegator(typeof(int)), 64, 64));
        Throws<ArgumentOutOfRangeException>(() => RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(int), -1, 64));
        foreach (int alignment in new[] { -8, 0, 3, 48 })
            Throws<ArgumentException>(() => RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(int), 64, alignment));
        // Zero bytes are accepted, but there is no readable/writable storage.
        _ = RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(MemoryChecks), 0, 64);
        Console.WriteLine("CONTRACT current raw API rejects non-runtime types/negative size/non-power-of-two alignment; accepts zero bytes without dereference");
#endif
        Console.WriteLine("BOUNDARIES negative size and alignments -8/0/3/48 rejected; no oversized allocations attempted");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ExerciseCollectibleType()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("MerewickTypeLedger"), AssemblyBuilderAccess.RunAndCollect);
        Type type = assembly.DefineDynamicModule("Ledger").DefineType("Merewick.ParcelIndex", TypeAttributes.Public).CreateType()!;
        Require(type.IsCollectible, "Expected collectible runtime type.");
        foreach (int alignment in new[] { 1, 8, 16, 64, 256 })
            TypeLedger.Write(type, 257, alignment);
        Collect();
        TypeLedger.Write(type, 257, 64);
        var weak = new WeakReference(type);
        GC.KeepAlive(type);
        return weak;
    }

    internal static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    internal static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }
}
