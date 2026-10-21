# Protect ZIP entry contents with a supported password algorithm

## Goal

A lighthouse survey contains private readings. The recipient must support the selected encryption method and receive the password through a separate trusted channel.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Formats.Tar. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

.NET 10 has no built-in ZIP encryption equivalent. The comparison fails explicitly instead of returning an unencrypted archive. A separately evaluated archive library/tool is a valid older route, but no third-party dependency or compatible package route is verified here.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

Use AES-256 for newly protected archives, and pass the password both when creating the entry and opening it. The directory helper shows the complete asynchronous creation/extraction option path, with overwrite disabled and cancellation passed through.

{{snippet:newer}}

## Migration

1. Keep the consumer's actual target explicit. Upgrade to .NET 11 before adopting this edition; no conditional older-target route is advertised.
2. Test the original survey, the damaged/empty case and resource cleanup before replacing the previous path.
3. Use the declared matching SDK/framework artifacts and rerun the focused checks below when source, target or dependency inputs change.

## Constraints

WinZip AES-128/192/256 and legacy ZipCrypto round-trip in this baseline. Prefer AES-256; ZipCrypto is weak and suitable only for deliberate legacy interoperability. Password entropy still matters; ZIP passwords are not account authentication, a key vault or a substitute for a reviewed secure-transfer design. Entry names and archive metadata remain visible. An empty password for an encrypted entry throws ArgumentException; a wrong password throws InvalidDataException in the tested cases. Unknown encryption is unsupported and must not be silently downgraded. Read to EOF before accepting plaintext because authentication/integrity can fail late. Do not log passwords; the caller supplies them at runtime. Strings cannot be reliably erased. Browser/WASI AES support is unavailable; this desktop/server example excludes both. Directory extraction can leave partial output on failure or cancellation: use a fresh staging directory, accept only after complete success, and clean failures. OverwriteFiles=false rejects existing files; it does not make extraction atomic. No interoperation with a particular external archive application is claimed.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

All four algorithms restored the original payload; wrong and missing passwords failed; each AES authentication tag was also damaged and rejected. Actual asynchronous AES-256 directory creation/extraction, overwrite refusal and cancelled extraction were executed.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
