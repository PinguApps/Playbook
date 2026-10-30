using System;
using Microsoft.CSharp.RuntimeBinder;

namespace Playbook.Samples;

// snippet:policy
#pragma warning disable CA2225 // The migration specifically demonstrates logical operators.
public interface ILoadingPolicy
{
    static bool operator true(ILoadingPolicy value) => false;
    static bool operator false(ILoadingPolicy value) => false;
}

public sealed class LoadingPolicy(bool permits) : ILoadingPolicy
{
    public bool Permits { get; } = permits;
    public static bool operator true(LoadingPolicy value) => value.Permits;
    public static bool operator false(LoadingPolicy value) => !value.Permits;
    public static LoadingPolicy operator &(LoadingPolicy left, LoadingPolicy right)
        => new(left.Permits && right.Permits);
    public static LoadingPolicy operator |(LoadingPolicy left, LoadingPolicy right)
        => new(left.Permits || right.Permits);
}

public sealed class UnknownPolicy : ILoadingPolicy { }
#pragma warning restore CA2225

public static class LoadingDecision
{
    public static LoadingPolicy Concrete(ILoadingPolicy left, Func<dynamic> right, bool either)
    {
        // Reject an incompatible implementation before attempting dynamic dispatch.
        if (left is not LoadingPolicy known)
            throw new ArgumentException("This route requires LoadingPolicy.", nameof(left));
        return either ? known || right() : known && right();
    }

    public static LoadingPolicy LateBound(ILoadingPolicy left, Func<dynamic> right, bool either)
    {
        dynamic operand = left;
        return either ? operand || right() : operand && right();
    }
}
// endsnippet:policy

#if INTERFACE_LEFT
// snippet:interface-left
public static class LegacyDecision
{
    public static object All(ILoadingPolicy left, dynamic right) => left && right;
    public static object Any(ILoadingPolicy left, dynamic right) => left || right;
}
// endsnippet:interface-left
#endif

public static class Program
{
    public static void Main()
    {
#if INTERFACE_LEFT
        foreach (bool either in new[] { false, true })
        {
            try
            {
                ILoadingPolicy left = new LoadingPolicy(true);
                dynamic right = new LoadingPolicy(false);
                _ = either ? LegacyDecision.Any(left, right) : LegacyDecision.All(left, right);
                throw new InvalidOperationException("Expected old runtime binding failure.");
            }
            catch (RuntimeBinderException exception)
            {
                Console.WriteLine($"EXPECTED legacy {(either ? "||" : "&&")}: {exception.GetType().Name}: {exception.Message}");
            }
        }
#else
        foreach (bool lateBound in new[] { false, true })
        foreach (bool either in new[] { false, true })
        foreach (bool left in new[] { false, true })
        foreach (bool right in new[] { false, true })
        {
            int calls = 0;
            dynamic ReadRight() { calls++; return new LoadingPolicy(right); }
            ILoadingPolicy policy = new LoadingPolicy(left);
            LoadingPolicy result = lateBound
                ? LoadingDecision.LateBound(policy, ReadRight, either)
                : LoadingDecision.Concrete(policy, ReadRight, either);
            bool expected = either ? left || right : left && right;
            int expectedCalls = (either ? !left : left) ? 1 : 0;
            if (result.Permits != expected || calls != expectedCalls)
                throw new InvalidOperationException("Decision or short-circuit count changed.");
            Console.WriteLine($"PASS {(lateBound ? "dynamic" : "concrete")} {(either ? "||" : "&&")} left={left} right={right} result={result.Permits} rightCalls={calls}");
        }
        try
        {
            _ = LoadingDecision.Concrete(new UnknownPolicy(), () => new LoadingPolicy(true), false);
            throw new InvalidOperationException("Concrete route accepted unknown policy.");
        }
        catch (ArgumentException) { Console.WriteLine("PASS concrete: unknown implementation rejected"); }
        try
        {
            _ = LoadingDecision.LateBound(new UnknownPolicy(), () => new LoadingPolicy(true), false);
            throw new InvalidOperationException("Dynamic route accepted missing operators.");
        }
        catch (RuntimeBinderException) { Console.WriteLine("PASS dynamic: missing operators fail at runtime"); }
#endif
    }
}
