# Give a dynamic event provider an explicit identity

## Goal

Boreal Botanical Garden emits BloomCount observations from named glasshouses.
Choose the provider identity independently of a runtime-selected garden name,
and confirm that the intended managed listener receives the event and payload.

## Older approach

.NET10's public dynamic EventSource constructor derives a GUID from its name:

{{snippet:older}}

Place this method in a class with `using System.Diagnostics.Tracing;`, call
`using EventSource source = Open("glasshouse");`, and emit
`source.Write("BloomCount", new EventSourceOptions { Level = EventLevel.Informational },
new { Bed = "fern-house", Count = 7 });`. Use a nullable, implicit-usings
net10.0/C#14 console project. No package or framework reference is needed.

This remains useful when the name defines identity: reopening after disposal
reproduces its GUID; changing the name changes it. A fixed derived class with
EventSourceAttribute(Name=..., Guid=...) is another valid older design when
the provider's identity is fixed in authored code. It does not accept arbitrary
runtime names and GUIDs through these new public constructors. No package
backport or net10 route for them is claimed.

## Newer approach

.NET11 accepts the explicit GUID alongside the dynamic provider name:

{{snippet:newer}}

Use `using EventSource source = Open("glasshouse",
new Guid("1755c71d-5b43-4e24-81fa-631b15d372da"));` in a net11.0/C#15 project.
Inspect `source.ConstructionException` before emitting the same BloomCount event;
handle initialization failure according to the application's logging policy.
Keep the identity stable in configuration rather than generating a fresh GUID
for every observation. Reuse one provider for the intended lifetime and dispose
it when its owner ends. A GUID is a diagnostic identity, not authentication.

## Migration

1. Decide whether a name-derived GUID already satisfies the integration. Keep that
   simpler route if it does. Use the explicit GUID constructor when the provider's
   runtime name and identity must be chosen independently. Migrate collector
   configuration deliberately; changing identity can change what subscribers select.
2. Choose either actual new overload: `new EventSource(name, guid)` or
   `new EventSource(name, guid, settings, traits)`. The simple overload defaults
   to **EtwManifestEventFormat** in the measured RC2 implementation. The canonical
   longer overload explicitly requests EtwSelfDescribingEventFormat for dynamic
   writes and passes alternating trait keys/values. These settings affect native
   event formatting; receiving a managed event does not certify an ETW format.
3. Check initialization separately from write failures. With both Default and
   ThrowOnEventWriteErrors, an empty GUID or odd trait list returns an object with
   `ConstructionException is ArgumentException` on this runtime; the constructor
   does not throw for those inputs. Do not infer success merely because `new`
   returned. Conversely, selecting both manifest and self-describing format flags
   throws ArgumentException during settings validation. Do not generalize one
   input's failure mode to all constructor arguments.
4. Enable the intended provider on an EventListener and check Name, Guid,
   GetTrait("garden"), Settings and actual EventWrittenEventArgs. The checked
   BloomCount event has Bed=`fern-house` and integer Count=7. Subscribe at
   Informational or Verbose; a logging call without an enabled listener does not
   prove delivery. Event names and payload fields remain separate from provider
   identity. Avoid relying on managed object identity after payload conversion.
5. Treat ThrowOnEventWriteErrors as a write-error policy. A real listener that
   throws while processing BloomCount is swallowed by the default source and
   becomes EventSourceException with the flag enabled. That observed distinction
   does not make initialization failures throw and does not guarantee collection
   merely because Write returned. Decide whether diagnostic failures should affect
   the application before enabling the flag.
6. Prevent overlapping live provider identities. Name-derived providers with the
   same name normally share a GUID. In the measured explicit-identity control,
   a second live dynamic provider with the same name/GUID is not eagerly rejected
   but only one of the two managed writes arrives. This does not establish supported duplicate
   identities for native collectors: manifest ambiguity and conflicting event
   metadata remain documented restrictions. Share one instance or dispose it
   before recreating it; do not use duplicate-name switches as a routine fix.
   Keep names unambiguous for collector configuration even with explicit GUIDs.
7. Disable subscriptions and dispose both listener and provider. The checks observe
   no further BloomCount deliveries after DisableEvents or source disposal;
   repeated source disposal is safe. Successful managed lifecycle checks do not
   establish a native trace session's deployment or flush semantics.

## Constraints and alternatives

The application route needs .NET11 SDK/runtime framework contracts. Exact
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105 and C#15 identify measured
evidence; the older real consumer uses SDK10.0.402/runtime10.0.13/C#14. These are
ordinary public APIs, not marked experimental in the audited artifacts; no preview
syntax, package, workload, external provider, account, rendering mode or opt-in
is needed. Trait pairs are application metadata; GetTrait alone does not prove
that a native collector interprets them.

The scope is a non-contract provider using Write<T>. A strongly typed attributed
provider remains appropriate for a fixed event schema. This recipe does not
validate native ETW/EventPipe collection, permissions, registration, performance,
Native AOT or cross-platform deployment. Such claims need separate real tooling
evidence. No native trace session is started here.

## Evidence

`eng/Test-ExtensionIdentity.ps1 -Record -VerifySources` compiles and executes
BorealSignals against both actual dependency sets. A managed EventListener
checks names/GUIDs, exact payload fields/values, selected settings/traits,
disable/dispose, old name-derived identity, both new constructors, duplicate live
identity and observed invalid-input/write behaviour. Original source hashes,
audit-matching reference assemblies, compiler and loaded CoreLib identity are
recorded through the existing sample-evidence workflow. Source review supplies
contract context; actual managed execution supplies the behaviour observations.

The ordinary installed package's real stdio list/get returns reusable discovery,
complete canonical Markdown, explicit target11/edition11, applicability and current
paired evidence from an unrelated directory. Target10 returns no_applicable_edition;
the comparison is not a new foundation. Retrieval with unusable proxies establishes
proxy independence, not network isolation. Windows x64 RC2 proof is not native
collector, other-platform or final-GA proof. PIN-959/PIN-962, existing Human gates
and the exact eight approved .NET10 foundations remain unchanged.
