# Convert international domain labels into caller buffers

## Goal

A fictional orchard routing validator exchanges Unicode and ASCII domain labels.
Write conversions into caller-owned buffers and keep conversion policy separate
from permission to route to a domain. No DNS lookup is needed.

## Older approach

.NET 10 IdnMapping string methods provide the legitimate conversion. This original
adapter copies that result into the application's destination:

{{snippet:older}}

It returns false/zero when the destination is too small, after making the converted
string. Invalid labels still throw. This route remains valid on .NET 10.

## Newer approach

.NET 11 TryGetAscii/TryGetUnicode accept spans and write directly to the caller's
destination, removing this intermediate string/copy:

{{snippet:newer}}

Include the complete helper in its own source file with ImplicitUsings enabled.
Create an IdnMapping with UseStd3AsciiRules=true and AllowUnassigned=false for the
tested strict policy. Pass toAscii=true to encode or false to decode, and consume
only destination[..written] after true. Use separate, non-overlapping source and
destination storage. Neither Try method turns malformed input into false: invalid
labels and overlapping buffers throw ArgumentException. False means insufficient
space with written=0; do not consume any destination contents after failure, which
may have been modified. Choose a bounded retry policy if the application needs one.

The measured round trips include `mélange` → `xn--mlange-bva`, `bücher` →
`xn--bcher-kva`, and `東京` → `xn--1lqs71d`. Exact output-size buffers succeed;
empty and one-character-short buffers return false/zero in both directions.
This does not promise every Unicode input round-trips unchanged: IDNA processing
can normalize/map text and platform data governs acceptance.

## Migration

1. Keep GetAscii/GetUnicode on net10.0. Use net11.0 for the new span methods;
   no compatibility package or additional .NET 10 foundation is advertised.
2. Include the current helper. Allocate destination storage under the caller's
   capacity policy; span lengths/counts are UTF-16 chars, not byte counts.
3. Select UseStd3AsciiRules and AllowUnassigned explicitly. With strict STD3, the
   tested underscore label is rejected; relaxed STD3 accepts `orchard_route`.
4. Check the boolean and written count before consuming output. Catch invalid-input
   ArgumentException separately from insufficient capacity. Never overlap buffers.
5. Re-run representative labels under the deployment's actual globalization mode
   and data version. Culture-invariant comparisons do not prove identical IDNA policy.
6. Apply separate authorization, spoofing/confusable and routing rules. Successful
   IDN conversion neither establishes ownership nor makes a domain trustworthy.

## Constraints

Primary route: net11.0/.NET 11, canonical C#15; older comparison net10.0/C#14.
The evidenced platform is Windows x64 with non-invariant system ICU, explicitly
configured by runtime host options System.Globalization.Invariant=false and
System.Globalization.UseNls=false in both sample projects. These options are
runtimeconfig properties (normally supplied by project RuntimeHostConfigurationOption
items), not changes to the current culture. No package, provider, workload,
rendering mode, network service or experimental opt-in is required.

Measured native ICU is C:\Windows\System32\icu.dll version72.1.0.4, with its SHA256
and sort-data identity saved in output. NLS, invariant mode, Linux/macOS and other
ICU/data versions are not executed routes here. Their IDNA versions, mapping and
rejection rules can differ. In the selected ICU, U+0378 in a label throws with
AllowUnassigned=false and true; setting true is not a universal acceptance promise.
The test records both outcomes rather than assuming a legacy option overrides ICU.
No allocation benchmark or zero-allocation implementation guarantee is made.

## Evidence

`eng/Test-TextBoundaries.ps1 -SampleNames OrchardRoutes -Record -VerifySources`
builds/runs both real consumers, capturing round trips, exact/short/empty buffers,
empty/malformed/overlong labels, lone surrogate, NUL, STD3 choices and both
AllowUnassigned settings. Current calls also reject overlapping spans. Actual
loaded ICU, native hash, runtime configuration and sort-data identities are recorded.

SDK10.0.402/ref-runtime10.0.13 and SDK11.0.100-rc.2.26504.105/ref-runtime11RC2
are the exact reproduction pins, distinct from supported release prerequisites.
No resolved NuGet dependencies. Compiler/CoreLib and audit-matching references,
commands and fresh inputs use the existing evidence workflow. Pinned official
sources qualify the platform branches and callable methods. Installed local-feed
stdio retrieval verifies full canonical Markdown, ID/title/goal, target11/edition11,
applicability and fresh paired evidence offline. RC2 does not establish GA;
PIN-959/PIN-962 and Human gates remain open.
