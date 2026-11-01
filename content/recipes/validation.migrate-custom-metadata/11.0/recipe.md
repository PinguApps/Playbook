# Migrate an authored validation resolver to new metadata contracts

## Goal

Tidepool Aquarium imports tank maintenance reports in a console host. Its custom
resolver validates a report code and nested pump serial. Migrate the extension
without losing service failures, asynchronous completion, cancellation or exact
error ownership. This is a bounded extension-author example, not a generic
reflection validator or web request adapter.

## Older approach

The actual .NET10 Validation package exposes experimental ASP0029 abstract
ValidatableTypeInfo, ValidatablePropertyInfo and ValidatableParameterInfo bases.
They implement IValidatableInfo.ValidateAsync; there is no synchronous metadata
entry point. The valid older approach can remain on its pinned dependency set.
The narrowly scoped pragma acknowledges that older experimental extension
contract; do not suppress all analyzer warnings across the application.

{{snippet:older}}

Both versions use this complete bounded service. With its default ready state,
Check succeeds immediately; the tests also hold its decision pending, release
it, fail it and cancel the wait. Replace it with your actual maintenance service
while preserving completion/failure/cancellation semantics.

{{snippet:ledger}}

Put these in separate source files in a nullable, implicit-usings net10.0/C#14
console app. Reference Microsoft.Extensions.Validation,
Microsoft.Extensions.DependencyInjection and Microsoft.Extensions.Options at
10.0.13; restore and retain the lock file. No ASP.NET framework reference or web
host is needed. Create a ServiceCollection, register `new MaintenanceLedger()`
with AddSingleton, build/dispose its provider, and await
`ReefImport.CheckAsync(new TankReport(), provider, cancellationToken)`.
The host explicitly awaits its external check before invoking the old metadata.
No sync-over-async route is advertised.

## Newer approach

The current package replaces those bases with typed interfaces. Implement their
synchronous and asynchronous operations; implement property lookup for the
properties this bounded resolver exposes. A property receives its containing
object. Error messages are structured ValidationError entries collected by path.

{{snippet:newer}}

Use a nullable, implicit-usings net11.0/C#15 console project. The verified package
composition is Microsoft.Extensions.Validation and
Microsoft.Extensions.DependencyInjection 11.0.0-rc.2.26504.105. Validation brings
Localization.Abstractions at the same version; the base framework supplies
Options and the moved abstractions. The actual Validation archive has only a
net11 asset; restoring it into net10 fails NU1202. Do not claim a net10 backport.
No workload, native provider, UI/rendering mode or experimental API opt-in is
needed by this current custom resolver. Current contracts are unmarked; the old
comparison remains explicitly experimental.

Call `await ReefExample.CheckAsync(new TankReport(), CancellationToken.None)` in the same assembly. The complete host is:

{{snippet:host}}

## Migration

1. Qualify the exact package/target before editing. The old source fails against
   RC2 with missing base types and incompatible resolver outputs/context members.
   Adding a ValidateAsync method alone cannot restore removed types. Keep the
   old working project as the comparison; migrate into the net11 consumer.
2. Change resolver outputs from IValidatableInfo to IValidatableTypeInfo and
   IValidatableParameterInfo. Return false/null for unknown types or parameters;
   do not intercept unrelated metadata. Keep resolver order deliberate because
   the first successful resolver wins. Our resolver only owns TankReport,
   PumpReport and ReefImport.Accept's report parameter.
3. Implement Validate and ValidateAsync on type, property and parameter metadata.
   Implement TryFindProperty with true/non-null for supported string properties
   and false/null otherwise. In this bounded example Code and Serial are exposed;
   Pump is traversed internally rather than exposed for individual validation.
   Delegate the async operation only when it completes synchronously, or await
   the actual service with the caller's token. Never block on unfinished async
   work in the sync entry point; this ledger throws and tells callers to use async.
4. Put services on ValidateContext.ServiceProvider instead of constructing its
   removed ValidationContext property. Required missing MaintenanceLedger fails
   explicitly. A missing optional localization factory falls back to the original
   message in the generated validation path. Do not silently accept missing
   required application dependencies.
5. Replace string-array dictionaries and OnValidationError subscriptions with
   ValidationErrors and AddValidationError. Set Name, full Path, ErrorMessage and
   Container explicitly. Code belongs to the TankReport; Serial belongs to its
   PumpReport. Multiple AddValidationError calls append at a shared path. Treat
   null and empty results as success; the dictionary is a read-only view, not a
   mutable output property. Use a fresh context per validation operation.
6. Restore the path after traversing a nested object, including on failure.
   The demonstrated graph has a fixed depth and non-null Pump; it is not a
   general cycle/collection/null-policy implementation. A reusable arbitrary
   graph resolver must define those policies and enforce depth itself; setting
   ValidationOptions.MaxDepth alone does not implement custom traversal.
7. Keep generated metadata alongside the narrow resolver. Mark an accessible
   model with [ValidatableType], add public readable validated properties, call
   AddValidation in that project and enable its generated interceptor namespaces:

   {{snippet:interceptors}}

   Generated resolvers are inserted before configured fallbacks. The executable
   checks a separate InspectionLabel with [Required] while our custom report
   resolver still works. Private marked nested types produce ASP0033; inaccessible
   validated properties produce ASP0035; a marker without AddValidation produces
   ASP0038. Fix accessibility/registration and rebuild; do not suppress them.
8. For generated localized validation, register IStringLocalizerFactory and set
   `options.LocalizerProvider = (type, factory) => factory.Create(type)` or select
   your actual localizer explicitly. The delegate receives the declaring model
   type for the tested property. The real generated consumer invokes the factory
   and returns the configured translated error. Our handwritten TextProperty
   deliberately uses a fixed message; it does not inherit automatic localization
   or DataAnnotations rules merely by implementing the interface. See
   `validation.format-localized-rules` and `validation.validate-asynchronously`
   for canonical rule formatting/async semantics.

## Constraints and alternatives

Use generated metadata for ordinary marked models where possible. Keep custom
metadata for a boundary with a specific authored policy. The sample checks only
whitespace, a fixed nested report graph and a required service. It is neither
binary-compatible with the old experimental interfaces nor a complete replacement
for the framework's validation implementations. Parameter execution is direct
reflection metadata in a non-web host; endpoint accessibility/adapter behaviour
remains outside this recipe. No trimming/AOT or web integration is claimed.

## Evidence

`eng/Test-ExtensionMetadata.ps1 -Record -VerifySources` builds/runs both original
ReefMaintenance consumers on Windows x64 with SDK10.0.402/runtime10.0.13/C#14
and SDK11.0.100-rc.2.26504.105/runtime11RC2/C#15. Locked dependencies, exact
commands/input hashes, compiler/CoreLib, audit-matching Validation archive,
assembly, shipped generator and selected compile/runtime assets are retained.
Both projects are non-web console hosts; no persistent environment, production
resource, device or Human action is required.

Old async type/property/parameter and current sync/async type/property/parameter
operations execute valid/invalid/nested reports. Current assertions check exact
Name/Path/Container/ErrorMessage, path restoration, required-service failure,
pending completion, original failure propagation and cancellation-token identity.
Actual generated metadata executes beside the custom resolver; LocalizerProvider
selects the model localizer and produces Code requis. Isolated compiler probes
reject the three old base types against RC2, the current package on net10 and
ASP0033/ASP0035/ASP0038 cases. The canonical current build is the fixed control.

The ordinary packed tool is installed through an isolated local-only feed/cache
and inspected through real stdio list/get from an unrelated directory. Discovery
ID/title/goal, complete canonical snippets/Markdown, target11/edition11,
applicability and fresh paired evidence are checked. Target10 has no applicable
edition; its comparison adds no foundation. Unusable retrieval proxies prove
proxy independence, not OS network isolation. Existing Human gates, PIN-959,
PIN-962 and the exact eight .NET10 foundations remain unchanged/open. RC2 evidence
does not establish final-GA, other-platform, native or endpoint behaviour.
