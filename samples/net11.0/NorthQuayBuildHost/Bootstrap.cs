// snippet:bootstrap
using NorthQuay.Building;

string sdk = Environment.GetEnvironmentVariable("QUAY_MSBUILD_SDK")
    ?? throw new InvalidOperationException("Set QUAY_MSBUILD_SDK to the complete SDK 11.0.100-rc.2.26504.105 directory");
// Only this disposable host process changes. No Locator/dynamic assembly loading under AOT.
Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(sdk, "MSBuild.dll"));
return PermitEngine.Run(args);
// endsnippet:bootstrap
