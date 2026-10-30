# Handle ZIP failures during asynchronous initialization

## Goal

A weather importer reads the station names in an uploaded ZIP before scheduling imports. Reject a damaged directory without publishing a partial list, regardless of whether the failure happens while opening the archive or traversing its entries.

## Older approach

.NET 10 already provides the five-argument ZipArchive.CreateAsync factory. With a readable, seekable stream it reads the end record asynchronously, but can defer central-directory entry loading until Entries is accessed. A missing end record already fails at the awaited factory; malformed directory entries can fail later. Keep both operations inside one InvalidDataException boundary. This remains a valid older approach, and the same canonical method below compiles and runs on net10.0 / C# 14.

An asynchronously initialized archive in .NET 10 is not a promise that Entries avoids synchronous reads. Use a suitable stream or explicitly stage a bounded input in seekable storage if your stream forbids them; staging needs a separate size/resource policy. Do not block on an async task to imitate the newer behavior.

## Newer approach

Use a net11.0 / C# 15 console project with implicit usings and import System.IO.Compression. Put this method inside a class. The framework supplies ZipArchive; no additional package, provider, workload or opt-in is required. The caller supplies a readable stream positioned for archive processing and owns its lifetime:

{{snippet:directory}}

For a file, keep an outer await-using FileStream scope around the method call. That scope must exist before awaiting the factory: no archive is returned on failure. A null result means the directory was rejected; an empty array can represent a valid empty archive. Cancellation propagates separately from invalid archive data.

In .NET 11 Read mode, the awaited factory loads the central directory asynchronously before returning. Entries then uses that loaded metadata without reading the underlying stream. The broader boundary above works across both releases and avoids relying on one exception timing. It does not catch file-opening I/O/permission errors or turn cancellation into a corrupt-input result.

## Migration

1. Find handlers that surround only Entries access. Move the awaited CreateAsync call into the same InvalidDataException boundary.
2. Establish stream ownership before the await. This method uses leaveOpen=true; the caller disposes source on success, cancellation and factory failure.
3. Publish the collected names only after complete traversal succeeds. Treat null as rejection and preserve an empty successful list.
4. Pass cancellation to CreateAsync. Keep cancellation distinct from damaged ZIP data and let your application's cancellation policy handle it.
5. Test a valid end record with malformed central entries, a missing end record, an async-only readable stream and successful metadata with a damaged payload. Read every required payload to EOF before claiming its CRC was checked.

## Constraints and alternatives

The change concerns directory initialization, not authentication or complete payload validation. The real Stored-payload fixture has a successful directory on both releases; .NET 11 detects the unchanged recorded CRC versus altered bytes only during a complete payload read. .NET 10 accepts that fixture's full read. Merely listing entries, opening an entry or reading a prefix does not establish payload integrity, and CRC is not sender authentication.

The focused async-only proof uses a real valid ZIP on a readable, seekable tracking stream. RC2 performs five async read calls during initialization and zero reads during Entries access; the .NET 10 factory succeeds but Entries attempts a prohibited synchronous read. Those counts describe this fixture, not a performance contract. Non-seekable Read inputs can be buffered internally; no bounded-memory or streaming-network guarantee is implied. Update can load/validate existing data too; Create has no existing directory. This recipe demonstrates Read mode, not those other modes.

On successful disposal, leaveOpen=false closes the supplied stream and true retains it. A failed factory on the tested seekable inputs leaves the supplied stream open even with false; pre-cancellation also occurs before ownership transfer. Always retain a caller cleanup scope. Disposal does not roll back downstream import work. Apply your own archive limits and path policy before extraction; this example only lists names.

The exact proof baselines are SDK 10.0.402 / 11.0.100-rc.2.26504.105, C# 14 / 15, and framework reference/runtime 10.0.13 / 11.0.0-rc.2.26504.105. These identify tested artifacts separately from release-family prerequisites. The APIs used have no experimental annotation or required experimental opt-in on the inspected assets. No separate NuGet or older-target catalogue route is advertised. Desktop/server stream processing is applicable; browser/WASI and rendering modes are not exercised. RC2 execution does not establish GA verification.

## Evidence

WeatherImport checks execute both actual consumers. Two original fixtures retain a valid end record but alter a central signature or entry count: .NET 10 rejects at Entries; RC2 rejects at the awaited factory. Both reject a missing end record at initialization. Checks cover the canonical successful/rejected results, real async-only reads, pre-cancellation, successful leaveOpen ownership, caller cleanup after failure and complete reads of structurally valid damaged Stored data. Trackers instrument real ZipArchive processing, not a mocked archive.

Saved input-hashed compilation/execution records identify exact compiler/ref/runtime assemblies and fixture hashes. The ordinary packed/installed tool serves this full canonical text and target11/edition11 applicability offline, while target10 is a comparison rather than another foundation. Existing archive access and full-corruption goals retain their IDs. PIN-959 qualifying coverage and PIN-962 actual-GA reconciliation remain open.
