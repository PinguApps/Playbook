# Handle Android request faults and cancellation

## Goal

Handle AndroidMessageHandler transport failures and cancellation during headers and body reads while retaining inner diagnostics.

## Newer approach

Breakwater reads a harbour bulletin using an explicitly selected AndroidMessageHandler.
Reuse the client for its owner's lifetime. Pass the same operation token to
GetAsync and ReadAsStringAsync: ResponseHeadersRead completes before body consumption,
so cancelling only the headers request is insufficient. Dispose the response after
the awaited read. Call EnsureSuccessStatusCode to make non-success status codes
HttpRequestException failures; a 404 response is otherwise an ordinary response.

Catch caller cancellation before faults and rethrow with `throw;` to preserve its
stack. A cancellation whose caller token is not cancelled can represent a timeout;
handle that separately according to the application's timeout policy. Do not classify
every OperationCanceledException as a user action. No retry policy is implied.

Transport/protocol failures now surface as HttpRequestException. For mapped legacy
failures, InnerException retains WebException and its Status, with the Java cause
further inside. Preserve the complete exception rather than replacing it with its
message or manufacturing a new exception. A status failure produced by
EnsureSuccessStatusCode need not have that transport inner exception.
Do not log credentials, authorization headers or unredacted sensitive URLs.

{{snippet:net11-requestreader-cs}}

## Older approach

The .NET10 AndroidMessageHandler can throw WebException directly for transport
failures. Its sample retains that catch alongside HttpRequestException for status
failures. This was valid handling for that release. SocketsHttpHandler has its own
contracts; selecting it does not prove AndroidMessageHandler behavior. Do not add
automatic redirect/disposal fixes as independent migration steps.

## Migration

1. Recreate the complete canonical files below, retaining their isolation and SDK pins.
2. Replace WebException-only handling with the current exception branches. Preserve
   inner diagnostics and propagate cancellation through both request and body operations.
3. On an available Android runtime, use an owned endpoint which can stall before
   headers, stall after headers, return a 503, and close a connection prematurely.
   Cancel during each stall and observe cancellation; inspect the full exception
   chain for transport faults and StatusCode for 503. Use explicit endpoint signals
   to coordinate cancellation rather than assuming a delay proves the read started.

## Evidence

The .NET10 app compiles with the actual Android workload. Pinned RC2 handler source
supports the exception mapping and cancellation-aware body stream. Android device
execution, the controlled-endpoint cases above and .NET11 native compilation are
**not-tested** under PIN-699. These are instructions for checking behavior, not
reported runtime results. No mocked handler or portable HTTP result establishes
the Android contract. No pending Human proof is owed for these unavailable checks.

## Constraints

Requires .NET11 Android, C#15, Android API24+, matching Android SDK and JDK21/25.
Gradle invoked using JDK25 needs9.1+. Canonical SDK is RC3; reviewed platform
assets are RC2 37.2.0-rc.2.84, not a tested RC3 composition. No extra NuGet
package, external provider/account or Blazor rendering mode is required. The
.NET10 comparison is not a supported older-target catalogue route. NativeAOT
is experimental and outside this sample. RC evidence does not certify GA.

## Canonical samples

### samples/net10.0/BreakwaterRequests/BreakwaterRequests.csproj

{{snippet:net10-breakwaterrequests-csproj}}

### samples/net10.0/BreakwaterRequests/Directory.Build.props

{{snippet:net10-directory-build-props}}

### samples/net10.0/BreakwaterRequests/Directory.Build.targets

{{snippet:net10-directory-build-targets}}

### samples/net10.0/BreakwaterRequests/Directory.Packages.props

{{snippet:net10-directory-packages-props}}

### samples/net10.0/BreakwaterRequests/global.json

{{snippet:net10-global-json}}

### samples/net10.0/BreakwaterRequests/MainActivity.cs

{{snippet:net10-mainactivity-cs}}

### samples/net10.0/BreakwaterRequests/RequestReader.cs

{{snippet:net10-requestreader-cs}}

### samples/net10.0/BreakwaterRequests/Properties/AndroidManifest.xml

{{snippet:net10-androidmanifest-xml}}

### samples/net11.0/BreakwaterRequests/BreakwaterRequests.csproj

{{snippet:net11-breakwaterrequests-csproj}}

### samples/net11.0/BreakwaterRequests/Directory.Build.props

{{snippet:net11-directory-build-props}}

### samples/net11.0/BreakwaterRequests/Directory.Build.targets

{{snippet:net11-directory-build-targets}}

### samples/net11.0/BreakwaterRequests/Directory.Packages.props

{{snippet:net11-directory-packages-props}}

### samples/net11.0/BreakwaterRequests/global.json

{{snippet:net11-global-json}}

### samples/net11.0/BreakwaterRequests/MainActivity.cs

{{snippet:net11-mainactivity-cs}}

### samples/net11.0/BreakwaterRequests/RequestReader.cs

{{snippet:net11-requestreader-cs}}

### samples/net11.0/BreakwaterRequests/Properties/AndroidManifest.xml

{{snippet:net11-androidmanifest-xml}}

