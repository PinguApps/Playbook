# Keep runtime-owned properties out of configuration binding

## Goal

Populate harbour capacity from configuration while leaving a runtime-issued session credential untouched, even when the input contains a matching key.

## Older approach

.NET 10 can use an explicit allowlist or bind a separate settings-only model. Both remain useful when the configuration boundary should be obvious in application code. The original comparison reads only BerthLimit; it never reads the credential key.

{{snippet:older}}

## Newer approach

.NET 11's ConfigurationIgnore attribute marks a property the binder must skip. The original model keeps its initializer while its ordinary property is bound. This checks reflection binding, not a source-generated binder or trimming deployment.

{{snippet:newer}}

## Migration

1. Create a net11.0 console project with Nullable and ImplicitUsings enabled, LangVersion 15.0, and a FrameworkReference to Microsoft.AspNetCore.App. This supplies the full configuration builder/binder used here; Microsoft.NETCore.App's abstractions alone are insufficient.
2. Add using Microsoft.Extensions.Configuration and put the declarations in namespace Playbook.Samples. Apply ConfigurationIgnore to each runtime-owned property. For this sample, call ConfigurationRootChecks.Run from Program.cs.
3. Bind a pre-created model if its runtime-owned values have already been assigned. Verify both the ordinary setting and the ignored value with hostile matching input.
4. Retain an allowlist or separate input model where that boundary better expresses ownership. Review every other consumer of the property; the attribute governs configuration binding only.

## Constraints

Ignoring a property does not remove the key from providers, redact logs, protect JSON serialization or establish a secret store. The in-memory provider makes the binding decision repeatable without files, watcher timing or external access. No generated binding/AOT, provider file-failure behavior or older-target package route is claimed. The full Microsoft.AspNetCore.App 11 shared framework is an explicit dependency of this executable route; other package compositions require their own verification.

## Evidence

Actual .NET 10 and .NET 11 RC2 projects compile and run the same ordinary-value/excluded-value assertions. Canonical source is embedded into this body. These are managed Windows x64 executions with no mocked binder. Exact SDK, shared-framework and input-hash records accompany the recipe; RC2 evidence does not establish GA behavior. No Human-only check is required for this bounded claim.
