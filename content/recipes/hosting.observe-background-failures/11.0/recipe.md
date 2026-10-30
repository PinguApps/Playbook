# Preserve background-service failures at the host boundary

## Goal

A fictional aquarium imports scheduled sensor readings. If its importer fails
after startup, stop the application and preserve a failure result for the process
supervisor. Distinguish the worker failure, the host's log and the application's
chosen exit code; a log entry alone is not a process failure policy.

## Shared worker and host setup

Create an SDK console project with implicit usings and nullable enabled. Add
`Microsoft.Extensions.Hosting`, pinned to **10.0.13** for net10/C#14 or
**11.0.0-rc.2.26504.105** for net11/C#15. Full Hosting remains a package on net11;
moving some Extensions abstractions into the framework does not remove it.
Put each complete snippet in its own `.cs` file. This original worker accepts the
application's asynchronous import operation and records failures without replacing
their exception identity. It filters only stopping-token cancellation:

{{snippet:worker}}

The standard Generic Host implementation below disables external default config
loading for this example and explicitly selects StopHost. Add the real import
operation before building; no custom IHost is involved.

{{snippet:setup}}

## Older approach: .NET 10

The host logs a background failure and requests shutdown under StopHost, but the
run/stop boundary can still complete successfully. A separate application journal
bridges that gap. Keep it alive independently of the disposed host and combine
background failures with a shutdown failure when reporting the process outcome:

{{snippet:older}}

This remains a valid older approach. Every participating importer must use the
same journal; it does not capture arbitrary uninstrumented services. The journal
is a small bounded-lifetime example, not a retry queue or durable error store.
Under Ignore this example reports only failures actually thrown at the boundary;
it does not reinterpret every logged failure as a shutdown request.

## Newer approach: .NET 11

The existing RunAsync API now propagates failures collected by the default host's
StopHost path. A single worker exception retains its identity; multiple worker
and/or shutdown failures can aggregate. The explicit boundary logs the exception
and returns a chosen nonzero code instead of swallowing it into success:

{{snippet:newer}}

For a runnable bounded demonstration, this Program uses the same classes above.
Replace the controlled gate and deliberate error with the aquarium's actual
scheduled import operation. The gate proves startup before releasing the failure:

{{snippet:program}}

`return await ...` from top-level Program makes 17 the process exit here. A library
method returning 17 alone cannot change the process exit. The independent logger
factory outlives RunAsync's host disposal, so boundary reporting remains usable.
An application can also leave the exception uncaught; its OS/supervisor decides
the resulting numeric exit representation. No universal numeric code is promised.

### Observed host boundaries

The paired real child processes use the same signalled failure after
ApplicationStarted. Explicit lifecycle cases also await StartAsync successfully.
Each raw route uses a diagnostic boundary that records the exception and maps a
caught failure to 17; it does not suppress it into exit 0.

| StopHost case | .NET 10 raw run/stop/wait | .NET 11 RC2 raw run/stop/wait |
| --- | --- | --- |
| One importer fails | logged, no boundary exception, exit 0 | original SensorImportException, exit 17 |
| Two importers fail | both logged, no boundary exception, exit 0 | AggregateException containing both original objects, exit 17 |
| Importer plus shutdown failure | background logged; shutdown exception alone, exit 17 | aggregate of both original objects, exit 17 |
| Ordinary stopping cancellation | no failure log/exception, exit 0 | no failure log/exception, exit 0 |
| Ignore and a failed importer | logged; host still running until explicit stop, exit 0 | logged; host still running until explicit stop, exit 0 |

The raw routes are `RunAsync`, synchronous `Run`, explicit `StartAsync` followed by
`IHost.StopAsync(CancellationToken)`, timed `StopAsync(TimeSpan)`, and explicit start
followed by `WaitForShutdownAsync` or synchronous `WaitForShutdown`. The default
host implementation and its stop path matter; these are not new API signatures.
The demonstrated .NET10 journal boundary instead returns 17 for its StopHost worker
failures and preserves their identities, including the shutdown combination.

The two-worker check releases both after the startup signal and verifies both
original objects without relying on their race-dependent aggregate ordering.
The current host waits for background monitors while collecting stop errors;
bounded shutdown cancellation can limit that wait. Startup errors are separate:
the paired StartAsync failure control already throws on both releases. Never
describe the earlier API as incapable of throwing.

## Migration

1. Inventory the actual host implementation, Hosting package and exception policy.
   Retain StopHost when failures should stop the importer; Ignore is not a success workaround.
2. Identify which layer owns process exit. Either allow the boundary exception to
   escape or explicitly log it and return a nonzero Main result, as shown above.
3. For .NET10 services, retain an explicit failure journal if successful raw host
   completion must still report failed work. On .NET11 the default StopHost path
   provides this propagation; do not duplicate collected exceptions in the boundary.
4. With manual lifecycle ownership, await StartAsync, then wait/stop in a try block
   and dispose the host in finally. Run/RunAsync already dispose it, including their
   failure path. Do not inspect disposed host services in the catch.
5. Verify one/multiple failures after the startup signal, shutdown failure, normal
   cancellation and Ignore. Assert thrown identity, failure logs and process exit
   separately. Bound and reap every test child; do not substitute timer sleeps for startup proof.

## Constraints

Use the default Generic Host with Microsoft.Extensions.Hosting's actual net11
package asset. Its dependency closure also uses framework-provided abstractions;
exact resolved packages and assets are recorded separately. Reproduction uses
SDK10.0.402 / C#14 / runtime-ref10.0.13 and SDK11.0.100-rc.2.26504.105 / C#15 /
runtime-ref11RC2, with the package pins above. Consumer requirements are the stated
release-family SDK/runtime and package contract, not a promise that every preview
contains the same implementation.

No native library/toolchain, workload, provider, account, rendering mode or external
service is needed. These selected host APIs are not Experimental and need no opt-in.
This is neither browser-host equivalence nor durable background scheduling; browser
application lifetime has separate guidance. Custom IHost implementations, other
platforms, service managers and newer-package-on-net10 routes require their own proof.
Keep normal cancellation separate from failed work; do not blanket-catch every
OperationCanceledException as success regardless of its token or lifecycle.

## Evidence

The sample executes 35 actual child processes per release: seven boundaries times
five scenarios. Both real dependency sets compile/run. Checks capture exact thrown
type/reference identity, aggregate membership, separate host failure logs, OS child
exit, normal cancellation, Ignore remaining unstopped, service disposal and child
cleanup. A separate startup failure proves the older StartAsync throwing boundary.
Canonical setup, worker and boundary classes are exercised by these checks; the
bounded Program runs in its own child as well.

`eng/Record-CalibrationHost.ps1` saves exact commands, input hashes, locked closures,
compiler/reference/runtime/selected package asset identities in the existing sample
evidence records. Only Windows x64 CoreCLR JIT is measured. RC2 is not actual-GA
proof; PIN-962, comprehensive PIN-959 and existing Human gates stay open. The .NET10
comparison preserves the exact eight foundations and adds no target10 catalogue edition.

## Official references

- [Pinned full host compatibility note](https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/ihost-runasync-stopasync-throw-backgroundservice-failure.md).
- [Package-matched current Host implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/Microsoft.Extensions.Hosting/src/Internal/Host.cs).
- [Previous Host implementation](https://github.com/dotnet/dotnet/blob/4099f83977f4d29604a9aeb971fb0a7bc1d5f699/src/runtime/src/libraries/Microsoft.Extensions.Hosting/src/Internal/Host.cs).
- [Run/wait/stop wrappers and disposal](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/HostingAbstractionsHostExtensions.cs).
