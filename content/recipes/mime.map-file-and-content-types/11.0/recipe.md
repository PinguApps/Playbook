# Map file extensions and content types

## Goal

Find a known content type for a filename/extension, or one extension for a media
type. Fictional Mossbank labels parcel attachments without maintaining a broad
mapping table. Use this for descriptive metadata; it does not inspect file bytes,
validate an upload or decide whether content is safe to open or serve.

## Older approach

.NET 10 has no System.Net.Mime.MediaTypeMap framework API. A small curated map is
a valid application policy when the accepted formats are deliberately limited.
This original example supports PDF, JSON, JPEG and gzip, with an explicit .jpg
reverse preference. It is not an exhaustive copy of the framework's table.
Copy the snippet into a nullable/implicit-usings console project; call the
internal AttachmentLabels methods from the same assembly.

{{snippet:older}}

This net10.0/C#14 consumer requires no package. A maintained framework-specific
or third-party mapper is another choice; its dependency and mapping policy need
their own verification. Do not treat System.Web APIs as a .NET 10 framework route.

## Newer approach

.NET 11 supplies System.Net.Mime.MediaTypeMap in the System.Net.Mail framework
assembly. Delegate the same application calls to its string/span overloads.

{{snippet:newer}}

For example, MediaType("/mossbank/parcel-48.PDF") returns application/pdf;
Extension(" APPLICATION/PDF ; revision=3 ") returns .pdf. Pass an existing
ReadOnlySpan<char> slice directly when the caller already has one. The result
is a nullable string; no unsafe caller code is required. No zero-allocation or
throughput claim is made.

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime. The canonical consumer declares
   C#15.0; no package, optional workload, provider, rendering mode, unsafe code or
   experimental opt-in is required. There is no claimed .NET 10 package backport.
   Keep a curated map on older targets. This adds no .NET 10 foundation.
2. Pass a path or extension, with or without its leading dot, to
   AttachmentLabels.MediaType (which calls MediaTypeMap.GetMediaType on .NET 11).
   Comparison is case-insensitive. The framework follows the host's
   Path.GetExtension rules; it does not open the file. A dotted directory is not
   the file's extension. Windows backslashes were tested on Windows only.
3. Handle null results explicitly for unknown or empty input. The string
   overloads reject null with ArgumentNullException; a default/empty span
   produces null. Do not assume every filename, extension or registered type is
   represented. Preserve any application-specific overrides deliberately.
4. Pass a media type to GetExtension for one known suffix including its dot.
   Lookup ignores case, trims surrounding whitespace and ignores parameters
   after the first semicolon. This is a lookup helper, not a complete MIME
   grammar validator or wildcard negotiator; unknown types return null.
5. Do not round-trip the original filename through the reverse map. Both .jpeg
   and .jpg map to image/jpeg; the tested reverse choice is .jpg. More than one
   suffix/type can be plausible. Preserve the original name separately when
   required, and test any exact suffix choice your application depends on.
6. Verify behavior beyond the old curated set before changing an allowlist.
   The framework's broader descriptive mapping must not silently expand accepted
   upload formats. Mapping data and ambiguous preferences can evolve between
   releases; this recipe records observed RC2 cases, not a frozen universal table.

## Constraints and alternatives

Only the final extension is used: bundle.tar.gz maps by .gz, not a compound
archive detector. A bare name equal to a suffix, such as pdf, is treated as an
extension and labelled; keep filename and extension-only inputs distinct in your
application. Do not pass a whole URL: parcel.pdf?revision=3 is not a PDF path
lookup. Parse the URI and select the intended path in a separate, explicit step.
Extension input is not whitespace-normalized: " pdf " is unknown in the tested
contract. File existence, contents, path traversal, filename legality, malware,
authorization, storage isolation and HTTP serving policy remain separate checks.

A misleading .pdf suffix does not turn arbitrary bytes into a PDF. Keep upload
size limits, application format validation and safe serving policy independently.
Use a curated override when business rules require a fixed mapping; keep unknown
handling intentional instead of assuming a permissive fallback makes a file safe.
No network or registry lookup occurs in these demonstrated framework table calls.

## Evidence

eng/Test-AttachmentFacts.ps1 -Record -VerifySources compiles and runs
AttachmentLabels with SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105. Both actual consumers use
only framework dependencies, zero warnings/errors and no workloads. Saved records
contain exact commands/input hashes, compiler/CoreLib identities and
System.Runtime/System.Net.Mail references matched to the source audit.
The bounded attachment-facts manifest pins both reference contracts and the
current official lookup implementation; source presence is not execution proof.

The paired suite executes string and span paths/extensions, dot/no-dot casing,
unknown/empty/null/default inputs, dotted directories, Windows paths, final gzip
suffix, URL-query and extension-whitespace boundaries. Reverse lookups execute
case/whitespace/parameter handling, unknown/wildcard results and the .jpeg/.jpg
many-to-one case. The old curated map solves these same bounded inputs; no claim
of complete old/new table equivalence is made.

The ordinary installed package is inspected offline through real stdio list/get
using an isolated local feed/cache from an unrelated directory. Discovery,
complete canonical Markdown, target11/edition11 applicability, explicit target10
rejection and fresh paired evidence are checked. Windows x64 RC2 was executed;
other platform path rules and final-GA mapping stability are unverified. The API
is not marked experimental in the audited contract. PIN-959/PIN-962, other Human
gates and the exact eight .NET 10 foundations retain their existing obligations.
