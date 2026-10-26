# Call a secured file-based endpoint with a development token

## Goal

A harbour keeper needs to try a protected permit endpoint locally. The token is disposable development input; it proves the request uses the configured local signing key, not that a real identity provider has authenticated a person.

## Requirements

The .NET 10 comparison uses a small Web SDK project and the existing `--project` selector. The .NET 11 file has the same endpoint and uses `dotnet user-jwts create --file quay.cs`. Keep the shared file-app setup explicit: Web SDK directive, target, central package file, user-secrets ID and managed publishing choice. The package directive is versionless because the supplied central file owns its pin. See the related `sdk.compose-file-apps` goal for growing a file utility; this recipe includes everything needed for this endpoint.

## Migration

1. Save the chosen target's canonical files below. In its application directory, create `appsettings.Development.json` containing `{}`. Keep that generated configuration, tokens and signing keys out of source control.
2. Use the absolute path to the selected SDK executable. Set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` before SDK first-run setup and `ASPNETCORE_ENVIRONMENT=Development` for this example. This also enables the normal WebApplication user-secrets loading.
3. On .NET 11 run `dotnet user-jwts create --file quay.cs --name HarbourKeeper --audience http://127.0.0.1:5093 --valid-for 5m --appsettings-file appsettings.Development.json --output token`. On .NET 10 replace `--file quay.cs` with `--project QuayTokens.csproj`. The tool writes the development signing key to the selected app's user secrets and configures its bearer scheme in the chosen appsettings file. Never paste the output into documentation.
4. Run `dotnet run --file quay.cs -- --urls http://127.0.0.1:5093` on .NET 11, or `dotnet run --project QuayTokens.csproj -- --urls http://127.0.0.1:5093` on .NET 10. The listener URL must match the token audience. Call `GET /permit`: no bearer token gives 401, the generated token gives 200 with `HarbourKeeper may inspect W3.`, and a damaged signature gives 401.
5. Stop the app and remove disposable credentials/configuration when finished. For repeated verification use `eng/Test-WebTooling.ps1`: it copies the exact files to unrelated temporary directories, sets an isolated APPDATA/CLI home, asserts where the signing key was saved, kills its own listeners and deletes that home. It preserves shared PATH and user secrets.

This is a development authentication check. It does not configure production issuance, authorization policies, certificate trust or account access. The sample refuses a non-Development environment. The explicit `PublishAot=false` keeps the file route managed; no native AOT behavior was tested.

## Older approach: .NET 10

Save these at the shown paths under an empty working directory. Keep the directory boundary files; they prevent unrelated parent build rules from changing this example.

### `samples/net10.0/QuayTokens/Directory.Build.props`

{{snippet:n10-directory-build-props}}

### `samples/net10.0/QuayTokens/Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### `samples/net10.0/QuayTokens/Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### `samples/net10.0/QuayTokens/QuayTokens.csproj`

{{snippet:n10-quaytokens-csproj}}

### `samples/net10.0/QuayTokens/Program.cs`

{{snippet:n10-program-cs}}

### `samples/net10.0/QuayTokens/global.json`

{{snippet:n10-global-json}}

## Newer approach: .NET 11

Save these at the shown paths under an empty working directory. Keep the directory boundary files; they prevent unrelated parent build rules from changing this example.

### `samples/net11.0/QuayTokens/Directory.Build.props`

{{snippet:n11-directory-build-props}}

### `samples/net11.0/QuayTokens/Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### `samples/net11.0/QuayTokens/Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### `samples/net11.0/QuayTokens/quay.cs`

{{snippet:n11-quay-cs}}

### `samples/net11.0/QuayTokens/global.json`

{{snippet:n11-global-json}}

## Constraints

Development only: the sample refuses other environments. A user-jwts signing key is not a production issuer or an identity provider. Keep generated tokens and user secrets outside source control.

The new selector needs the .NET 11 SDK. The .NET 10 comparison uses a real Web SDK project with --project; it is comparison evidence, not a target-10 catalogue route.

Run commands from the application directory, create appsettings.Development.json containing {} before using --appsettings-file, and set the same audience URL used by the server. WebApplication loads its UserSecretsId in Development.

The file uses the shared SDK file-app directives, versionless #:package plus the supplied CPM boundary, and PublishAot=false. No native AOT, trimming, production authorization policy or browser flow is claimed.

## Evidence

The exact SDK, language and package inputs are recorded with the samples. Saved compilation records establish the canonical code builds; the separate repeatable `eng/Test-WebTooling.ps1` command establishes real CLI/generated-server behavior. No mocks or cross-compilation are used. Installed-package checks independently compare the entire expanded body and applicability over stdio from an unrelated directory; that delivery check does not replace the runtime checks. GA reconciliation and comprehensive product publication gates remain outstanding.

- [Development JWT file selector](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#dotnet-user-jwts-supports-file-based-apps)
- [File directives, package management and AOT defaults](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps)
- [Development JWT CLI](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-10.0)
