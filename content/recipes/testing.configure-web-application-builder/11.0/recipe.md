# Configure a web integration test through the application builder

## Goal

A geology API reads its catalogue source during startup. Its tester needs an
isolated configuration and catalogue without editing production settings. Use
the new early factory hook when startup itself must observe those values, and
test the real entry assembly through HTTP.

## Requirements

Keep the executable API and test harness in separate projects. The complete
StrataTests project references App/StrataApi.csproj and the matching
Microsoft.AspNetCore.Mvc.Testing package, which supplies TestHost and
DependencyModel. Its application really calls WebApplication.CreateBuilder and
app.Run; WebApplicationFactory discovers the entrypoint and supplies TestServer.
No mocked builder, external service, browser or package-native asset is used.

Exact previous pins: SDK10.0.402/framework10.0.13/C#14 and Mvc.Testing/TestHost/
DependencyModel10.0.13. Current pins: SDK11.0.100-rc.2.26504.105, framework and
matching packages11.0.0-rc.2.26504.105, C#15. These identify reproduction, not
minimum servicing versions. Windows x64 is the executed lane. The newer hook
targets the .NET11 framework/package family with no experimental opt-in or
C#15-specific syntax; actual-GA reconciliation remains open.

## Constraints

ConfigureWebApplicationBuilder(IHostApplicationBuilder) runs during the real
entrypoint's WebApplication.CreateBuilder call. Configuration and services
added there are visible immediately afterward. The sample application respects
an existing source value with ??= and an existing service with TryAddSingleton.
Later unconditional application writes/registrations can replace early test
settings; this hook does not grant the fixture permanent precedence.

ConfigureWebHost remains useful on both targets. The older comparison registers
configuration and replaces the singleton there. The request observes fixture/
test-core, but startup already captured production. A separate late control on
both runtimes observes startup=production and current=late. Choose the seam by
when the application reads the value, rather than assuming all callbacks run
before startup code. The test assembly's WebApplicationFactoryContentRoot
attribute identifies the output directory with the copied StrataApi.dll as its
marker. Its -100 priority precedes the generated project-root attribute.
Discovery succeeds before ConfigureWebHost runs, even outside any solution.
Update the attribute key/marker when renaming or versioning the application.
UseContentRoot only in that later callback cannot prevent an earlier solution
discovery failure. The app uses
no physical views/assets; a real content application needs its correct root.

The early override is supported only for the WebApplicationBuilder entry path.
The newer negative controls return actual IHostBuilder and IWebHostBuilder
instances and verify the factory rejects both. Their historical WebHostBuilder
use has a narrow ASPDEPR004 suppression solely to exercise that rejection.
Do not carry that suppression into the normal application or describe this hook
as a general Generic Host configuration callback.

ConfigureHostApplicationBuilder was a spelling in earlier .NET11 previews.
Only a fixture using that preview API needs that rename. .NET10 has no such
method to rename and retains its working ConfigureWebHost alternative. No older
target backport or actual earlier-preview package execution is asserted here.

Await factory.DisposeAsync. The test singleton is registered with a factory
delegate so the host owns it; its asynchronous disposal yields before setting
the observed flag. The app's Run() teardown can overlap factory teardown: an
initial standalone check observed factory disposal return before that flag was
set. The test therefore also awaits the singleton's DisposalCompleted task with
a deadline before checking the flag. Do not infer that factory disposal alone
joins every asynchronous application cleanup operation.
An instance supplied directly to DI has different ownership responsibilities.
Factories are independent: the uncustomized control still returns production/
production-core. No production file or persistent environment setting changes.

## Migration

1. Identify the actual entry assembly and the time at which startup reads test
   configuration. Retain ConfigureWebHost when only late request-time replacement
   is needed; .NET10's valid setup remains demonstrated below.
2. Build the matching .NET11 Mvc.Testing/TestHost consumer. Override the protected
   ConfigureWebApplicationBuilder(IHostApplicationBuilder) method to add a local
   configuration source and host-owned test service before application startup
   reads them. Rename ConfigureHostApplicationBuilder only in a prior preview
   fixture that actually used that spelling.
3. Keep intentional application precedence explicit. The sample uses ??= and
   TryAddSingleton; an app that always assigns the production value must define
   its own override policy. Do not change global environment or production files
   merely to make tests win.
4. Copy every corresponding file below, preserving App/. Select that root
   project directory, set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in the child and
   build Release with the matching absolute SDK executable. Initial restore
   creates both lock files; retain them for subsequent --locked-mode restores.
   Execute bin/Release/net10.0/StrataTests.dll or net11.0/StrataTests.dll with
   the same matching executable. The real app entrypoint is exercised by the factory.
5. Expect newer startup/current=fixture with test-core; the older late approach
   keeps startup=production and current=fixture. Both uncustomized controls remain
   production/production-core. The separate late control keeps startup=production
   and current=late. Newer explicit legacy builder paths must be rejected.
6. Use await using for the factories and await the owned service's explicit
   completion signal to verify async cleanup finishes even when application and
   factory teardown overlap. TestServer exercises the real application pipeline but does not
   prove native socket/TLS/HTTP3 or a deployed host. Keep actual-GA/publication
   and unrelated Human checks open.

## Older approach: .NET 10

Copy every file below at its relative path into an independent directory.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### StrataTests.csproj

{{snippet:n10-stratatests-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

### App/StrataApi.csproj

{{snippet:n10-app-strataapi-csproj}}

### App/Program.cs

{{snippet:n10-app-program-cs}}

## Newer approach: .NET 11

Copy every file below at its relative path into an independent directory.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### StrataTests.csproj

{{snippet:n11-stratatests-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

### App/StrataApi.csproj

{{snippet:n11-app-strataapi-csproj}}

### App/Program.cs

{{snippet:n11-app-program-cs}}

## Evidence

Run eng/Test-WebResourceRecipes.ps1 -RecordEvidence from the task worktree to
refresh the six actual consumers, diagnostic controls and audited artifact hashes.
The saved sample records include exact commands, locked assets, native identities,
inputs and observed output. Run the installed-artifact Inspector and returned-body
consumer commands in docs/inspect-guidance.md. No recurring OS/client/native matrix
is added. The eight approved .NET10 foundations and existing edition IDs remain
unchanged; this comparison is part of one new 11.0 edition.

## References

- [Factory hook, order and supported entry paths](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs)
- [Previous factory has no early hook](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs)
- [Application builder construction](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/DefaultBuilder/src/WebApplicationBuilder.cs)
- [Integration testing with the real application](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-11.0)
