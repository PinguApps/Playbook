using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Playbook.Samples;

// Original control for PIN-1033: ordinary net10/net11 references are complete.
public sealed class ParcelCounter
{
    private int count = 42;
    public ref readonly int Read() => ref count;
    public void Update(int value) => count = value;

    public void Verify()
    {
        var fromMethod = Read;
        var fromLambda = ref readonly int () => ref count;
        ref readonly int LocalRead() => ref count;
        ref readonly int retained = ref LocalRead();
        if (fromMethod() != 42 || fromLambda() != 42 || retained != 42)
            throw new InvalidOperationException("Readonly calls returned unexpected data.");
        Update(73);
        if (fromMethod() != 73 || fromLambda() != 73)
            throw new InvalidOperationException("Readonly references became copies.");
        CheckRetained(retained, 73);

        foreach (var method in new[] { fromMethod.GetType().GetMethod("Invoke")!, fromLambda.GetType().GetMethod("Invoke")!,
            typeof(ParcelCounter).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Single(method => method.Name.Contains("g__LocalRead|", StringComparison.Ordinal)) })
        {
            var modifiers = method.ReturnParameter.GetRequiredCustomModifiers();
            if (!method.ReturnType.IsByRef || !modifiers.Contains(typeof(InAttribute)))
                throw new InvalidOperationException($"Missing readonly modreq on {method.Name}.");
            Console.WriteLine($"PASS metadata {method.Name}: byref modreq({typeof(InAttribute).FullName})");
        }
        Console.WriteLine("PASS readonly: method-group/lambda/local calls retain 42 -> 73 reference identity");
    }

    private static void CheckRetained(int actual, int expected)
    {
        if (actual != expected) throw new InvalidOperationException("Retained reference became a copy.");
    }
}

public static class Program
{
    public static void Main() => new ParcelCounter().Verify();
}
