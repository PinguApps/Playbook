# Recognise embedded URI payloads and describe video media types

## Goal

Describe a fictional harbour training clip by recognising an embedded data URI and assigning its separately supplied video format a media-type label. Scheme recognition and a content label do not establish payload validity.

Use an SDK console project with implicit usings and nullable enabled. Compare net10.0 / C# 14 with net11.0 / C# 15, put declarations in namespace Playbook.Samples and import System.Net.Mime. Both examples are local managed code; no network access or extra package is needed.

## Older approach

The .NET 10 implementation uses standard URI parsing plus explicit scheme/media-type strings. These identifiers are valid and remain useful when the target has no named constant.

{{snippet:older}}

The application supplies format separately from address. For example, Describe("data:video/mp4;base64,AA==", "mp4") returns Embedded=true and DeclaredMediaType="video/mp4". This tiny example does not decode the payload. Passing an HTTPS address reports Embedded=false without fetching it.

## Newer approach

.NET 11 supplies Uri.UriSchemeData and five MediaTypeNames.Video constants for the same result:

{{snippet:newer}}

The supported format map is mp4 → video/mp4, mpeg → video/mpeg, ogg → video/ogg, mov → video/quicktime and webm → video/webm. Relative addresses and unknown format labels are rejected. URI schemes are compared using ordinal case-insensitive equality. No file-extension guess or media sniffing is performed.

## Migration

1. Keep URI syntax checking and the application's allowed schemes/formats distinct. Uri.TryCreate with Absolute rejects relative addresses; this sample intentionally describes any absolute URI rather than declaring it safe to open.
2. Replace repeated "data" and video media-type strings with framework constants when targeting net11.0. The constants remove duplication; they do not change URI parsing or transport behavior.
3. Decide separately how to validate a data URI's header, media declaration, encoding and payload size before decoding it. A recognised scheme is not proof of a valid data URI payload.
4. Validate the actual bytes and choose an application-specific allowlist before storage, rendering or playback. The separately provided format can disagree with an embedded media declaration. This sample reports that supplied label and makes no assertion about the bytes.
5. Use the returned content label only where the application owns the metadata contract. Browser/codec support, HTML sanitization, remote fetching and HTTP endpoint implementation are separate concerns.

## Constraints

This single goal consolidates the provisional URI/MIME groupings: uri.handle-schemes, mime.describe-content and mime.describe-video-content are not additional recipes. The retained concrete ID is uri.identify-data-uris. No duplicate content or general URI/media subsystem is introduced.

## Evidence

Actual net10.0 and net11.0 ContentDescriptions projects compile and execute checks for all five media types, uppercase DATA, HTTPS, invalid relative input and unknown formats. A malformed data payload is deliberately still recognised by its scheme, proving this is not payload validation. No network request, media decoding, safety check or playback was executed or claimed.

The newer APIs are framework constants, with no external package/assets, workload, provider, rendering mode or experimental opt-in. There is no older-target route using newer framework APIs. Exact SDK/runtime/language artifacts and input hashes are saved with the samples; .NET 11 RC2 evidence does not establish GA verification.
