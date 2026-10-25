# Component interop evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-801-803.md).

## Actual artifacts and behavior

The old consumer is net10.0 / SDK **10.0.402** / C# **14.0** /
ASP.NET Core and runtime **10.0.13**. The new consumer is net11.0 /
SDK **11.0.100-rc.2.26504.105** / C# **15.0** /
ASP.NET Core and runtime **11.0.0-rc.2.26504.105**.
Both use the matching implicit Microsoft.AspNetCore.App.Internal.Assets package.
No external package/provider/workload/account is needed.

[Canonical hosts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-801-803/canonical-hosts.evidence.json) retain actual
DLL and served framework hashes and loaded coreclr/hostfxr paths.
Both executed DLLs remain byte-identical after LF/lock-file normalization and
the final evidence builds:

- .NET 10 DLL: `98da5cf3b34b9ea18589e456ae6e1f6c8f45e116bd0d1a32ed48ea7452875ebe`.
- .NET 11 DLL: `5c542fdf43d7f307f592993a6544f7a9eb37da02c8c367557d4d9c0ebec0d539`.

[Compact browser observations](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-801-803/browser.evidence.json) retain the
actual read-only capture function, four consumer/circuit identities and the
settled before/click/failure results. Actual Windows Chrome extension clicks,
followed by waits for visible completion text, established:

1. Both consumers display West breakwater and keyed scale 3, with a stable
   circuit-owned session through updates. The new renderer invokes the registered
   property policy, including inherited public injected properties and IJSRuntime.
2. The working formula changes from d=2×3 to d=3×3. All seven root/descendant
   elements remain in the MathML namespace. The old direct-markup control,
   dynamically introduced after startup, has the HTML namespace.
3. The real module produces West breakwater: 3 and one successful invocation.
   A failing void call displays Announcement rejected while preserving the
   successful output and count. Module import keeps its meaningful generic result.

[Missing-key proof](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-801-803/missing-key.evidence.json) uses the actual
new host with `--missing-key true`. A real circuit invokes StationPropertyPolicy
and fails with the required keyed-service exception for Scale. Development-only
DetailedErrors/debug logging made the exact failure visible; no sample source
was changed for this check.

The policy owns a deliberately closed application component set, not a general
container replacement. It explicitly assigns StationPanel's properties and
rejects other injected component types. Non-injected framework boundaries receive
a no-op. Adding an injected property to the known component requires updating the
mapping. No claim is made for general concurrent activation, Hot Reload, trimming,
NativeAOT, third-party containers or untested rendering modes.

Compilation records preserve source commit
`eb8d9f21c72180801cd11b2b2a27ea201ba9633e` and **sourceDirty=true**.
They are not relabelled as clean execution of a later commit. Recorded input
hashes establish freshness. Compilation, official review and browser execution
remain separate; no mocks or cross-compilation are represented as execution.
