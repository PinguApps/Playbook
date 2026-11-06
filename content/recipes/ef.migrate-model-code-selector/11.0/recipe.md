# Select generated model code using explicit options

## Goal

Keep a custom model-code selector working while scaffolding an original marsh observation schema into usable C#.

## Older approach

EF10 exposes the string-language selector overload. The original decorator retains that obsolete overload under a narrow CS0618 suppression and forwards language selection. The options overload already exists in EF10; projects can adopt it before upgrading.

## Newer approach

EF11 removes the string overload. Implement Select(ModelCodeGenerationOptions), forwarding the complete options object to the underlying selector. The Bittern decorator is exercised by the real reverse-engineering scaffolder, not just a direct interface call.

{{snippet:n11-program-cs}}

## Migration

1. Pin compatible EF, Design and SQLite dependencies together. These consumers use EF10.0.13 or EF11 RC2 with SDK10.0.403 or SDK11 RC3 respectively. EF11 requires net11.0; the audit's historical SDK10.0.402/SDK11 RC2 pins remain separate. Preserve the patched Pkcs10.0.13 dependency and locked transitive code-generation graph.
2. Make Design compile assets available with IncludeAssets=all and retain PrivateAssets=all. This is an actual Design API consumer, not just an application referencing tooling indirectly.
3. Change a custom selector implementation from Select(string) to Select(ModelCodeGenerationOptions). Forward the options without reconstructing only Language: the contract allows selection to account for other generation choices. The older decorator demonstrates language-only forwarding; the newer decorator preserves the complete object.
4. Register EF design-time services and SQLite's provider design services. The provider registration type is internal; the sample confines EF1001 suppression to that one registration. Pin and retest it when packages change. The selector interface itself is public.
5. Create the isolated SQLite Observations schema with Reedbank/12.5 and Mudflat/null rows. Supply explicit language, context/model namespaces, context name, nullable-reference mode and suppressed OnConfiguring. Keep connection details out of generated source.
6. Register the decorator over the original selector and invoke IReverseEngineerScaffolder. Assert it was actually called. Save generated entity/context code into a unique temporary directory rather than overwriting authored files.
7. Compile that actual generated output as a separate application with the same SDK/provider graph. Query both rows through the generated context, checking names, numeric values and nullability. Run eng/Test-BitternScaffolding.ps1 -Record to capture compilation and the separate generated-consumer execution receipt.

## Constraints

Windows x64 file SQLite only. Exact native/transitive assets remain in the locks; no paid/encryption alternative is selected. This is reverse-engineered model code, not compiled-model optimization, query precompilation or generation-stage guidance. No other language/provider, arbitrary selector policy, GA or UI rendering claim follows. No language-preview opt-in is needed. Internal provider registration has no stable compatibility guarantee. Restore needs package access; delivered Markdown/snippets are offline. No additional .NET10 foundation, recurring native matrix or existing edition change is introduced.

## References

- [Design-time services](https://learn.microsoft.com/en-us/ef/core/cli/services)
- [IModelCodeGeneratorSelector](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.scaffolding.imodelcodegeneratorselector)
## samples/net10.0/BitternScaffolding/BitternScaffolding.csproj

{{snippet:n10-bitternscaffolding-csproj}}

## samples/net10.0/BitternScaffolding/Consumer.cs.txt

{{snippet:n10-consumer-cs-txt}}

## samples/net10.0/BitternScaffolding/Directory.Build.props

{{snippet:n10-directory-build-props}}

## samples/net10.0/BitternScaffolding/Directory.Build.targets

{{snippet:n10-directory-build-targets}}

## samples/net10.0/BitternScaffolding/Directory.Packages.props

{{snippet:n10-directory-packages-props}}

## samples/net10.0/BitternScaffolding/global.json

{{snippet:n10-global-json}}

## samples/net10.0/BitternScaffolding/Program.cs

{{snippet:n10-program-cs}}

## samples/net11.0/BitternScaffolding/BitternScaffolding.csproj

{{snippet:n11-bitternscaffolding-csproj}}

## samples/net11.0/BitternScaffolding/Consumer.cs.txt

{{snippet:n11-consumer-cs-txt}}

## samples/net11.0/BitternScaffolding/Directory.Build.props

{{snippet:n11-directory-build-props}}

## samples/net11.0/BitternScaffolding/Directory.Build.targets

{{snippet:n11-directory-build-targets}}

## samples/net11.0/BitternScaffolding/Directory.Packages.props

{{snippet:n11-directory-packages-props}}

## samples/net11.0/BitternScaffolding/global.json

{{snippet:n11-global-json}}

## samples/net11.0/BitternScaffolding/Program.cs

{{snippet:n11-program-cs}}

## Evidence

Independent locked builds and real focused execution were recorded on Windows x64 with SDK10.0.403 and SDK11 RC3. Compilation, service execution and installed offline retrieval establish separate claims. Review the embedded evidence entries for exact inputs, dependencies and command outputs; freshness describes package-build inputs, not the consumer machine or GA. SQLite generated consumers measure engines 3.53.3/3.53.4. Other providers remain unverified.
