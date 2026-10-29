# Refresh credentials without replacing a live hub connection

## Goal

A fictional quarry reading station starts with visitor clearance and later grants surveyor clearance to the same person. .NET 10 reconnects deliberately. .NET 11 refreshes the authenticated principal on the existing connection. The old reconnect approach remains valid when a server or client cannot refresh, or when the actual user changes.

The local `LocalTicket` scheme accepts deterministic test strings. It is deliberately **not production authentication**. Replace it with your trusted cookie/JWT configuration; never accept `name:clearance` from real users. The assertions exercise the actual authentication middleware, Kestrel, SignalR endpoint and WebSocket transport; no hub or transport is mocked.

## Migration steps

1. Use a .NET 11 server and compatible refresh-capable client. Enable `EnableAuthenticationRefresh` per hub and retain `CloseOnAuthenticationExpiration`. Refresh needs negotiation; do not use skip-negotiation as a shortcut.
2. Supply current credentials through `AccessTokenProvider` (.NET) or `accessTokenFactory` (TypeScript). A refresh fetches application credentials again. The samples disable automatic scheduling to make accept/reject checks deterministic; automatic refresh requires a positive server-reported lifetime.
3. Use `OnAuthenticationRefresh` to reject disallowed claims. The built-in same-user check also remains: refresh is not a way to change the connection's user. Reconnect when identity/routing needs to change. Production policy must validate tenant and other application boundaries too.
4. Handle success and failure events. Rejection is HTTP 403; the example proves the connection retains its last accepted principal. A failed refresh does not itself disconnect. Do not retry a policy rejection blindly; obtain valid credentials or sign in again. Expiry still closes the connection when configured.
5. Override `OnAuthenticationRefreshedAsync` for post-refresh work. Later hub calls see accepted claims. Work already in progress and application-owned caches/groups need their own policy; this sample makes no concurrent-invocation or group-recalculation promise.
6. For negotiate redirection, keep the application-token factory. The original application's `/refresh` endpoint must implement the redirecting server's refresh contract, including any replacement transport token. The local gateway below forwards refresh to its final local hub and returns a replacement token. Removing it yields HTTP 404 at `/gateway/refresh`, even though initial negotiation connects successfully. This bounded local gateway check establishes no Azure SignalR Service capability.

## TypeScript artifact qualification and offline setup

At verification, npm `@microsoft/signalr` latest was 10.0.11 and next was 11.0.0-preview.7.26381.103. The latter lacks the final refresh API. Do not install a fictional matching RC2 npm version. The current comparison builds actual first-party sources at `4a70870818bf5662f21b0c28f9a8c267dccb77f0`, with its MIT license retained in generated output. The resulting client is a **development source build**, not a Microsoft-published/supported RC2 npm package. A production consumer needs a qualified matching client artifact; this recipe does not silently substitute the published Preview7 client.

Save the complete TypeScript files together. Run Node's npm CLI by absolute path to install dependencies with scripts disabled; `npm install --ignore-scripts --no-audit --no-fund` creates a lock, or use `npm ci` with the repository's supplied lock. Run `Build-Client.ps1`, then the local `node_modules/typescript/bin/tsc --project tsconfig.json`. The build downloads and SHA-256 checks the pinned first-party archive; network access is needed for this preparation. No source fetch is needed to read the installed guidance. The TypeScript 4.9.5 compiler and Node 24.21.0 identify verification, not a general production support policy.

Run each .NET project with `dotnet run -c Release` to execute its actual .NET client checks. To run the TypeScript check against that server, run the project with `-- --serve --urls http://127.0.0.1:5198`, then `node check.js http://127.0.0.1:5198/quarry 10` for the older server or `11` for current. Also run the current check with `/gateway`. Use your resolved absolute executables and the matching project SDK. These are local ephemeral test listeners, not production resources.

## Constraints

Both local hosts register ephemeral Data Protection. The demonstration does not
require persistent protection keys; production key storage and identity validation
remain application-owned choices.

Measured SDKs are 10.0.402 and 11.0.100-rc.2.26504.105; frameworks and .NET SignalR packages are 10.0.13 and 11.0.0-rc.2.26504.105. Locked dependencies and source/tool hashes accompany the evidence. No down-level client/server refresh route, automatic expiry scheduling, Windows authentication, cloud provider, browser TypeScript transport, every SignalR transport, trimming/AOT or actual-GA support is proved. TypeScript ran on Node with WebSockets. Use TLS and trusted token validation in production.

Blazor UI refresh is a separate subscription concern: see `blazor.react-to-refreshed-identity`. Framework propagation is context, not proof that an arbitrary authentication provider refreshes itself.

## Older approach

The complete original older consumer preserves its explicit reconnect or refresh action.

### .NET 10: `CopperRidgeHub.csproj`

{{snippet:n10-copperridgehub-csproj}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

## Newer approach

The complete current consumer uses the new refresh or notification contract.

### .NET 11: `CopperRidgeHub.csproj`

{{snippet:n11-copperridgehub-csproj}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

## Complete TypeScript files

### TypeScript: `package.json`

{{snippet:ts-package-json}}

### TypeScript: `package-lock.json`

{{snippet:ts-package-lock-json}}

### TypeScript: `tsconfig.json`

{{snippet:ts-tsconfig-json}}

### TypeScript: `Build-Client.ps1`

{{snippet:ts-build-client-ps1}}

### TypeScript: `check.ts`

{{snippet:ts-check-ts}}

## Evidence

The saved evidence names the exact commands, inputs, SDKs, resolved packages and actual execution. `eng/Record-LiveIdentity.ps1` captures the real .NET/TypeScript and browser checks. Installed stdio inspection verifies delivery separately; it does not prove production identity-provider support.
