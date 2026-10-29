// snippet:bootstrap
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

if (args is ["--courier-child"]) { Console.WriteLine("Gannet courier ready"); return; }
#if NET11_0
const string version = "11.0.100-rc.2.26504.105";
#else
const string version = "10.0.402";
#endif
string sdkRoot = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "sdk", version);
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string path = Path.Combine(sdkRoot, name.Name + ".dll");
    return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
await Run(sdkRoot);
[MethodImpl(MethodImplOptions.NoInlining)]
static async Task Run(string root)
{
    await Gannet.WorkshopDemo.Run();
#if GANNET_PROOF
    await Gannet.WorkshopChecks.Run(root);
#endif
}
// endsnippet:bootstrap
