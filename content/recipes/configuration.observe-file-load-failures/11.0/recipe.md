# Observe configuration load failures separately from changes

## Goal

A planetarium loads its display schedule from `display.json`. Report a failed
load at the provider boundary, and treat a change token only as a signal to read
the provider again. A token proves neither valid input nor completed consumer work.

## Older approach

The .NET 10 JSON provider already reports parse and missing-required-file errors
through `OnLoadException`. File-open failures escape that callback. This policy
reports an explicit open failure at the call site and deliberately ignores reported
load errors. Its automatic reload path does not promise to contain every exception.

{{snippet:older}}

Use a real `PhysicalFileProvider` rooted at the schedule directory, pass it to
`Create`, own the returned provider with `using`, and call `Load`. The file contains
a JSON object with a `Display` string, for example the planetarium's current show.
Read `provider.TryGet("Display", out var show)` after a successful load. An ignored
error is an availability decision; applications that require valid settings should
reject startup or explicitly stop their own work instead.

## Newer approach

.NET 11 extends the callback to file-open exceptions, including arbitrary custom
`IFileInfo.CreateReadStream` exceptions. Keep the full `Exception` type and choose
`Ignore` explicitly. A throwing reporter changes the loading outcome; keep the
reporter safe and synchronous. Set `Ignore=false` to preserve propagation from
explicit `Load()`; automatic reload reports then catches escaped load failures in
the current provider. No global unobserved-task event is needed for reporting.

{{snippet:newer}}

The actual paired JSON consumers measured the following matrix. “Notify” means
the token captured before the operation changes, not a fixed callback count per
OS write. Explicit `Load()` always uses the initial-style path, even after a
successful load. The JSON parser retains its prior dictionary on that parse failure;
other parsers can modify data before throwing.

| Operation | Data after ignored failure | .NET 10 notify | RC2 notify |
| --- | --- | --- | --- |
| Successful initial load or reload | New parsed values | Yes | Yes |
| Initial malformed JSON | Empty | Yes | No |
| Explicit malformed Load after success | Previous value | Yes | No |
| Missing required file in explicit Load | Previous value, or empty before first success | Yes | No |
| Missing optional file | Cleared | Yes | Yes |
| Malformed automatic reload after success | Cleared | Yes | Yes |
| Windows exclusive-lock open failure | Previous value | No; callback not reached | No; callback reached |
| Delete required file during automatic reload | Cleared; reload treats it as optional | Yes | Yes |
| Recreate with valid JSON | New values | Yes | Yes |

With `Ignore=false`, explicit parse/missing errors throw before notification.
RC2 open failures also throw when unignored and preserve existing data without
notification. An unignored malformed automatic reload clears data but does not
notify; the RC2 provider contains the escaped failure. The separately reaped
children on both releases report the callback and remain alive after this physical
reload. The old implementation does not provide the new provider-level catch;
its observed physical notification path must not be generalized to every custom provider.

## Migration

1. Keep the same required/optional file policy. Reference
   `Microsoft.Extensions.Configuration.Json`; the checked current version is
   `11.0.0-rc.2.26504.105`, with net11 assets. The old comparison locks 10.0.13.
2. Create the physical file provider and the JSON provider with a reporting callback.
   Call `Load`, check required values and apply an explicit invalid-settings policy.
3. Replace unconditional exception casts with type inspection or full exception
   logging. Keep `Ignore` deliberate; neither true nor false creates a fallback cache.
4. Register consumers against `GetReloadToken`, re-read values after a notification,
   and handle missing/cleared values. For async consumer coalescing and failure
   handling use the existing `configuration.await-change-callback` guidance
   ([PIN-745](https://linear.app/pinguapps/issue/PIN-745)); loading and consumer
   completion are separate events. Do not duplicate that registration lifecycle here.
5. Exercise malformed initial input, malformed reload, deletion/recreation and real
   open failure on the deployment filesystem. Dispose consumer registrations,
   configuration providers and the physical provider when finished.

## Constraints

Measured on Windows x64 NTFS with actual JSON files, SDK10.0.402/C#14/runtime10.0.13
and SDK11.0.100-rc.2.26504.105/C#15/runtime11RC2. The full packages/selected assets
and compiler/reference/runtime hashes are recorded. FileExtensions and Json are
not made available merely because some Extensions abstractions joined net11's
shared framework. No newer-package/older-target route is taught.

The source note's broad ignored-failure headline needs the reload qualification
above. The callback does not surround `GetFileInfo` or stream disposal. A custom
provider can fail outside it; the separately injected `GetFileInfo` fault escapes
without a callback. Direct parser `Load(Stream)` is a different boundary.
No cross-platform lock mechanism, OS event order/count, browser support, persistent
last-known-good cache or asynchronous options-validation contract is promised.
There is no required workload, rendering mode or experimental opt-in.

## Evidence

`eng/Record-FileChain.ps1 -VerifySources` captures paired real initial/explicit/
automatic operations, exception types, Ignore, dictionary state and pre-operation
tokens. Real Windows `FileShare.None` proves IO failure and recovery. The custom
stream exception and `GetFileInfo` fault are labelled **synthetic**, not physical
IO. Bounded conditions/registrations replace assumptions about per-write event
counts. The unignored automatic-reload case also runs in a bounded, reaped child on each release.
Providers/registrations are disposed and temporary files removed. Exact dependency
closures, commands, inputs and artifacts remain in the saved sample records.

The ordinary packed/local-feed-installed tool must return this complete canonical
body offline through real stdio list/get. RC2 is preparation evidence; PIN-959,
PIN-962 and existing Human gates remain open, and the exact eight foundations
are unchanged. Official references and applicability accompany this body offline.
