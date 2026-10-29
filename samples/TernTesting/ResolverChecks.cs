// snippet:resolver
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Resolution = Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces.AssemblyResolveEventArgs;

namespace TernTesting;

internal static class ResolverChecks
{
    internal static Resolution Context(string name, Assembly? requestor)
    {
#if LEGACY_PLATFORM
        return new Resolution(name) { RequestingAssembly = requestor };
#else
        return new Resolution(name, requestor);
#endif
    }
    public static void Run(string directory)
    {
        directory = Path.GetFullPath(directory);
        var bridgeCalls = 0;
        Assembly? Resolve(Resolution? args)
        {
            if (args?.Name is null) return null;
            var name = new AssemblyName(args.Name).Name;
            if (name != "Tern.Rules") return null;
            if (args.RequestingAssembly?.GetName().Name != "Tern.Plugin") throw new InvalidOperationException("lost requestor");
            Interlocked.Increment(ref bridgeCalls);
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(directory, "rules", "Tern.Rules.dll"));
        }
        ResolveEventHandler bridge = (_, args) => Resolve(Context(args.Name, args.RequestingAssembly));
        AppDomain.CurrentDomain.AssemblyResolve += bridge;
        try
        {
            var plugin = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(directory, "Tern.Plugin.dll"));
            var value = plugin.GetType("Tern.Plugin.Entry", true)!.GetMethod("Read")!.Invoke(null, null);
            if (!Equals(value, "west:13") || Volatile.Read(ref bridgeCalls) != 1) throw new InvalidOperationException("plugin dependency not resolved");
            if (Resolve(Context("Tern.Missing", plugin)) is not null || Context("Tern.Missing", null).RequestingAssembly is not null)
                throw new InvalidOperationException("missing/null contract changed");
            try { Assembly.Load("Tern.Missing"); throw new InvalidOperationException("missing assembly unexpectedly loaded"); }
            catch (FileNotFoundException) { }
            Console.WriteLine("PASS real-plugin;dependency=Tern.Rules;requestor=Tern.Plugin;result=west:13;missing-rejected");
        }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= bridge; }

        // Actual .NET PlatformAssemblyResolver uses ALC.Resolving: no requestor is available.
        var platformCalls = 0;
        using (var resolver = new PlatformAssemblyResolver())
        {
            resolver.AssemblyResolve += (_, args) =>
            {
                if (args?.Name != "Tern.PlatformMissing") return null;
                if (args.RequestingAssembly is not null) throw new InvalidOperationException("invented ALC requestor");
                Interlocked.Increment(ref platformCalls);
                return null;
            };
            try { Assembly.Load("Tern.PlatformMissing"); }
            catch (FileNotFoundException) { }
        }
        var before = Volatile.Read(ref platformCalls);
        try { Assembly.Load("Tern.PlatformMissing"); }
        catch (FileNotFoundException) { }
        if (before != 1 || Volatile.Read(ref platformCalls) != before) throw new InvalidOperationException("platform event/disposal differs");
        Console.WriteLine("PASS platform-resolver;requestor=null;missing-rejected;disposed");
    }
}
// endsnippet:resolver
