# Generate structured logging methods for generic values

## Goal

A dispatch library records typed berth codes with stable event fields. Keep the reusable method generic and avoid evaluating expensive descriptions when debug logging is disabled.

## Older approach

.NET 10 source-generated logging supports concrete parameter types. A method specialized for BerthCode is clear and efficient for one domain; a reusable library can maintain concrete overloads when generic generation is unavailable. Neither this approach nor LoggerMessage.Define is inherently invalid.

{{snippet:older}}

## Newer approach

The newer LoggerMessage generator supports generic methods and ordinary constraints. Here TBerth remains a struct/Enum through the generated state. The separate Describe wrapper guards the expensive delegate call before passing its result to another generated method.

{{snippet:newer}}

## Migration

1. Use a net11.0 C# 15.0 project with nullable/implicit usings enabled and import Microsoft.Extensions.Logging. Add a private Microsoft.Extensions.Logging.Abstractions reference pinned to 11.0.0-rc.2.26504.105 to supply the verified generator assets. Runtime logging abstractions are in Microsoft.NETCore.App; this package's net11 lib asset is empty. The combined tracing sample's ASP.NET shared framework is not a logging requirement.
2. Place partial LoggerMessage methods in a partial type, assign stable event IDs and meaningful named template fields, then replace concrete overloads with the supported generic signature where reuse warrants it. Call Assigned with an ILogger, a berth enum value and the count. Existing concrete overloads remain a valid choice.
3. Use ordinary constraints such as struct/Enum as shown. Do not use allows ref struct: the actual generator reports SYSLIB1011. A constrained generic parameter does not mean every possible type or formatting contract is supported.
4. Guard expensive argument construction at the call site with IsEnabled for the same level. A generated internal guard cannot prevent evaluation of an argument expression before the method is entered. Pass cheap existing values directly. Avoid sensitive payloads and unbounded identifiers in log fields.
5. Enable CA1873 deliberately when useful (dotnet_diagnostic.CA1873.severity = warning in .editorconfig); it can be only a suggestion by default. The pinned SDK flags the unguarded LogDebug delegate invocation in the negative build. Apply the explicit guard or pass existing data; source generation alone does not make an already evaluated expensive argument free.

## Constraints

The generator preserves typed generic state, avoiding a forced object parameter at the method boundary. That is not an end-to-end zero-boxing or zero-allocation promise: a provider that enumerates object-valued structured fields can box value types. No allocation benchmark or exporter performance result is claimed.

CA1873 is analyzer guidance, not a new runtime API or a standalone recipe; its severity, detection and messages depend on the SDK. SYSLIB1011 is a generator restriction. The ordinary sample suppresses NU1510 for its intentional private analyzer reference, without suppressing either diagnostic. Do not use newly public Microsoft.Extensions.Logging.Internal compatibility shims: the inspected RC2 types are obsolete with error=true and EditorBrowsable Never. Use supported ILogger and LoggerMessage contracts.

The actual package/analyzer pin is separate from runtime/SDK families. No newer-generator/.NET 10 target route is advertised without its own consumer proof. No unrelated independently versioned logging product or provider setup is added.

## Evidence

Both actual projects compile their generated implementations and execute event 71 through an in-process recording ILogger sink, asserting message, event ID, typed Berth/Count fields, disabled no-record behavior and zero expensive delegate calls while disabled. This sink is application-owned test instrumentation, not a production exporter or remote logging service. RC2 generated source retains TBerth in its state; that source inspection is distinct from runtime counts. The bounded authoring script builds deliberately invalid allows-ref-struct and expensive-argument branches, observes SYSLIB1011/CA1873, then restores a successful ordinary build. Exact packages, SDKs, output and input hashes are retained; GA reconciliation remains outstanding.
