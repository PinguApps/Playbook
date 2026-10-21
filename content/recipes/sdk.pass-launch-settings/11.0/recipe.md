# Keep launch arguments, build properties and environment values distinct

## Goal

Launch a depot audit with the intended shelf name and zone. Avoid mixing application arguments with SDK options or assuming that a process environment value automatically becomes an evaluated build property.

## Older approach

In SDK 10 a launch profile can supply a literal argument string and process environment. Save Properties/launchSettings.json with a Depot profile whose commandName is Project, commandLineArgs is `--shelf DepotProbe` and environmentVariables contains DEPOT_ZONE=profile. Run `dotnet run --project SdkWorkflows.csproj -c Release --launch-profile Depot`.

{{snippet:older}}

The application reports its actual arguments, environment and compiled configuration:

{{snippet:program}}

Setting DEPOT_ZONE in the parent process is another valid older route. It changes inherited process state, so restore that process variable after use. It is not a persistent user/machine environment change.

## Newer approach

SDK 11 can pass the zone with `dotnet run --project SdkWorkflows.csproj -c Release -e DEPOT_ZONE=cli --launch-profile Depot`. Its profile's commandLineArgs may use the literal MSBuild expression `--shelf $(AssemblyName)`. The SDK expands that expression against the evaluated project; with `-p:AssemblyName=NightDepot`, the application receives `--shelf` and `NightDepot` as two arguments.

{{snippet:newer}}

The -e option is before -- and sets the launched process environment. Repeated -e options supply several values; quote an entire NAME=value argument when it contains spaces. SDK 11 also exposes these assignments to build logic as RuntimeEnvironmentVariable items; they are not arbitrary global MSBuild properties. To set a build property use -p:Name=value. Do not put -p into an application argument list and expect a rebuild setting.

## Migration

1. Inspect the actual TargetFramework and selected SDK. Keep profile arguments and environment values separate in configuration.
2. Use -e before -- to override the launched environment for this command. The test proves that cli overrides the profile's zone.
3. Use -p:AssemblyName=NightDepot when the evaluated build value should change. The profile expansion reads that evaluated name.
4. Put literal application switches after --. The older check deliberately passes `-- -e DEPOT_ZONE=literal`; the program receives those strings and keeps its inherited zone.
5. Treat launch-profile expressions and values as trusted configuration. Avoid putting secrets in arguments, logs or reusable examples. The recipe needs no secret or service account.

## Constraints

An environment variable, MSBuild property, profile argument and application option are different boundaries. MSBuild evaluation does not promise shell expansion or arbitrary command execution in commandLineArgs. Literal $(...) inside PowerShell double quotes invokes PowerShell substitution; preserve profile expressions in JSON or a single-quoted PowerShell string. The profile argument expansion and SDK -e item contract demonstrated here use SDK 11; target-10 SDK routes are not declared by this edition.

This managed console route needs no provider, rendering mode, native workload or package. It does not establish environment behavior inside containers or child services. SDK 11 proof is RC2, not GA.

## Evidence

Actual .NET 10/11 programs report their process inputs as JSON. The real commands distinguish inherited and profile variables, the SDK 11 command-line override, evaluated AssemblyName expansion and literal arguments after --. Both projects compile independently. Installed stdio checks compare full canonical Markdown and applicability; these checks do not infer compatibility from SDK presence alone.
