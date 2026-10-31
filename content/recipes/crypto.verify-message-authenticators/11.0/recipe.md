# Verify message authenticators

## Goal

A fictional Mallow Quay buoy sends a packet and its message authentication code
(MAC). Verify that the expected shared key authenticates the exact bytes before
accepting the packet. The protocol fixes the algorithm and a 32-byte tag length;
neither comes from an untrusted sender. This does not provide encryption, replay
protection, sender authorization or proof against someone who also holds the key.

## Older approach

The valid .NET10 approach computes the MAC and compares it with
CryptographicOperations.FixedTimeEquals. Ordinary byte/string equality is not an
appropriate replacement. Incremental GetCurrentHash preserves accumulated data;
GetHashAndReset finishes it and starts an empty message.

{{snippet:older}}

## Newer approach

.NET11 provides verification helpers that combine computation and comparison.
The original protocol policy still applies, particularly for variable-length KMAC.
The incremental helpers below validate exactly32-byte tags before accessing or
resetting the accumulated state.

{{snippet:newer}}

Current preserves state after both success and a same-length mismatch. Finish
resets after both success and a same-length mismatch. Those are different caller
decisions; resetting after a failed comparison still discards the accumulated
message. Do not reuse a failed stream's partial state as an authenticated packet.
The sample uses IncrementalHash.CreateHMAC(SHA256,key), not an unkeyed hash.

## Migration

1. Keep the exact authenticated bytes, algorithm, shared-key handling and tag
   encoding. The synthetic fixture uses key bytes00..1F and UTF-8
   `Mallow Quay|buoy=7|offset=-9|sequence=42`, without a newline. Its expected
   HMAC-SHA256 is02907D3EDAC15E52E6C0051B65B7E0902866C47B2A9791EBEEE9BBF8C2627FC9,
   independently generated using Python hmac/hashlib. These public keys are test
   data, never credentials for real packets.
2. Target net11.0 and replace HashData plus FixedTimeEquals with HMACSHA256.Verify.
   Reject malformed protocol lengths first. Raw HMACSHA256.Verify throws
   ArgumentException for lengths0/31/33; it does not return false for those shapes.
   The application wrapper consistently returns false for non32-byte tags on
   both releases, preserving the older caller contract.
3. For streams, use VerifyAsync with an explicit CancellationToken. It consumes
   the stream from its current position and leaves ownership with the caller.
   Cancelled verification throws OperationCanceledException; it is not a failed
   MAC comparison or rollback of bytes already read. The sample actually supplies
   a12-byte prefix, pauses the second read, then cancels the in-flight operation.
4. For accumulated HMAC data, use VerifyCurrentHash to inspect without resetting,
   or VerifyHashAndReset to finish. Pass full SHA256 tags; do not assume malformed
   length exceptions perform a reset. The checks exercise prefix/full messages,
   preserved state after mismatch and empty-message state after both successful
   and failed same-length final verification.
5. Use KMAC only when Kmac128.IsSupported is true for the actual runtime/provider.
   Keep the customization bytes and chosen output length in the protocol. Here
   UTF-8 `Mallow packet v1` and32 bytes produce
   B13A6D6DD298CBA1BF0832444C29B6B9568B300C6F7A001BCF65696A1BC2A2C3,
   independently checked with OpenSSL3.5.5 CLI KMAC128. Changing or omitting the
   customization makes verification fail. An unsupported provider cannot establish
   positive KMAC proof; the authoring check requires support and executes it.
6. Keep an explicit KMAC length guard. Raw Kmac128.Verify uses the received
   nonempty tag's length as the requested output size; a valid31-byte KMAC is
   accepted by that API, while an empty tag throws ArgumentException. The packet
   protocol rejects both31-byte and empty tags before that call. A shortened KMAC
   is not generally a prefix of a longer one. Kmac128 incremental verification has
   the same preserve/reset choice, exercised with protocol-length tags.

## Constraints

Primary route: .NET11 SDK/runtime, net11.0/C#15, framework cryptography assets.
Exact proof uses SDK11.0.100-rc.2.26504.105 and ref/runtime11.0.0-rc.2.26504.105;
the older consumer uses SDK10.0.402, net10.0/C#14 and ref/runtime10.0.13. No
additional NuGet package, experimental diagnostic opt-in, workload or rendering
mode is required. SDK patch pins identify reproduction rather than universal
consumer requirements. No package backport route is advertised.

Windows x64 CNG execution is measured with Kmac128.IsSupported=true on both
runtimes; saved output identifies bcrypt10.0.26100.1 and
bcryptPrimitives10.0.26100.9711 with hashes. Support on another Windows version,
OpenSSL build, Apple or other target must be checked there; source/API presence
does not prove an available provider. The OpenSSL CLI known-answer cross-check
is separate from these Windows executions. No broad platform/provider matrix,
FIPS certification, timing benchmark, allocation guarantee, Native AOT or GA
execution is claimed. SHA256 HMAC remains valid when a protocol does not use KMAC;
never silently substitute one algorithm for another in an existing wire contract.

## Evidence

eng/Test-ClassifierAuthentication.ps1 -Record -VerifySources executes both real
consumers and requires positive local KMAC support. Checks cover known answers,
valid/bad/empty/truncated tags, wrong keys/data, actual stream completion and
in-flight cancellation, incremental preserve/reset on success and mismatch,
KMAC customization and raw API length behavior. Evidence retains exact commands,
inputs, resolved dependencies (none beyond the framework), audit-matching reference
hashes, compiler/runtime implementation and native provider identities.

The ordinary packed tool is installed from an isolated local-only feed/cache and
queried through real stdio list/get from an unrelated directory. Inspection checks
ID/title/goal, complete canonical Markdown, target11/edition11, applicability,
fresh evidence and matching structured/text JSON offline. Target10 returns
no_applicable_edition. Exactly eight foundations and existing identities remain;
PIN-959/PIN-962 and other Human gates stay open. RC2 proof is not final-GA proof.
