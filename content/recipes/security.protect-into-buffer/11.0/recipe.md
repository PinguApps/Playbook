# Protect data directly into an owned buffer

## Goal

Harbour Museum issues a collection token for a temporary loan. Its service already
owns the output storage and wants encryption to write there. On .NET 10 the valid
approach is to protect into a returned array, copy it and clear the temporary
arrays. On .NET 11, discover `ISpanDataProtector` on the actual `IDataProtector`
and pass the writer by reference. Retain the array fallback for other protectors.
The token's expiry policy is separate: ordinary `Protect` does not add a lifetime.

## Constraints: applicability and verified artifacts

The primary route targets **.NET 11 / ASP.NET Core 11**, using a normal
`FrameworkReference` to `Microsoft.AspNetCore.App`. No separate package, optional
workload, cloud connection, browser/rendering mode or experimental opt-in is
required. The samples use C#14 and C#15 respectively; the new writer call does not
depend on a C#15-only feature. The generic framework contracts allow ref structs;
this recipe deliberately uses a class writer. Interface availability is distinct
from a particular protector/encryptor's capability.

Verification pins are SDK **10.0.402**, C# **14.0**, .NET/ASP.NET Core **10.0.13**,
and SDK **11.0.100-rc.2.26504.105**, C# **15.0**, frameworks
**11.0.0-rc.2.26504.105**. These identify tested artifacts, not mandatory patch
upgrades. Both real DataProtection assemblies' runtime paths, product versions and
SHA-256 values are in the sample records. Their reference assemblies match the
bounded web source audit byte-for-byte:

| Framework | DataProtection.Abstractions reference SHA-256 | DataProtection reference SHA-256 |
| --- | --- | --- |
| 10.0.13 | `7e8e871b7e5f32a141df2131fd7be09f9516d34b28fed1d3aa40e06e9d02d1e1` | `bdeb427e44dc8689a647e37d38f07db50478a38e158d8480d8c36604e8eb197b` |
| 11 RC2 | `824e75212014f99bb2b0b10e26546fa6d4d12ae0205282b79789347d2dce3808` | `15180d442714c6daeb741c082011d1d66dee53edd5424a1c569dcb74f70fbd18` |

The installed compilers are Roslyn `5.9.0-1.26505.111` (SDK10) and
`5.12.0-1.26504.105` (SDK11). The source audit pins ASP.NET source
`4a70870818bf5662f21b0c28f9a8c267dccb77f0`; the actual runtime build identities are
`4099f83977f4d29604a9aeb971fb0a7bc1d5f699` and
`820230c386db5b0106c5430b3e5aee92a7826bf5`. Source intent and callable, executed
artifacts are qualified separately. .NET10 execution confirms both span interfaces
are absent; no older-target package backport is taught or advertised.

The measured host is Windows x64, using the framework's real
`Cng.CbcAuthenticatedEncryptor` with explicit AES-256-CBC/HMACSHA256. The OS supplies
its cryptographic backend; there is no separately restored native asset. This
does not establish other backends, operating systems, trimming or NativeAOT.
The public APIs carry no experimental diagnostic in these artifacts, but **RC2
execution is not actual-GA qualification**. PIN-959 coverage and PIN-964 web GA
reconciliation stay open; the exact eight approved .NET10 foundations and all
existing recipe/edition identities remain unchanged.

## Migration steps

1. Keep the application's existing purpose string, application discriminator and
   real key-storage policy. This test uses two disposable directories and never
   loads production/user keys. The sample key files are deliberately temporary
   and have no explicit at-rest encryptor; a custom filesystem location disables
   automatic at-rest protection. Protect production keys according to your threat
   model. Dispose providers before deleting the test directories.
2. Keep the valid .NET10 array route below. In a .NET11 consumer, check
   `protector is ISpanDataProtector`; do not cast every `IDataProtector` blindly.
   The framework implementation can itself fall back when the selected encryptor
   lacks the advanced span capability. A positive interface check promises neither
   zero allocations nor a universal allocation count.
3. Give `OwnedBuffer` exclusive access to an existing array. Bound payload sizes
   and provision capacity for your actual configuration. The test's payload+1024
   budget is a measured fixture allowance, not a general ciphertext-size formula.
   The writer refuses to grow and fails if the requested contiguous region does
   not fit. Input and AAD must not overlap destination storage: the adapter checks
   both before clearing and rejects overlap. Rejection also clears the destination,
   so an aliased source is discarded too. Use separate buffers, without concurrent access.
4. Call `MuseumToken.Transform(..., unprotect: false)` for protection and `true`
   for recovery. Consume only the successful `WrittenSpan`; its lifetime ends
   on reuse/clear/disposal. Do not retain spans or use array aliases afterwards.
   The caller owns source plaintext cleanup too. The fallback clears both temporary
   arrays after copying. This reduces application array plumbing; it is no measured
   end-to-end allocation/performance claim.
5. On every operation failure, clear the **entire** destination, including bytes
   obtained through `GetSpan` but not advanced. Never inspect, parse, log or use
   partial plaintext after failed authentication. Disposal clears the full storage
   before the caller reuses it; this does not erase other copies or strings.
6. Run the complete checks below on the actual framework. The capability-hiding
   wrapper is a fallback control around real crypto, not a cryptographic mock.
   Verify token/empty/1MiB round trips, wrong purpose/key ring, modified ciphertext,
   capacity/overlap refusal and successful/failed-output cleanup. Failure must reject the
   input and leave no readable destination bytes.
7. Use `ISpanAuthenticatedEncryptor` directly only when already maintaining an
   envelope/key-management integration. Obtain the real encryptor from an `IKey`,
   discover its span capability and supply the exact same AAD bytes on decrypt.
   The advanced sample proves matched AAD and rejected changed AAD/tampering for
   all three sizes. Its ciphertext lacks DataProtection's key id and purpose
   envelope; it cannot replace a normal protected token. Key selection, rotation,
   envelope design and custom algorithms are outside this goal.

## Caller-owned writer shared by both consumers

Save this as `OwnedBuffer.cs`. It wipes unadvanced storage too; `Advance` alone
cannot establish that failed cryptography left no plaintext behind.

{{snippet:owner}}

## Older approach: .NET 10

Create `MuseumBuffer.csproj` and `MuseumToken.cs` below. In this repository the
.NET10 directory also owns the shared `OwnedBuffer.cs` and `Program.cs`. Use SDK10 with C#14. No NuGet
dependencies or package lock are needed. The array API remains supported on 11.

{{snippet:n10-project}}

{{snippet:n10-token}}

## Newer approach: .NET 11

Use the matching SDK11/runtime/reference packs. The .NET11 project links the two shared files
from `../../net10.0/MuseumBuffer`. For a standalone project place them beside the
project and remove the two explicit `Compile Include` entries. The sample directories contain empty
`Directory.Build.props`, `Directory.Build.targets` and `Directory.Packages.props`
boundaries to avoid inheriting the tool host's policies; an application should
retain its own reviewed build configuration.

{{snippet:n11-project}}

{{snippet:n11-token}}

### Advanced encryptor adapter

Save this only in the .NET11 project. It does not choose or implement an algorithm.
The full check program creates an encryptor from the framework key and checks
its capability. A different encryptor may lack it; keep the existing supported
array/envelope path in that integration instead of assuming a universal cast.

{{snippet:advanced}}

## Evidence: complete real-framework check program

Save the following as the shared `Program.cs`. Each provider owns a fresh
directory below a GUID-named temporary directory. All providers are disposed
before final cleanup. Randomized large plaintext need not produce deterministic
ciphertext; the assertions establish recovery and rejection, not a fixed byte
sequence. The program also prints loaded artifact identities. Never use these
test keys for a deployed service.

{{snippet:checks}}

Run `dotnet run --project MuseumBuffer.csproj -c Release` from the corresponding
project directory using its matching SDK. In the repository use each sample's
`global.json` pin and `eng/Record-Evidence.ps1`; it builds with locked restore,
executes the actual DLL, and saves input hashes, resolved dependency set (empty),
commands, SDK/runtime output and behavior results. Both compilation/execution
records must be current before ordinary packing. Records bind exact input hashes;
their source commit/dirty flag records capture provenance, rather than asserting
that the capture happened at the eventual reviewed commit. Retrieval does not rerun crypto.

Expected: .NET10 prints the array route and absent interfaces. .NET11 prints the
real span protector, both span/array paths and real CNG encryptor; all round trips
and negative controls pass. Any authentication failure discards the entire output.
No supported older-target span route or selectable target10 edition is asserted.

## Official references

- [Span protector contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/DataProtection/Abstractions/src/ISpanDataProtector.cs)
- [Advanced span encryptor and AAD contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/DataProtection/DataProtection/src/AuthenticatedEncryption/ISpanAuthenticatedEncryptor.cs)
- [Framework span implementation and array fallback](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/DataProtection/DataProtection/src/KeyManagement/KeyRingBasedSpanDataProtector.cs)
- [Purpose isolation](https://learn.microsoft.com/aspnet/core/security/data-protection/consumer-apis/purpose-strings)
- [Key storage and protection at rest](https://learn.microsoft.com/aspnet/core/security/data-protection/implementation/key-storage-providers)
