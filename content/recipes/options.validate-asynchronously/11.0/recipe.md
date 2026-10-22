# Await settings validation before application startup completes

## Goal

Check a departure permit through an asynchronous service and fail startup when it is rejected, without synchronously blocking on a task.

## Older approach

.NET 10 can run an application-owned gate in IHostedService.StartAsync. Register it before services that accept work, keep concurrent startup disabled, and ensure no serving path starts earlier. This remains a valid application lifecycle choice; it is not built-in asynchronous options validation.

{{snippet:older}}

## Newer approach

.NET 11's options builder accepts asynchronous validation delegates. ValidateOnStart integrates them with the Generic Host's awaited startup and seeds the built-in default IOptions and matching monitor caches after successful validation.

{{snippet:newer}}

Custom validators implement both contracts. A synchronous implementation must perform meaningful validation or Skip an unrelated name; it must not return Success merely to bypass a required asynchronous check. New custom startup gates register as IAsyncStartupValidator.

{{snippet:dual}}

For source-generated synchronous validation, choose generated or handwritten ownership. This canonical example normally lets the generator provide Validate. Defining EXPLICIT_VALIDATE deliberately enables the conflicting handwritten method and the actual RC2 generator rejects it with SYSLIB1205. Remove that branch for generated ownership, or remove OptionsValidator and own the full manual contract. Async generation is separate.

{{snippet:generated}}

## Migration

1. Create a net11.0 console project with C# 15.0, Nullable/ImplicitUsings enabled and FrameworkReference Microsoft.AspNetCore.App. Import Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Hosting and Microsoft.Extensions.Options; the generated model also uses System.ComponentModel.DataAnnotations.
2. Register the service and async delegate shown above. In StartupExamples, create HostApplicationBuilder, call RegisterDeparture on its Services, build the host and await StartAsync before any consumer reads these settings or accepts work. Pass a startup cancellation token where appropriate.
3. For the optional generated example, add a private Microsoft.Extensions.Options PackageReference pinned to 11.0.0-rc.2.26504.105 so its analyzer assets run. The sample suppresses NU1510 for that intentional analyzer reference: net11 runtime types already come from the shared framework. It does not disable analyzer diagnostics.
4. Migrate handwritten IAsyncValidateOptions implementations to include Validate as well as ValidateAsync, register them as IValidateOptions or through Validate<TValidator>(), and preserve name matching. Replace new uses of obsolete IStartupValidator (SYSLIB0066) with IAsyncStartupValidator.
5. Check startup acceptance/rejection and every synchronous access path your application uses. Do not add GetAwaiter().GetResult, synchronous network calls or a success-only validator as a workaround.

## Constraints

The built-in async-only delegate fails synchronous validation for its matching name. Adding another succeeding synchronous validator does not cancel that failure. Default IOptions access before startup, fresh IOptionsSnapshot scopes, IOptionsFactory.Create and monitor reload remain synchronous. Startup does not populate snapshot caches. Monitor reload can fail and withhold change notifications; it provides no asynchronous last-known-good guarantee. No public asynchronous monitor/snapshot getter is established here.

The built-in async startup route requires the exact built-in factory, and default-name validation requires the built-in IOptions implementation. Derived/replacement factories use synchronous startup; custom default IOptions can make startup throw. Existing monitor values win; a faulted monitor cache can fail startup. A custom legacy IStartupValidator takes precedence and suppresses async startup validators. These custom-composition limits are pinned source findings; they are not all executed by this sample.

PermitRegister is a real asynchronous in-process policy, not proof of an external account or network registry. A production remote check needs its own timeout, credentials and integration evidence. No such provisioning is required or claimed here.

## Evidence

Both actual release projects run supported Generic Host startup, acceptance and rejection. RC2 additionally executes IAsyncStartupValidator, named dual validation, seeded default/monitor caches, synchronous pre-start/snapshot/factory failures, an async-only plus succeeding synchronous validator collection, and configuration-driven monitor reload failure with zero notifications. The Options package's real generator builds the valid model, rejects the conflicting explicit implementation with SYSLIB1205, and builds/runs again after restoration. Exact artifacts and input hashes are saved. These managed checks are distinct from the source-only custom-factory/legacy-precedence restrictions; no GA, external-service or browser proof is claimed.
