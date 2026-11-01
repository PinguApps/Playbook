using System.Reflection.Emit;

namespace Playbook.Samples;

internal static class SignatureChecks
{
    internal static void Run()
    {
        var method = new DynamicMethod("RejectedSignature", typeof(void), Type.EmptyTypes);
        ILGenerator il = method.GetILGenerator();
        bool nullRejected = false, nonPointerRejected = false;
        try { il.EmitCalli(null!); } catch (ArgumentNullException) { nullRejected = true; }
        try { il.EmitCalli(typeof(Func<int, int>)); } catch (ArgumentException) { nonPointerRejected = true; }
        CallChecks.Check(nullRejected && nonPointerRejected, "null/non-function-pointer signature rejection");
        Console.WriteLine("CHECK signature: null and delegate class rejected before any invalid IL is executed");
    }
}
