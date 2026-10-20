# Retrieve typed JSON metadata and distinguish resolution misses

## Goal

Obtain the generated contract for a cargo manifest without casting, and handle an intentionally unregistered contract without treating all serializer failures as harmless misses.

## Older approach

In net10.0 / C# 14 import `System.Text.Json`, `System.Text.Json.Serialization` and `System.Text.Json.Serialization.Metadata`; use namespace `Playbook.Samples` with standard implicit imports. GetTypeInfo(Type) requires a cast. For a resolver-only optional lookup, query its nullable result directly; this is a limited equivalent, not the same cached/configured options API as the newer helper.

{{snippet:older}}

Configure `JsonSerializerOptions options = new() { TypeInfoResolver = ManifestContext.Default }; options.MakeReadOnly();`. The generated context deliberately knows Manifest and not Uri. A resolver can throw; do not broadly catch InvalidOperationException or every exception as a missing contract.

## Newer approach

In net11.0 / C# 15 with the same imports, the options helpers return JsonTypeInfo<T> directly. Required throws NotSupportedException for a missing contract; TryResolve returns false with null only for a resolution miss. Resolver or contract-configuration errors can still propagate.

{{snippet:newer}}

With the options above, `ManifestMetadata.Required(options)` returns Manifest metadata. `ManifestMetadata.TryResolve<Uri>(options, out _)` returns false. Keep the generated-only resolver if a missing registration must be visible; adding a reflection fallback changes that policy.

## Migration

1. Configure the resolver/context deliberately and finish option setup before sharing read-only options.
2. Use GetTypeInfo<T> where a contract is required; a missing registration is then an application configuration fault.
3. Use TryGetTypeInfo<T> for optional support and inspect its Boolean result before using the metadata.
4. Let resolver/configuration exceptions retain their meaning; catch only failures the caller can actually recover from.
5. Test both a registered and an unregistered type with the production resolver composition.

## Constraints

The new helper requires framework System.Text.Json on .NET 11, SDK 11 / C# 15. No package asset, workload, provider, rendering mode or experimental opt-in. No older-target package route is claimed. Generated metadata and typed retrieval can aid AOT-oriented designs, but these ordinary managed builds do not prove trimming or Native AOT publication. The older direct resolver lookup bypasses options caching/configuration and is explicitly not a drop-in behavioral replacement.

## Evidence

Actual .NET 10 and .NET 11 RC2 executions cover required typed success, optional success, a generated-only Uri miss and a resolver deliberately throwing InvalidOperationException. The .NET 11 generic required lookup is also checked for NotSupportedException. These are real serializer/resolver calls; the intentionally failing resolver tests exception propagation rather than pretending to supply real metadata. Offline installed list/get verifies the entire body and applicability. Exact builds/input records are saved; GA and AOT remain unverified, no Human proof is required.
