// snippet:bootstrap
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

#if NET11_0
const string sdk = "11.0.100-rc.2.26504.105";
#else
const string sdk = "10.0.402";
#endif
string sdkRoot = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "sdk", sdk);
// Use the complete qualified SDK installation, never a mixed package/SDK closure.
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string path = Path.Combine(sdkRoot, name.Name + ".dll");
    return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
await Run(sdkRoot);

[MethodImpl(MethodImplOptions.NoInlining)]
static Task Run(string sdkRoot) => Foundry.Staging.StagingChecks.Run(sdkRoot);
// endsnippet:bootstrap
