# Reuse a base-item rendering fragment for derived items

## Goal

Aster Observatory renders the common name of an observation in a slot accepting a `SpecialObservation`. Reuse the base renderer because every special observation is also an observation. Keep the template within a compatible render context; generic variance is a delegate conversion rule.

## Older approach

.NET 10 declares `RenderFragment<TValue>` without variance. The original typed adapter accepts `SpecialObservation?` and invokes the `Observation?` renderer. It preserves the same output and is still valid on newer releases. Direct delegate assignment fails on the actual older consumer; upgrading only the compiler does not change the referenced delegate declaration.

### .NET 10: `AsterObservatory.csproj`

{{snippet:n10-asterobservatory-csproj}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Observation.cs`

{{snippet:n10-observation-cs}}

### .NET 10: `ObservationView.razor`

{{snippet:n10-observationview-razor}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

## Newer approach

.NET 11 declares `RenderFragment<in TValue>`. The `in` means a renderer that accepts a base item can also fill a slot that will pass only derived items. Assign `BaseTemplate` directly. It still receives the actual derived instance, renders its inherited name with HTML encoding, and deliberately accepts null. The delegate conversion neither checks null nor creates an alternative renderer for derived-only fields.

### .NET 11: `AsterObservatory.csproj`

{{snippet:n11-asterobservatory-csproj}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Observation.cs`

{{snippet:n11-observation-cs}}

### .NET 11: `ObservationView.razor`

{{snippet:n11-observationview-razor}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

## Migration

1. Verify the consumer targets `net11.0` with matching ASP.NET Core 11 runtime/reference assets. Keep the adapter for .NET 10; no external NuGet package or language-only switch adds this framework declaration.
2. Copy the complete current files into one directory. The Razor project is an executable harness with a `Microsoft.AspNetCore.App` framework reference. It requires a matching SDK/runtime, no provider, native assets or optional workload.
3. Keep the base template accepting every value that the derived slot can pass. Replace only the forwarding adapter with direct assignment. Retain the explicit nullable input and placeholder policy, or choose and test a deliberate rejecting policy in your own component.
4. Keep generic templates inside one compatible rendering context. Do not pass them across a static/interactive render-mode serialization boundary. The separate `blazor.project-static-child-content` recipe only captures supported non-generic static content.
5. Build with `dotnet build -c Release`, then execute the generated DLL with the matching runtime. Program checks derived content, encoding and null. `eng/Test-ArchiveFragments.ps1` additionally compiles rejected reverse assignments on both releases and rejected direct assignment on .NET 10.

## Constraints

Never convert a `RenderFragment<SpecialObservation?>` to `RenderFragment<Observation?>`: a caller could supply an ordinary observation without the special fields. Both actual compilers reject this unsafe direction. Do not work around that rejection with a cast. This reference-type example makes no value-type variance or arbitrary delegate serialization claim.

Null handling remains application policy. Here a missing item renders `No observation selected`; `AddContent` encodes item text. The original adapter performs no filtering, copying or type conversion, so removing it preserves the intended behavior. The real in-process HtmlRenderer proves this template's output; it does not demonstrate a browser connection or cross-render-mode execution.

## Evidence

Actual .NET 10/C#14 (SDK 10.0.402/framework 10.0.13) and .NET 11/C#15 (SDK 11.0.100-rc.2.26504.105/framework 11.0.0-rc.2.26504.105) consumers compile and render `SpecialObservation("Comet & dust", 30)` as encoded text, then render the null placeholder. They have empty external dependency closures and lock files. `eng/Record-Evidence.ps1` saves fresh input hashes, exact commands and successful real renderer output in the existing sample evidence records.

The focused compiler checks reject the unsafe reverse conversion on both dependency sets and the new direct conversion on .NET 10. The installed-MCP check rebuilds and executes consumers using only the returned Markdown outside the repository. These are execution and compiler results, not mocks. Exact pins are verification baselines, separate from the release-family consumer requirements. No experimental opt-in applies. Actual-GA reconciliation (PIN-964) and comprehensive coverage (PIN-959) remain outstanding.
