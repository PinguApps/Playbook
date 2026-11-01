// snippet:call-emitter
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Playbook.Samples;

internal static class CallEmitter
{
    internal delegate int ManagedCall(int loads, int adjustment, nint address);
    internal delegate int NativeCall(int loads, int multiplier, int divisor, nint address);

    internal static void RequireDynamicCode(bool supported)
    {
        if (!supported) throw new PlatformNotSupportedException("The quarry adapter requires runtime code generation.");
    }

    [RequiresDynamicCode("Creates a DynamicMethod and JIT-compiles its calli instruction.")]
    internal static unsafe ManagedCall CreateManaged()
    {
        RequireDynamicCode(RuntimeFeature.IsDynamicCodeSupported);
        var method = new DynamicMethod("QuarryManaged", typeof(int), [typeof(int), typeof(int), typeof(nint)]);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2); // Pointer is pushed AFTER arguments.
        il.EmitCalli(typeof(delegate* managed<int, int, int>));
        il.Emit(OpCodes.Ret);
        return method.CreateDelegate<ManagedCall>();
    }

    [RequiresDynamicCode("Creates a DynamicMethod and JIT-compiles its calli instruction.")]
    internal static unsafe NativeCall CreateNative()
    {
        RequireDynamicCode(RuntimeFeature.IsDynamicCodeSupported);
        var method = new DynamicMethod("QuarryNative", typeof(int), [typeof(int), typeof(int), typeof(int), typeof(nint)]);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);
        il.EmitCalli(typeof(delegate* unmanaged[Stdcall]<int, int, int, int>));
        il.Emit(OpCodes.Ret);
        return method.CreateDelegate<NativeCall>();
    }
}
// endsnippet:call-emitter
