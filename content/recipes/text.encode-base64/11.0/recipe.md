# Encode binary data directly into Base64 buffers

## Goal

A beacon frame contains arbitrary bytes, including zero and high-bit values. Use standard padded Base64 for a text exchange rather than interpreting those bytes as text.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Buffers.Text, System.Buffers and System.Formats.Cbor; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

Convert.ToBase64String/FromBase64String already provide the character round trip. The older UTF-8 route allocates the maximum destination, invokes the existing status API and slices to bytesWritten. These are valid .NET 10 routes.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

The new Base64 convenience APIs return strings/character arrays/UTF-8 arrays directly, or write to supplied destinations. The same original payload encodes to AP9+gCE= and returns unchanged from both forms. Import System.Buffers.Text.

{{snippet:newer}}

## Migration

1. Keep the chosen alphabet/padding in the wire contract. Replace Convert/adaptation only when the new input/output representation helps. Check operation status, consumed and written counts for partial buffers, and mark the final block correctly.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

This is standard Base64, not Base64Url: alphabet and padding differ. Use GetEncodedLength/GetMaxDecodedLength for destination planning, and consume only the written portion. The OperationStatus forms preserve consumed/written and isFinalBlock semantics; retain unconsumed tails across blocks. The test proves a non-final two-byte tail returns NeedMoreData with zero consumption. In the actual RC2 artifact TryDecodeFromChars throws FormatException for malformed text; Try here is not a blanket promise of nonthrowing validation. Use the OperationStatus overload for InvalidData. A short destination is a separate condition. Encoding does not encrypt, authenticate or compress the payload.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original BinaryData projects compile and run against their declared .NET 10/11 SDKs and locked matching CBOR packages. Checks round-trip the original payload through characters and UTF-8, verify short destinations and non-final Base64 tails, exercise every CBOR split plus one-byte chunks, reject a truncated final token and reject trailing roots. The RC2 malformed-Try exception is explicitly checked.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
