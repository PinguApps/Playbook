# Handle absent results and errors from native bindings

## Goal

Handle missing native callback results and errors when corrected Apple binding annotations expose nullable values.

## Newer approach

Whimbrel reads coastal trail markers through CoreNFC. An active, connected INFCNdefTag calls ReadNdef with nullable NFCNdefMessage and NSError values. Check error first, then the absent message, then consume the successful message. If both arrive, the example reports the error; if neither arrives, it reports an indeterminate read and suggests retry within an active session. It never dereferences error simply because a message is absent.

NFCQueryNdefStatusCompletionHandler likewise permits an absent NSError. Error absence alone does not promise a useful status or message. Preserve callback/session lifetime, dispatch UI updates to the main thread and avoid retaining native wrappers beyond their supported lifetime. report is a presentation callback; session creation, entitlement configuration and tag connection are application responsibilities outside this bounded consumer library.

The related AppKit source-contract family includes nullable window lookups, main/key/modal windows and current/next events. Check for null before using any returned wrapper; an absent window/event is a normal state, not proof of an exception or a replacement API. These AppKit APIs require a separate macOS workload; the iOS consumer does not make AppKit available on iOS.

{{snippet:net11-tagreader}}

## Older approach

Older annotations may imply a result/error is non-null even when native code can omit it. The same defensive callback logic is valid in the older library and compiles against the installed .NET10 iOS references. Annotation corrections do not remove ReadNdef or guarantee a non-null pair. This consumer migration is separate from failable Objective-C initializer factory generation (apple.bind-failable-initializers).

{{snippet:net10-tagreader}}

## Migration

1. Inspect the real old/current managed bindings and native availability for each callback or lookup used by the application.
2. Replace unconditional message/error dereferences with explicit error, absent-result and success branches. Define the both-present policy deliberately.
3. Preserve native session and callback lifetimes. Check CoreNFC capability and entitlements before starting a session; a library compilation is not a complete NFC app.
4. On an available matched Mac/Xcode/device route, exercise success, error, neither-present and callback delivery/lifetime. Here native linking, signing, NFC/device execution and current native compilation are not-tested under PIN-699.
5. Review related AppKit lookups separately. Do not invent replacements for retained enums or infer native support from HMError.QuotaExceeded's presence. Factory generation retains its separate recipe.

## Canonical samples

{{snippet:net10-tagreader}}

{{snippet:net11-tagreader}}

## Constraints

Applicability records runtime, SDK, C# version, packages, workloads, provider, platform, rendering and stability conditions. GA published composition and GA verification compiler are separate baselines, not a supported combined native installation. No experimental feature opt-in is required for these APIs. Comparisons retain the eight .NET10 foundations. Native exclusions do not establish compatibility or GA readiness.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Run eng/Test-WhimbrelGuidance.ps1 for older Android selected compile/runtime assets and actual old/current MAUI engine checks. Older Apple library compilation is recorded separately. Pinned mobile source/reference review uses eng/Inspect-MobileApiGaps.ps1. Native exclusions apply only to the named samples and conditions; no mock is labelled device evidence. Installed stdio list/get proves offline delivery, not native behavior. PIN-959 and PIN-966 remain open for their own qualifying coverage/GA criteria.

## References

- [Official appkit.cs](https://raw.githubusercontent.com/dotnet/macios/34b3b25d471c22f2a62c36ec0971b021d0a0e000/src/appkit.cs)
- [Official corenfc.cs](https://raw.githubusercontent.com/dotnet/macios/34b3b25d471c22f2a62c36ec0971b021d0a0e000/src/corenfc.cs)
