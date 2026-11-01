# Supply a provider without application services

## Goal

Hollowmere Museum's preview rule engine accepts an IServiceProvider. With no
optional exhibit rules registered, previews use the engine's explicit permissive
default. Supply a provider with no application registrations while preserving
the DI interfaces callers can legitimately query.

## Older approach

.NET 10 can build an empty ServiceCollection. This remains a valid container with
normal lifetime ownership. The original example disposes the container after use.

{{snippet:older}}

Copy the complete types into a nullable, implicit-usings net10.0/C#14 console
project with `FrameworkReference Include="Microsoft.AspNetCore.App"`. Call
`MuseumRules.Preview("lunar-clock")` in the same assembly. Missing optional rules
return true under this fictional preview policy; an application requiring a rule
should instead fail explicitly with GetRequiredService.

## Newer approach

.NET 11's EmptyServiceProvider.Instance supplies a shared provider without creating
an application container. Declare the variable/parameter as IServiceProvider when
calling its explicit interface implementations.

{{snippet:newer}}

Use a net11.0/C#15 console project. DependencyInjection.Abstractions is in the
.NET11 base shared framework, so this consumer needs no ASP.NET framework
reference, NuGet package, workload, provider or experimental switch. The older
full-container comparison uses ASP.NET's shared implementation. No package
backport or older-target route for EmptyServiceProvider is claimed.

## Migration

1. Confirm no application registrations are needed at this boundary. Pass the
   singleton instead of constructing/discarding an empty container. Preserve the
   consumer's explicit fallback or failure policy; this change does not invent one.
2. Missing ordinary or named keyed application services return null.
   GetRequiredService and IKeyedServiceProvider.GetRequiredKeyedService throw
   InvalidOperationException for missing services, including a null service key.
   Supply a real populated container if required dependencies should exist.
3. Preserve built-in services. IServiceProvider, IServiceScopeFactory,
   IServiceProviderIsService and IServiceProviderIsKeyedService resolve, and their
   unkeyed queries report true. The provider resolves itself for IServiceProvider
   and both query interfaces. A null service key follows unkeyed resolution;
   named keys do not expose those built-ins. Implementing IKeyedServiceProvider
   does not mean it is itself a registered service.
   The executed .NET10 container reports named-key built-in queries as true even
   though named-key resolution returns null; the .NET11 empty provider reports
   those queries false. Do not assume complete query parity during replacement.
   The older container resolves its root scope for IServiceProvider; the new
   singleton resolves itself. Preserve service behaviour rather than comparing
   old container object identity with the resolved provider.
4. Closed IEnumerable<T> requests return empty sequences, for reference types and
   (in the executed managed JIT environment) value types. This also holds with
   named keys or AnyKey. Queries report closed enumerables available. Open generic
   IEnumerable<> is not an available service and resolves to null. Do not request
   open generic service definitions as though they were closed collections.
5. Use CreateScope or CreateAsyncScope normally. Empty scopes share the singleton
   provider; disposal performs no application cleanup and leaves it usable. Do
   not require fresh scope/provider identity or use this as an ownership mechanism
   for resources. The .NET10 comparison retains its real container disposal.
6. Keep KeyedService.AnyKey for enumeration. Trying to resolve an individual
   application service or built-in with AnyKey throws InvalidOperationException.
   Querying availability is distinct from resolving a service and does not remove
   that restriction. Test the actual interfaces your consumer invokes.

## Constraints and alternatives

This singleton has no application registrations; it does not literally resolve
nothing. The pinned Preview7 description's broad null/false claim is contradicted
by the actual RC2 implementation and the executed interface checks. A custom
provider that always returns null has a different built-in/enumerable/scope
contract. Keep such a deliberately narrower contract only when callers expect it.

Native AOT value-type enumeration remains unsupported here. The pinned source
throws for a value-type enumerable when dynamic code is unavailable. Successful
JIT IEnumerable<int> checks are not an AOT build/run, and availability queries do
not prove resolution succeeds in AOT. No trimming/AOT deployment is claimed.
Keep a populated container for real dependencies, startup options validation,
scope-owned resources or registered rules; this goal is separate from startup DI.

## Evidence

`eng/Test-ExtensionContracts.ps1 -Record -VerifySources` builds/runs MuseumServices
with SDK10.0.402/runtime10.0.13 and SDK11.0.100-rc.2.26504.105/runtime11RC2.
Application requirements are release-family SDK/runtime11; exact pins describe
observed evidence. Both use empty locked package closures, no native toolchains,
external providers, UI/rendering modes or experimental opt-ins. The audited
contract is not marked experimental.

Actual IServiceProvider/IKeyedServiceProvider/query-interface calls verify the
optional preview, absent and required-service failures, built-ins/self resolution,
null/named/AnyKey boundaries, reference/value closed enumerables, open generics,
scopes, synchronous/asynchronous disposal and subsequent provider use. Records
retain commands, inputs/dependencies, compiler/CoreLib, loaded Extensions assets
and audit-matching references. Source review supports the AOT limitation but
does not replace its explicitly unavailable execution evidence.

The ordinary packed tool is installed using an isolated local-only feed/cache
and inspected from an unrelated directory over actual stdio list/get. Discovery,
complete canonical snippets/Markdown, target11/edition11 applicability and current
paired evidence are checked. Target10 returns no_applicable_edition; its working
comparison adds no foundation. Unusable retrieval proxies establish proxy
independence, not network isolation. Windows x64 managed RC2 proof does not
establish other-runtime/platform or final-GA behaviour. PIN-959/PIN-962, pending
Human gates and the exact eight .NET10 foundations remain unchanged.
