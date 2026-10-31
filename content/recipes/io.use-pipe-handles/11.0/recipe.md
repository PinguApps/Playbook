# Use pipe handles with explicit ownership and platform-aware I/O

## Goal

Move a short sensor report through a pipe while making the owner of each handle clear. A pipe is a sequential byte channel, not a seekable file. Reads can be partial; they need not preserve write boundaries. The example limits a report to 256 bytes. Larger protocols need concurrent reading/writing, framing, cancellation and bounded buffering.

## Older approach: .NET 10 / C# 14

The existing pipe-stream abstraction is valid when a stream is what the application needs. Import `System.IO.Pipes`; create a nullable-enabled console project and keep the class in a separate file. Both streams share the **same SafePipeHandle instance** for the client end: the reader disposes it before the server's remaining cleanup. Do not independently close its raw OS value or call DisposeLocalCopyOfClientHandle while this same-process reader uses it.

{{snippet:older}}

## Newer approach: .NET 11 / C# 15

Import `Microsoft.Win32.SafeHandles` and `System.IO`. CreateAnonymousPipe returns independently owned read/write SafeFileHandles. Each must be disposed. Its default flags are `asyncRead=false`, `asyncWrite=false`; Type identifies the pipe ends. RandomAccess read/write now accept unseekable handles. Supply a nonnegative offset, such as zero: it is ignored for a pipe, and does not make the pipe seekable. A zero read means EOF after every writer is closed.

{{snippet:newer}}

The synchronous bounded example is not a throughput design. Keep writers concurrent with readers once messages can fill the OS buffer. FileStream wrappers take ownership of supplied SafeFileHandle instances; dispose the wrappers and never use those handles afterwards. Passing the same SafeHandle object through coordinated owners is different from constructing competing wrappers around an unowned raw descriptor.

## Connected named handles and SYSLIB0063

The following methods **consume ownership of the supplied SafePipeHandle instance**. The handle must already represent a connected pipe, with its actual async mode supplied accurately. If obtained from another stream, retire that stream from I/O and let its later Dispose be idempotent cleanup. Never reuse it after the wrapper closes the handle. An API signature does not connect an unconnected native handle.

.NET 10's constructor accepts `isConnected`; the historical implementation uses that flag to set stream state. For a connected handle, true is the valid older approach:

{{snippet:older-handle}}

The current four-argument constructor warns SYSLIB0063 and delegates to the replacement; its isConnected argument has no effect on that current implementation. Remove the argument rather than suppressing the warning:

{{snippet:newer-handle}}

## Unix flags, permissions and packet elements

On current Unix, IsAsync reflects nonblocking descriptors. A regular file opened with FileOptions.Asynchronous can report **false** on both SafeFileHandle and FileStream, while ReadExactlyAsync still works. Do not reject async-method use based on that flag, infer kernel-asynchronous regular-file I/O, or assume a cached flag notices later external fcntl changes. For created pipes, request each end's mode explicitly and pass the matching mode when wrapping it in FileStream.

A FileStream-backed `new SendPacketsElement(stream, offset, count)` accepts a synchronous stream on current non-Windows; Windows retains the async-stream constructor guard. This is constructor evidence, not proof of a completed SendPacketsAsync operation.

Use `PipeOptions.CurrentUserOnly` for same-user named-pipe communication. Current Unix binds the socket node with mode 0600. Adding a restricted instance to an existing shared name tightens its node; a later unrestricted instance cannot loosen it during that shared server lifetime. Removing the flag is not a verified cross-user solution: directory access, umask and peer checks also matter. This recipe provides no cross-user route.

## Migration

1. Choose pipe streams or raw handles according to the consumer; retain streams when seeking or low-level handle access is unnecessary.
2. Set an explicit owner for every returned/transferred handle. Use one coordinated SafeHandle instance, never two independent owning wrappers for the same OS value.
3. Replace the obsolete connected-handle constructor's four arguments with three, keeping real connection state and async mode correct.
4. Treat pipes as sequential; loop over partial reads, frame messages, and arrange concurrent reads/writes before increasing message sizes.
5. Remove IsAsync-based bans on async methods. Check Windows packet-element guards separately and retain same-user access policy.

## Constraints and alternatives

Framework APIs only, no package, provider account, optional workload, rendering mode or experimental opt-in. The current low-level API requires .NET 11 references/runtime; it is not advertised for net10. Existing AnonymousPipeServerStream/NamedPipeClientStream remain valid higher-level alternatives. Named Unix pipes use socket paths; do not apply Windows raw HANDLE assumptions to Unix sockets. Platform process-spawning, handle inheritance and cross-user access are separate goals.

## Evidence

`eng/Record-NativeBoundaries.ps1 -VerifySources` compiles and executes actual .NET 10.0.13/SDK 10.0.402 and .NET 11RC2/SDK 11RC2 consumers on Windows x64, then executes the same managed artifacts in digest-pinned network-none Linux containers. Canonical reports, connected-handle transfer/disposal, all four current blocking/nonblocking end combinations, FileStream ownership and EOF pass. The unchanged old constructor compiled against current references fails specifically with SYSLIB0063; supported code builds with warnings as errors.

Linux regular-file flags change true/true to false/false while asynchronous reads succeed. Synchronous SendPacketsElement construction changes from rejection to acceptance there; Windows retains rejection. With measured umask0022, Unix socket modes are755/755/755 on the older runtime versus755/600/600 on current shared-path tightening; first restricted bind is600 currently. These measured modes do not promise a universal older umask. Temporary socket/file/container state is removed. No cross-user, SendPacketsAsync transfer, arbitrary external descriptor mutation or actual-GA claim follows.
