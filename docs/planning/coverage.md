# Release-wide coverage planning

Pingu has locked in comprehensive .NET 11 coverage wherever a capability changes authored code or how a goal is implemented. The original 25 Blazor topics, seven additions, and three expansions remain required. This document is a researched coverage map, not a completed recipe inventory or a claim of tested examples.

## Eligibility

Include new author-facing APIs, language constructs, and concrete migrations that replace or change application code. Include authored markup, scripts, project/build configuration, and platform manifests where these change how a goal is implemented. A performance-oriented API can qualify when using it requires a different implementation.

Exclude invisible JIT/GC/library speedups and other benefits obtained solely by upgrading. A performance switch alone is not automatically a recipe: establish a meaningful authoring or migration decision rather than relabeling an automatic benefit. Required correctness, compatibility, security, and deprecation migrations can qualify even when they add no new capability.

No framework gets an invented recipe just to fill a category. Record an area as reviewed with no eligible changes if that is what its source/API audit establishes. Prioritization orders the work; it does not remove qualifying .NET 11 coverage.

The accepted product boundary is the .NET 11 release family and explicitly related first-party components. Dependencies required to demonstrate a qualifying .NET 11 recipe are documented. Standalone coverage of independently versioned libraries, tooling, and native OS APIs needs a separate scope decision.

## Coverage map

### Languages

Audit C# 15, F# 11, and any Visual Basic authoring changes in the release. Candidates include unions/closed hierarchies, collection construction, extension indexers, loop control, F# record authoring, and bounded asynchronous work. Language version, SDK version, runtime target, and preview requirements must be separate facts. [C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15), [F# RC2 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md).

### Shared libraries and runtime-facing APIs

Audit process execution, serialization/JSONL/unions, LINQ and asynchronous sequences, networking/DNS/HTTP content, compression/archives, memory-backed streams, validation, numerics, cryptography, interoperability, and relevant Extensions libraries. The release overview is a discovery index, not proof every listed optimization needs a recipe. [Library changes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries), [Extensions releases](https://github.com/dotnet/extensions/releases).

Strong example goals include capturing child-process output/cancellation, reconciling data with FullJoin, streaming compressed HTTP requests, and serializing versioned data contracts. These require independently authored examples and actual API verification.

### SDK, projects, builds, and tools

Audit file-based application authoring, project references/directives, test runner configuration, solution/build configuration, container publishing artifacts, launch configuration, and NuGet authoring changes. Changes to command completion, installer internals, or build speed alone do not qualify. [SDK changes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/sdk), [RC2 SDK additions](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/sdk.md).

### Web frameworks and hosting

Keep the entire accepted Blazor subset. Extend the audit to Minimal APIs, MVC, Razor Pages, validation, authentication/authorization, SignalR, OpenAPI, gRPC where applicable, caching, and hosting configuration. Examples of new authoring opportunities include validation and binding-failure handling, cancellation in hub calls, richer response contracts, and explicit policy configuration. [ASP.NET Core 11 changes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0).

The CSRF and navigation/CSP migrations meet Pingu's conditional acceptance where the original recipe changes authored startup, forms, markup, or JavaScript. Explain eligibility and retained protections rather than presenting automatic middleware effects as custom application work. [Migration guidance](https://learn.microsoft.com/en-us/aspnet/core/migration/100-to-110?view=aspnetcore-11.0).

### Data access

Audit EF Core 11 modelling, queries, supported providers, and migration tooling. Candidates include complex-type keys/indexes, new query translations, SQL Server JSON/full-text/vector authoring, Cosmos configuration, and migration-command configuration. Provider/database prerequisites are part of applicability. [EF Core 11](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew).

Review breaking changes separately for concrete mitigation recipes, including Cosmos persistence behavior and provider/configuration compatibility. Faster query generation without an authored change is excluded. [EF Core 11 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).

### Windows desktop

Audit WPF and WinForms separately, including designer/migration implications. WinForms candidates include explicit rendering/layout choices, disposable painting scopes, kiosk behavior, and designer-compatible code. WPF's current notes show fewer distinct authoring changes; investigate the clipboard exception-compatibility switch without inventing recipes for automatic fixes. [WinForms RC2](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/winforms.md), [WPF RC2](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/wpf.md).

### Mobile and cross-platform UI

Audit MAUI and its .NET platform workloads: UI/navigation/input, source-generated interop/XAML, permissions/media/activation, authored assets/manifests, and required platform migrations. Candidates include animation cancellation, AOT-safe HybridWebView calls, Apple scene lifecycle changes, and Android build/compatibility configuration. Native binding additions need the release-boundary decision above. [MAUI 11 overview](https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-11), [MAUI RC2 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/dotnetmaui.md).

## Completeness method

Create a coverage register from official release notes across previews/RC/GA, migration and breaking-change inventories, and relevant public API changes. Every candidate should have a source, introduced release, authoring impact, applicability, and disposition:

- Included as a named recipe.
- Covered by another recipe, with a link.
- Excluded, with the reason it does not meet the authoring criterion.
- Unresolved, with the missing fact or verification requirement.

Separate recipe completeness from runtime verification. Headline examples above do not replace that register, and the register does not prove samples work. Keep original runnable samples and package integration evidence tied to exact artifacts. Avoid claiming exhaustive API coverage until the register is reconciled; this session performed bounded source research, not a full API diff.

## Selected .NET 10 carry-forward

Pingu approved a small curated foundation set in Round 3, retaining comprehensive .NET 11 as the main coverage commitment. The specific initial recipes remain to agree. Do not assume the age of a release proves agents know it. No target-agent benchmark has been performed.

Prioritize capabilities that remove substantial custom code, underpin .NET 11 recipes, or show repeated errors in later agent evaluations. Candidate families:

- C# 14 extension members and field-backed properties; relevant language foundations for newer recipes. [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14).
- Minimal API validation, typed SSE, Identity passkeys, and generated OpenAPI customization. [ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0?view=aspnetcore-10.0).
- EF named query filters, outer joins, complex JSON mapping, and update-expression authoring. [EF Core 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew).
- Strict JSON contracts and asynchronous LINQ. [Libraries](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/libraries).
- File-based applications and Microsoft.Testing.Platform integration. [SDK](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/sdk).
- Select desktop/mobile migrations where useful: typed clipboard, stable asynchronous form APIs, and MAUI compatibility-control migrations. [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100), [WinForms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/whats-new/net100), [MAUI](https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10).

These are candidate families, not approved recipe counts or a commitment to all .NET 10 changes. Mark introduction release accurately; a .NET 10 recipe can remain applicable to .NET 11 unless superseded.

### Proposed initial eight recipes

This shortlist is researched and awaiting Pingu's selection. All examples must be authored independently from scratch.

1. **Extend a type you cannot edit with a computed property.** Replace helper-method call sites with C# 14 extension-member authoring. Check declaration restrictions and instance-member precedence. [Extension members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods).
2. **Validate or normalize property assignments without maintaining a backing field.** Use C# 14 field-backed accessor bodies in place of a separately declared field and full accessors. [Field keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/field).
3. **Reject invalid Minimal API input before the handler runs.** Replace repeated checks/custom filters with registered model validation; prove generated metadata covers the model assembly. Keep this foundation synchronous; asynchronous validation belongs in the .NET 11 edition. [Validation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/validation?view=aspnetcore-10.0).
4. **Include archived records while retaining a tenant filter.** Use named EF filters and selective disabling instead of one combined filter and all-or-nothing removal. Do not present query filtering as write protection or authorization. [Named query filters](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew#named-query-filters).
5. **Join two data sets while retaining unmatched rows.** Use LeftJoin/RightJoin instead of the manual outer-join pattern, with explicit missing-side handling and EF provider applicability. [EF join support](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew#support-for-the-net-10-leftjoin-and-rightjoin-operators).
6. **Reject malformed JSON contracts at the import boundary.** Adopt strict deserialization options rather than scattered structural checks; keep business validation separate. [Strict JSON options](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/libraries#strict-json-serialization-options).
7. **Filter and enrich an asynchronous stream without buffering it first.** Use the async LINQ operators in place of manual iterator plumbing, accounting for cancellation and older package migration. [AsyncEnumerable migration](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/10.0/asyncenumerable).
8. **Run a small C# repository utility without creating a throwaway project.** Use a file-based application with pinned dependency directives, accounting for inherited build/central-package configuration and SDK-specific directives. [File-based applications](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps).

No authentication UI, mobile workload, or device setup is needed for this proposed foundation set. It remains a proposal; no samples have been created or run.

## Source freshness and verification impact

The research baseline is .NET 11 RC2, not GA. Several official overview/download tables lag the language-specific or canonical RC notes. Resolve conflicts against the actual pinned SDK/packages before publishing recipes. [Release overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview).

In particular, F# compiler-test builders are not automatically shipped FSharp.Core APIs, and an attribute named in a compiler note is not necessarily in the BCL. Desktop overviews can omit RC2 changes. MAUI workload/package and native toolchain versions are independent of the .NET SDK version; use the actual workload release metadata. [MAUI RC2 workload release](https://github.com/dotnet/maui/releases/tag/11.0.100-rc.2.26513.2).

The documentation tool remains a portable, workload-free host. Pingu explicitly limited Windows/macOS CI to very brief smoke checks. Plan a packed-tool install/start/initialize/list/get/shutdown check per OS, without native UI, emulator, or device suites. Keep focused Linux validation and feasible sample compilation. Do not move a broad native matrix into another workflow to bypass that limit.

Checking native recipe behavior would require the corresponding platform/toolchain and, for some features, an emulator or device. The approved tool smoke does not establish that behavior. Label each example's actual evidence accurately: source-reviewed, compiled on a stated target, or executed in a stated environment. Cross-compilation is build evidence, not native behavior evidence. Any expanded native testing is a separate future decision. [Platform setup](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?tabs=visual-studio-code), [Windows cross-targeting](https://learn.microsoft.com/en-us/dotnet/core/tools/sdk-errors/netsdk1100).

No application samples, package installations, native toolchains, or runtime behavior were exercised in this research. The original-example requirement applies across every area.
