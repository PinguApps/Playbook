# Share helpers on an interface without adding implementation requirements

## Goal

Keep a helper beside the contract it serves when all callers should use the same implementation. Use a non-virtual static method with a body, called on the declaring interface. Implementing classes supply the instance contract only. The original example gives a planetarium observation a station label: trim and uppercase the station, then append a nonnegative sequence with at least three digits, using invariant formatting.

## Older approach

A .NET 10 / C# 14 library can keep label generation in a companion static class. This is a good choice when the utility serves several contracts or should evolve separately. Create a console project targeting net10.0 with LangVersion 14.0. Include `using System.Globalization;` and use namespace `Playbook.Samples` for both snippets:

{{snippet:older}}
{{snippet:older-consumer}}

The interface-helper version below is also legal on .NET 10 with C# 14. These are alternative organizations of the same behavior, not evidence that .NET 10 requires the companion class. The verification compiles and executes that same interface surface on .NET 10 as well.

## Newer approach

For this .NET 11 / C# 15 edition, move the implementation onto its owning interface. Create a console project targeting net11.0 with LangVersion 15.0. Use the same import and namespace:

{{snippet:newer}}
{{snippet:newer-consumer}}

`ObservationReport.Label(new RoofSensor(), 7)` returns `ROOF-007`. `IObservationSource.CreateLabel(" dome ", 42)` returns `DOME-042`. Blank stations throw ArgumentException; negative sequences throw ArgumentOutOfRangeException. The private helper is an implementation detail; the public helper is a callable library API. Neither becomes an obligation for RoofSensor.

C# 15's release change is narrower than the title “static interface members” might suggest: it removes the default-interface-method runtime-support gate for non-virtual static members. Static interface helper bodies already existed in earlier C#. The canonical RC1 notes list the change as stabilized, while Roslyn PR 83097 and the language-design notes explain its older-runtime purpose. This edition demonstrates net11.0 and compares net10.0; it does not offer or verify a .NET Framework/netstandard route. Do not infer such a route from the release headline or these executions.

## Migration

1. Choose an interface that owns the helper's meaning. Keep an independent utility class if the behavior spans unrelated contracts.
2. Move the method body and its private dependencies onto the interface. Use `public static` for callers and `private static` for internal factoring. Keep input validation and formatting unchanged.
3. Update callers to `IObservationSource.CreateLabel(...)`. Do not call through an instance, an implementing class, or a constrained type parameter. Static helpers are not inherited by implementing classes.
4. Leave implementers responsible for `Station` only. Do not add static abstract or static virtual modifiers to share this fixed implementation.
5. Compile downstream callers and run the original label checks. If the companion utility was public, keep a forwarding entry point for existing consumers or treat removing it as a breaking API change. Moving a method does not redirect previously compiled callers.

## Constraints

An ordinary static method is non-virtual and needs its body. It runs the interface's implementation; a same-named method on a class does not override it. A private method can be used inside the declaring interface and cannot be called by external consumers. Interface static members do not add instance storage.

Static abstract members require an implementation supplied by the implementing type. Static virtual members provide a default implementation that participating types can replace. Both are dispatch contracts called through a constrained type parameter such as `T.Member`, rather than directly on the interface name. They solve a different problem from this fixed helper. An explicit implementation of this non-virtual helper is rejected.

The declared route targets .NET 11 with SDK 11 and C# 15. It uses only framework APIs: no additional packages or package assets, provider, rendering mode, experimental opt-in or optional workload. Any .NET 11-supported platform can use the portable pattern; the recorded sample executions were on Windows. No UI or rendering behavior is claimed. Only RC2 artifacts were verified, not GA. A .NET 10 comparison is not an extra foundation catalogue entry. No legacy runtime or newer-SDK/older-target route is advertised; runtimes lacking default interface methods have separate accessibility/runtime restrictions that these samples do not test.

## Evidence

The actual net10.0/C# 14 and net11.0/C# 15 projects compile and execute the same label and invalid-input rules. The newer execution checks that the public method is static, neither abstract nor virtual. eng/Test-StaticInterfaceHelpers.ps1 builds and runs a separate consumer assembly implementing only Station, then requires compiler rejection of private access (CS0117), class access (CS0117), instance access (CS0176), explicit helper implementation (CS0539), direct static virtual access (CS8926), and an unsatisfied static abstract contract (CS0535). It also compiles and executes the canonical interface source with .NET 10 / C# 14 to verify the historical distinction.

Saved sample records identify exact SDKs, runtimes, commands and hashed inputs, including the compiler-check script. The consumer checks run separately in eng/Verify.ps1; their output is not part of the sample execution record. Source review establishes the C# 15 runtime-gate change; no legacy-runtime compilation or execution is claimed. No mocks, cross-compilation or targeted Human proof are used for the declared net11.0 route.
