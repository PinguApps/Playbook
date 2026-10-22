# Validate settings with a dependency-injected validator

## Goal

Reject a sailing crew larger than the vessel's configured capacity before the application starts accepting work, keeping the reusable rule in its own injected class.

## Older approach

.NET 10 already supports IValidateOptions and dependency injection. Registering the validator under IValidateOptions<SailingOptions> makes it part of the options pipeline. A validation delegate accepting dependencies is another valid choice for a small rule.

{{snippet:older}}

## Newer approach

.NET 11's parameterless generic Validate<CrewValidator>() overload registers the validator type through the options builder. Its constructor receives VesselRules through DI. It does not require registering CrewValidator separately in this example.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project with C# 15.0, Nullable/ImplicitUsings enabled and a FrameworkReference to Microsoft.AspNetCore.App. Import Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Hosting and Microsoft.Extensions.Options. Put the declarations in namespace Playbook.Samples.
2. Keep the dependency and validator class. Replace the explicit IValidateOptions registration with Validate<CrewValidator>() on the matching options builder, and remove a duplicate registration that would execute the rule twice.
3. Inside StartupExamples, create HostApplicationBuilder and call RegisterCrew on builder.Services with the configured crew. Build the host and await StartAsync before accepting work. ValidateOnStart moves failure to that supported startup path; merely constructing the host does not validate.
4. Check an allowed crew and an excessive crew. Keep meaningful validation in the validator and handle OptionsValidationException at the application's startup boundary if a custom failure policy is needed.

## Constraints

The example is a synchronous in-process policy, with no database, network or external registry claim. Validate<TValidator>() does not turn synchronous rules into asynchronous rules. Use the async-startup recipe for checks that actually await work.

The validator applies to every name in this example. A validator owning one named option should return Skip for other names; configuration and validation must agree on that name. Use dependencies with lifetimes suitable for the options registration; do not capture a scoped service in a singleton options consumer. A custom options factory or custom startup validator may change the default pipeline. The full shared framework provides Hosting here; the base shared framework's abstractions are not a complete host.

## Evidence

Actual .NET 10/11 projects construct the Generic Host with real DI, await startup for crew 3, and require startup rejection for crew 9 against maximum 6. The canonical validator is the one those checks execute. No mocked container or generated-validator substitution is used. Exact SDK/shared-framework and input-hash records accompany the recipe. Windows x64 RC2 evidence is not GA or native/browser integration proof.
