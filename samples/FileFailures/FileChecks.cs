internal static class FileChecks
{
    internal static void Contract(Type type)
    {
        bool experimental = type.CustomAttributes.Concat(type.GetConstructors().SelectMany(c => c.CustomAttributes))
            .Concat(type.GetMethods().SelectMany(m => m.CustomAttributes))
            .Any(a => a.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.ExperimentalAttribute");
        Check(!experimental, "unexpected experimental contract");
        Console.WriteLine($"CONTRACT {type.FullName}; assembly={type.Assembly.GetName()}; experimental={experimental}");
    }
    internal static Exception? Catch(Action action) { try { action(); return null; } catch (Exception e) { return e; } }
    internal static void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); }
    internal static async Task Until(Func<bool> condition, string name)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(12));
        while (!condition()) { try { await Task.Delay(20, deadline.Token); } catch (OperationCanceledException) { throw new TimeoutException(name); } }
    }
}
