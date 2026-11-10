# Replace synchronous mobile HTTP sends

## Goal

Moorland's field app retrieves an observation without blocking the caller while
network I/O runs. Await the request, propagate cancellation and dispose each
response. Apply this when migrating Android, iOS or tvOS synchronous Send callers.

## Older approach

SendAsync already exists on .NET 10. This original comparison deliberately uses
that supported asynchronous approach; it does not claim synchronous requests
worked on a mobile handler before. Existing asynchronous code remains valid.

{{snippet:older}}

A legacy client.Send(request) call blocks its calling thread. Replace that call
with awaited SendAsync rather than wrapping it in Task.Run, Result, Wait or
GetAwaiter().GetResult. Such wrappers do not make an unsupported handler support
synchronous I/O, and blocking on tasks can deadlock a UI.

## Newer approach

The .NET 11 recipe keeps the same asynchronous operation. The change is that
the synchronous APIs now disclose additional unsupported mobile platforms.

{{snippet:newer}}

Call await MoorlandReports.ReadAsync(sharedClient, endpoint, cancellationToken)
from the same nullable/implicit-usings assembly. Reuse a suitably configured
HttpClient, creating a fresh request per operation. The helper disposes the
request and response, but does not dispose the caller's client. It uses the
default ResponseContentRead behavior, buffering response content before returning
from SendAsync; do not use this small-text example for unbounded payloads.

## Migration

1. Find synchronous HTTP call sites and their callers. The audited .NET 11
   System.Net.Http reference adds android, ios and tvos unsupported annotations
   to all four HttpClient.Send overloads, HttpMessageInvoker.Send and the
   HttpClientHandler.Send override. Browser remains unsupported as before.
   This is not a declaration that every arbitrary custom handler's Send method
   gained the same annotation, or that all HTTP operations are unsupported.
2. Replace the send with SendAsync and pass a CancellationToken. Await through
   the caller chain; use task-returning application methods. An actual UI event
   handler may await and report errors at its boundary. Avoid async void outside
   event-handler boundaries. Do not suppress platform warnings to retain blocking.
3. Inspect the status before consuming a successful report. This helper uses
   EnsureSuccessStatusCode; applications needing error bodies should read and
   handle them explicitly before disposal. Do not interpret every HTTP response
   as success. Handle HttpRequestException at the application boundary.
4. Pass cancellation to content reads too. Distinguish user cancellation from
   timeout or transport failure using the caller token and your timeout policy;
   do not report every OperationCanceledException as a user action. No automatic
   retries are taught, particularly for operations with side effects.
5. Keep the app's native handler, permissions, TLS and lifecycle policy. Mobile
   networking may require platform manifest permissions and transport/security
   configuration. The loopback desktop HTTP fixture does not justify cleartext
   production networking, a particular native handler, or UI responsiveness.
6. Build the real application using its matched SDK/workload and native tooling.
   The guidance targets .NET11 with canonical C#15.0; the comparison uses
   .NET10/C#14.0. No extra package, experimental opt-in or rendering mode is
   required by this helper. Native app/toolchain requirements remain those of
   the app. Do not retarget a mobile app to desktop merely to remove a warning.

## Constraints

Awaiting removes this explicit blocking call; it does not guarantee scheduling,
responsiveness or support for every platform handler. Keep request size/time
budgets, authentication and certificate validation in application policy. Dispose
responses on both success and failure and avoid reusing a sent request message.

## Evidence and explicit native exclusions

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

eng/Test-ProviderRequests.ps1 -Record compiles and executes both desktop consumers
with SDK10.0.403/runtime10.0.14 and SDK11.0.100-rc.3.26522.148/runtime11 RC3.
The real controlled loopback server checks a successful report, 503 status,
in-flight caller cancellation and a transport disconnect. The script reconciles
actual reference assets with the audit and records compiler/runtime identities.
The synthetic SupportedOSPlatform("android") caller produces CA1416 for
synchronous Send on .NET11 and no such mobile warning on .NET10; the async helper
remains valid. This is desktop analyzer evidence, not native-target compilation.

For these exact MoorlandReports samples, actual Android/iOS/tvOS target builds,
native platform warnings, device/emulator execution, native-handler success,
cancellation, status/transport failure and UI responsiveness are **not-tested**:
the required devices/emulators and Apple toolchains are unavailable here.
PIN-699 permits these sample-specific exclusions without another environment
prerequisite; no native execution or mobile support claim follows from Windows
execution. Pingu owns feasible interactive approvals and refresh coordination.
No recurring device CI matrix is added.

The annotation audit uses both pinned official System.Net.Http reference sources;
source links and hashes are retained with metadata and sample inputs. Neither
metadata presence nor a synthetic platform attribute substitutes for native proof.
This is RC2 evidence, not final GA. PIN-959 and PIN-962 remain open, the eight
.NET10 foundations remain unchanged, and no down-level package route is needed.
