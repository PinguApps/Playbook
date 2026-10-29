// snippet:bootstrap
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Depot.Events;

#if NET11_0
const string sdk = "11.0.100-rc.2.26504.105";
#else
const string sdk = "10.0.402";
#endif
string sdkRoot = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "sdk", sdk);
// This reader is bound to a complete, hash-checked SDK, not a redistributable MSBuild host.
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string file = Path.Combine(sdkRoot, name.Name + ".dll");
    return File.Exists(file) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(file) : null;
};
Run(sdkRoot);

[MethodImpl(MethodImplOptions.NoInlining)]
static void Run(string sdkRoot) => DepotProof.Run(sdkRoot);
// endsnippet:bootstrap
