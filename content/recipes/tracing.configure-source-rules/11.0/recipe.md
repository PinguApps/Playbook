# Select activity sources and operations with refreshable tracing rules

## Goal

A reef logistics service records dispatch operations while excluding heartbeat noise. Select sources, operations, named listeners and factory-local scope, and let configuration changes affect an already-created source.

## Older approach

.NET 10 supports ActivityListener directly. The application owns source matching, sampling policy, listener registration and disposal. This remains a useful small explicit listener, but changing its predicate alone does not provide the newer refresh contract.

{{snippet:older}}

## Newer approach

Register a named listener and tracing rules. The listener supplies sampling and callbacks; the rules select which sources and operations reach it. An enable rule does not create an exporter. This example deliberately selects local sources and excludes the Heartbeat operation for the dispatch listener.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project with C# 15.0, nullable/implicit usings enabled and FrameworkReference Microsoft.AspNetCore.App. Import System.Diagnostics, Microsoft.Extensions.DependencyInjection and Microsoft.Extensions.Diagnostics.Tracing. This verified full-framework route requires no external Diagnostics package. The combined sample's private logging-generator dependency is unrelated to tracing requirements.
2. Create a ServiceCollection and call TraceSetup.Register with a stopped-activity callback. Build and own a ServiceProvider. Resolve ActivitySourceFactory from that provider; the infrastructure initializes registered listeners when the factory is resolved (supported host startup also integrates it).
3. Create the source through factory.Create("Reef.Dispatch"), retain it for the component lifetime and dispose it before the owning provider. StartActivity("Ship") produces a sampled activity; StartActivity("Heartbeat") returns null for this policy. Use a nullable using declaration because no matching/sampling listener can mean no activity.
4. For configuration-driven rules, call AddConfiguration on the tracing builder with an IConfiguration section. EnabledLocalTracing:Reef.Reload=false disables the example local source; changing that value to true and calling IConfigurationRoot.Reload enables it. Setting false and reloading detaches it again. Register a named listener with sampling and stopped callbacks as above. Import Microsoft.Extensions.Configuration for the builder/reload APIs.
5. If retaining a manually registered ActivityListener instead, update its ShouldListenTo state and call RefreshSources on .NET 11 to revisit existing sources. Merely changing a captured boolean is insufficient. Dispose every listener/provider when its owner ends.

## Constraints

A source created by new ActivitySource is global; a factory-created source belongs to that factory's scope. Local rules do not subscribe a different provider's local sources. The sample proves exclusion of a global source with the same name. Listener name and operation matching are exact; source rules support prefixes or one wildcard. Prefer narrowly scoped rules and check specificity when combining configuration and code. More specific listener/source/operation/scope rules win; the last equally specific rule wins in the pinned contract.

Rules filter; sampling still decides whether an activity is created/recorded. ActivitySourceFactory lifecycle is DI-owned; do not dispose a resolved singleton factory in a transient consumer. Reuse sources rather than constructing one per request. Callback work must be safe for the threads that produce activities. No OpenTelemetry exporter, remote collector, trace persistence, browser integration or allocation benchmark is established here. The new APIs are not designated experimental by the inspected artifacts; RC2 proof still needs GA reconciliation.

## Evidence

The actual .NET 10 listener and .NET 11 factory/rules produce only the intended stopped Ship record, exclude Heartbeat and another source, and mark the sampled activity Recorded. RC2 excludes a global same-name source, applies false/true/false configuration changes to an existing factory source, and executes manual RefreshSources attach/detach. These are real in-process tracing callbacks, separate from source review of matching rules and from unclaimed exporter behavior. Exact release projects and hashed evidence accompany the recipe.
