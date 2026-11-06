# Author typed Apple app manifest values

## Goal

Add, override or remove app manifest values by build configuration when one shared Info.plist should remain unchanged.

## Newer approach

Oystercatcher's survey app keeps its base identity in Info.plist. A distribution configuration needs a different display name, an offline Boolean and a list of survey regions; an obsolete private key must disappear. Use AppManifestEntry items for these configuration decisions. They are processed after the main manifest, partial manifests and generated values, so they intentionally win. Do not use this precedence to remove mandatory platform keys.

Type is significant: Boolean accepts true/false without case sensitivity; String retains text; StringArray splits on semicolons unless ArraySeparator is supplied. Remove takes no Value. An MSBuild Remove attribute would remove an item from the item collection, not delete a plist key. Avoid duplicate conflicting entries rather than relying on ordering.

{{snippet:net11-oystercatchermanifest-csproj}}

## Older approach

The .NET10 comparison uses a valid PartialAppManifest overlay for additions and overrides. To remove OystercatcherLegacy, remove it from the base plist (or select a complete configuration-specific manifest) before building. Partial dictionaries merge rather than expressing deletion. The retained base deliberately demonstrates the difference; do not claim the older overlay removes that key. Keeping complete plists is still useful when an entire manifest varies. No older-target catalogue route is advertised.

## Migration

1. Identify keys that are product identity versus configuration overrides.
2. Move only the latter into conditional AppManifestEntry items. Keep each key unique after evaluation.
3. Use typed values; omit Value for Type=Remove. Choose a separator absent from array elements.
4. Build with the matching native workload. Inspect the generated Info.plist, then the signed app manifest, checking both value and plist type.
5. Expect Oystercatcher Surveys, true, [Marsh, Estuary], and no OystercatcherLegacy in the current output. The focused real task check observes these values; a signed native app was not produced.

## Canonical samples

### net10-directory-build-props

{{snippet:net10-directory-build-props}}

### net10-info-plist

{{snippet:net10-info-plist}}

### net10-marker-cs

{{snippet:net10-marker-cs}}

### net10-oystercatchermanifest-csproj

{{snippet:net10-oystercatchermanifest-csproj}}

### net10-surveyoverrides-plist

{{snippet:net10-surveyoverrides-plist}}

### net11-directory-build-props

{{snippet:net11-directory-build-props}}

### net11-info-plist

{{snippet:net11-info-plist}}

### net11-marker-cs

{{snippet:net11-marker-cs}}

### net11-oystercatchermanifest-csproj

{{snippet:net11-oystercatchermanifest-csproj}}

## Constraints

Applicability lists all runtime, SDK, language, workload, artifact, provider, platform, rendering and stability constraints. These are .NET11 Apple workload features, with stable API labels and RC preparation evidence; neither actual GA nor a complete RC3 Apple composition was tested. No preview language syntax or experimental opt-in is used. .NET10 comparison compilation is separate from current task/reference checks.

## Evidence

Run eng/Test-AppleAuthoring.ps1 for pinned-source/artifact checks, real typed plist and stripping task output, actual binding generation and generated-source/consumer reference compilation. The source manifest records exact SHA-256 identities. SDK11 RC3 runs RC2 Apple tasks and bgen on Windows; that does not establish native linking, signing or runtime support. Source-review receipts retain sample/input freshness; native exclusions apply to the named examples under PIN-699. No Human action is pending. Packed stdio list/get establishes offline guidance delivery only. The eight .NET10 foundations and the comprehensive/GA gates remain unchanged.
