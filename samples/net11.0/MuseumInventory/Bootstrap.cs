// snippet:bootstrap
using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;

#if NET11_0
const string sdkVersion = "11.0.100-rc.2.26504.105";
#else
const string sdkVersion = "10.0.402";
#endif
string dotnet = Environment.ProcessPath!;
string sdkDirectory = Path.Combine(Path.GetDirectoryName(dotnet)!, "sdk", sdkVersion);
MSBuildLocator.RegisterMSBuildPath(sdkDirectory);
RunAfterRegistration();

// Keep MSBuild types out of the registration method's JIT compilation.
[MethodImpl(MethodImplOptions.NoInlining)]
void RunAfterRegistration() => InventoryReader.Read(Path.Combine(AppContext.BaseDirectory, "Museum.csproj"));
// endsnippet:bootstrap

MuseumChecks.Run(dotnet, sdkDirectory, sdkVersion);
