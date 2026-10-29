// snippet:legacy-bootstrap
using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;
using NorthQuay.Building;

string sdk = Environment.GetEnvironmentVariable("QUAY_MSBUILD_SDK")
    ?? throw new InvalidOperationException("Set QUAY_MSBUILD_SDK to the complete SDK 10.0.402 directory");
MSBuildLocator.RegisterMSBuildPath(sdk);
// The older host resolves this fixed SDK by its conventional directory layout.
Environment.SetEnvironmentVariable("MSBuildSDKsPath", Path.Combine(AppContext.BaseDirectory, "Sdks"));
return AfterRegistration(args);

[MethodImpl(MethodImplOptions.NoInlining)]
static int AfterRegistration(string[] arguments) => PermitEngine.Run(arguments);
// endsnippet:legacy-bootstrap
