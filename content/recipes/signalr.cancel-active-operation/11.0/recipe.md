# Stop abandoned hub work while keeping the live connection

## Goal

A fictional coastal survey console requests a sounding calculation. If the operator abandons that request, stop the server calculation and release its resources. On .NET 11, cancel that individual non-streaming invocation without disconnecting the console.

## Older approach — .NET 10

Passing a CancellationToken to a non-streaming InvokeAsync cancels the caller's wait, but does not ask a .NET 10 server to cancel that work. A valid older solution is a dedicated connection: the hub passes Context.ConnectionAborted to its work, and the caller stops that connection when it abandons the request. This also abandons other work on that connection and requires reconnecting before reuse. Applications that need finer control can retain an explicitly authorized, operation-scoped cancellation protocol; this comparison does not implement one. Streaming cancellation is a different, already-supported contract.

Create the complete file set below in an independent Soundings directory. The empty Directory files isolate surrounding build and central-package rules. The .NET 10 client package is pinned to 10.0.13, the SDK to 10.0.402 and the language mode to C# 14. Build and run from that directory with the matching SDK: `dotnet restore --use-lock-file`, `dotnet build -c Release -p:RestoreLockedMode=true`, then `dotnet ./bin/Release/net10.0/Soundings.dll`. Initial restore needs the listed packages and framework packs; installed Playbook guidance needs neither network access nor these sample dependencies.

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

#### .NET 10: `Soundings.csproj`

{{snippet:n10-soundings-csproj}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `SoundingHub.cs`

{{snippet:n10-soundinghub-cs}}

#### .NET 10: `SoundingWork.cs`

{{snippet:n10-soundingwork-cs}}

#### .NET 10: `Proof.cs`

{{snippet:n10-proof-cs}}

## Newer approach — .NET 11

Use the .NET 11 .NET client and ASP.NET Core 11 server together. The final token in InvokeAsync controls the invocation; the client sends the operation ID in a cancellation message when it is canceled. The hub declares a CancellationToken parameter supplied by SignalR, then passes that token through every cancellable operation. The client supplies only the ordinary id argument: never serialize a token as a hub argument.

SoundingHub.Calculate returns one Task result, not a stream. The held calculation uses WaitAsync with the supplied token so the example can prove cancellation without hoping a timer fired at the right moment. Replace it with real token-aware I/O or bounded loops that check the same token. A canceled client Task is not proof that server work has stopped: the independent HTTP observation waits for the server's cancellation branch. The subsequent Echo and successful calculation demonstrate reuse of the same live connection.

Create this complete alternative file set in a separate directory. The exact SDK 11.0.100-rc.2.26504.105, SignalR.Client 11.0.0-rc.2.26504.105 and C# 15 mode reproduce RC2 evidence; they are not claims that C# 15 syntax is necessary. Run the equivalent commands with the matching SDK and `net11.0` output path. No workloads, browser, external server, account or resource provisioning are required. The server chooses an ephemeral loopback port and shuts down automatically after the checks.

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

#### .NET 11: `Soundings.csproj`

{{snippet:n11-soundings-csproj}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `SoundingHub.cs`

{{snippet:n11-soundinghub-cs}}

#### .NET 11: `SoundingWork.cs`

{{snippet:n11-soundingwork-cs}}

#### .NET 11: `Proof.cs`

{{snippet:n11-proof-cs}}

## Migration

1. Check both deployed sides: a new client alone or a new server alone is insufficient. Use a .NET 11-capable .NET client and matching ASP.NET Core server assets. Keep the older fallback until both support this contract.
2. Add a CancellationToken parameter to each non-streaming hub operation that should observe abandonment. Remove the connection-aborted-only dependency for that operation; SignalR's supplied invocation token also observes connection closure.
3. Pass the caller's token as the final InvokeAsync cancellation argument. Forward the hub's supplied token into the calculation, downstream requests and cancellable waits. Cancellation does not undo effects already committed.
4. Handle OperationCanceledException on the caller when its own token was canceled. Do not infer server cleanup from that exception; observe application-specific completion when an acknowledgement matters.
5. Run the complete sample. Expect a canceled client task, server outcome canceled, Echo on the same connection, and a separate ordinary invocation that completes. For the old sample, expect server work still running after client cancellation, then canceled after StopAsync.

## Constraints

Non-streaming invocation; matched new client/server capabilities. This route requires the .NET client and ASP.NET Core 11 server. A .NET 10 client with a new server still cancels only its local wait. A new client with a .NET 10 server cannot retrofit invocation-token support. No older-target route, JavaScript/Java/Swift cancellation API, Azure SignalR Service behavior or MessagePack claim is made.

Cancellation is cooperative and races with successful completion. The server may finish before the cancellation arrives. Work that ignores its token continues. Letting OperationCanceledException escape the hub is an error completion on the server protocol, while the locally canceled client task has already stopped waiting. Do not treat it as a transactional rollback, durable job-cancellation acknowledgement or a guarantee of immediate resource release.

InvokeAsync tracks an invocation ID. SendAsync has no result-waiting invocation to cancel. Streaming uses its own existing cancellation contract; do not change a unary result into a stream just to copy this sample. ConnectionAborted is connection-wide, whereas the new hub parameter identifies the invocation and also reacts to disconnects.

The proof's singleton registry, HTTP controls, two parallel invocation slots, unguessable test IDs and skipped negotiation exist to observe real loopback WebSocket calls. They are not a production cancellation service or authorization scheme. Do not deploy the proof endpoints; retain trusted authentication, operation ownership checks and your normal transport/reconnect policy in a real application. The original sample is a bounded in-memory calculation, not a database, CPU benchmark or background-job system.

## Evidence

Both original consumers compile against their declared SDK, target, language and locked package assets. The saved compile records identify every input and resolved dependency. `eng/Test-Soundings.ps1` separately executes actual .NET clients against actual ephemeral Kestrel servers for all four .NET 10/11 client/server pairs. The matching new pair observes server cancellation and continued connection use; each missing-capability control observes still-running work before proving the disconnect fallback. The normal new invocation completes after explicit release. No mocked transport or server decision substitutes for execution.

The recipe preserves edition 11.0 and stable goal identity. The .NET 10 code is a comparison, not a ninth curated foundation. Stable cancellation APIs were exercised on .NET 11 RC2; this does not establish GA behavior or other platforms, transports, clients or hosting providers. See `docs/verification/PIN-814.md` for exact artifact hashes, execution and packed offline list/get proof. No required Human proof is outstanding for this local managed scope.

- [SignalR cancellation release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#cancel-hub-invocations-from-the-client)
- [The .NET client's cancellation contract](https://learn.microsoft.com/en-us/aspnet/core/signalr/dotnet-client?view=aspnetcore-11.0#cancel-a-hub-method-invocation)
- [Pinned RC2 client cancellation implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/SignalR/clients/csharp/Client.Core/src/HubConnection.cs)
- [Pinned RC2 server invocation dispatch](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/SignalR/server/Core/src/Internal/DefaultHubDispatcher.cs)
- [Pinned RC2 synthetic hub arguments](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/SignalR/server/Core/src/Internal/HubMethodDescriptor.cs)
- [Previous-release client implementation](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/SignalR/clients/csharp/Client.Core/src/HubConnection.cs)
