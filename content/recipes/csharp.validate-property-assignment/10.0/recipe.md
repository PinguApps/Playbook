# Normalize and validate properties with the C# field keyword

## Goal

Normalize or validate incoming values in a property setter without maintaining a separate backing-field member. C# 14's `field` keyword accesses compiler-managed storage while preserving the validation rules of an explicit-field implementation.

The worked example uses a warehouse import that supplies bay codes with surrounding whitespace and mixed casing. Store the trimmed, invariant uppercase value. Reject null, blank and codes longer than 12 characters, retaining the previous code after a rejected update. Constructor input follows the same rules.

## Older approach: .NET 9 / C# 13

An explicit backing field is a valid way to keep validation in one setter. It remains useful when other methods genuinely need direct access to storage.

{{snippet:older}}

## Newer approach: .NET 10 / C# 14

The contextual `field` keyword accesses compiler-created storage inside the property. The validation and normalization are unchanged; the manually maintained `_code` member disappears.

{{snippet:newer}}

## Migration

1. Target `net10.0` with the .NET 10 SDK and C# 14. No preview mode, extra packages or workloads are needed.
2. Replace the getter with `get;`, replace `_code = normalized` with `field = normalized`, then remove `_code` only after checking all its references.
3. Keep the constructor assignment through `Code`. Copying an unvalidated constructor argument into an initializer changes the behavior.
4. Run the same assignment checks before and after migration: `" swan-7 "` becomes `"SWAN-7"`; invalid updates throw and leave the previous code intact.

## Constraints and initialization

`Code = normalized` inside the setter calls the setter again, eventually overflowing the stack; `field = normalized` writes storage directly. A property initializer also writes storage directly and does **not** run the setter. Here `string.Empty` only establishes storage before construction; every successful public construction validates through `Code = code`.

The keyword is contextual inside an accessor. If existing accessor code uses a member named `field`, qualify it (`this.field`) or escape the identifier (`@field`) deliberately. Do not mechanically rename references and change their meaning.

The getter and setter retain the same external contract as the explicit-field version. This simplification does not add thread safety, persistence validation or validation for callers bypassing the property. The .NET 9 comparison is comparison evidence, not a supported .NET 9 catalogue target.

## Conditional .NET 11 SDK route for .NET 10

A newer SDK can build an older target. The optional .NET 11 SDK route keeps `TargetFramework=net10.0` and `LangVersion=14.0`, using SDK `11.0.100-rc.2.26504.105` with the .NET 10 targeting pack/runtime. It compiles the same canonical source and runs the same checks. Select this route only when those requirements are available; Playbook has not inspected them. The .NET 10 SDK route remains valid. This does not claim supported C# 15 syntax on .NET 10, or GA verification from an RC build.

## Evidence and alternatives

Both original projects use independent SDK pins and compile against their actual target frameworks. Their shared executable checks cover construction, normalization, null/blank/length rejection and preservation of the previous value. Run `eng/Verify.ps1` to refresh this evidence. Compilation and these local execution checks establish only the demonstrated behavior, not application integration or concurrent safety.

Keep the explicit backing field when storage must be accessed outside the property. Prefer an immutable value type when a normalized code needs stronger identity or cannot change after creation. Official references and exact sample baselines are included in this response's metadata; no external page is required to use the example.
