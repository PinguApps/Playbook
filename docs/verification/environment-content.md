# Environment content evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-795.md).

## Scope and observations

Both complete Host/Client pairs compile with their actual matching SDKs:

| Consumer | SDK / language | Runtime and application packages | Execution |
| --- | --- | --- | --- |
| .NET 10 Host + Client | 10.0.402 / C# 14 | 10.0.13 | Actual static SSR and Chrome WebAssembly |
| .NET 11 Host + Client | 11.0.100-rc.2.26504.105 / C# 15 | 11.0.0-rc.2.26504.105 | Actual static SSR and Chrome WebAssembly |

These are interpreted browser consumers with `WasmBuildNative=false`. Their
structured `workloads: []` fields declare no required workloads; this task
installed none. Both recorded hosts already had `wasm-tools` and mobile
workloads installed, as shown in the raw `workloadInfo` inventories. This
is not evidence from an empty-workload host. The proof is compilation and real
execution, not cross-compilation, HtmlRenderer, a browser-environment DI stub
or a mock.
The API has no Experimental opt-in; the framework artifacts are **RC2, not GA**.

Real server responses in Production contain identity, receipts, desk help and
contact. Review responses contain identity, rehearsal notice, rehearsal notes,
desk help and contact. The same results were checked in real browser rendering
for `rEvIeW` and Production. The overlapping excluded notice is absent in both.
Whitespace and multiple comma-separated names, discarded empty entries,
case-insensitive include/exclude matching, exclusion precedence, unspecified
Include, whitespace-only Include and comma-only Include are exercised.

The host deliberately stays Production while its browser boots as Review.
`WebApplication` supplies the server `IHostEnvironment`; the actual RC2
`WebAssemblyHostBuilder` registers the browser environment adapter. The new
Chrome component contains EnvironmentView decisions, without environment
injection or manual conditionals. Its wrappers inject environment services only
to label the observed renderer/environment in diagnostic attributes.

**Hidden chrome is not authorization.** The browser startup switch and assets are
public and user-modifiable. Protect every sensitive operation and data access on
the server. This ticket adds no authentication feature, account, resource,
deployment or package publication.

The empty/whitespace *current environment* rule is explicitly **source review**:
only unspecified/whitespace Include AND Exclude render in that case. It is not
claimed as configured-host or browser execution. Auto, Hybrid, AOT, other browser/
OS combinations and GA artifacts were not executed. None is a required Human
prerequisite for this bounded local capability.

## Sources, inputs and artifacts

- The [recipe metadata](../../content/recipes/blazor.show-environment-specific-content/11.0/recipe.json)
  retains official references and explicit runtime, SDK, language, package,
  asset, provider, platform, rendering, prerequisite, constraint and stability
  requirements. The [full prose](../../content/recipes/blazor.show-environment-specific-content/11.0/recipe.md)
  expands canonical source regions into complete offline implementations.
- The [source-disposition register](../planning/audits/blazor.dispositions.json)
  preserves G12 / WEB-012 and the stable goal/edition identity. The exact RC2
  [EnvironmentView source](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/EnvironmentView.cs)
  and [browser adapter registration](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly/src/Hosting/WebAssemblyHostBuilder.cs)
  support the contract. Worked examples and prose are original.
- Four `verification-evidence.json` records beside the actual sample projects
  record matching-SDK compilation, resolved dependency closure, workloads,
  language, command, output and exact evidence inputs. They are compile records;
  browser/server execution is recorded separately.
- [Canonical execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-795.runtime.evidence.json) records four real SSR
  responses and four real browser cases, source/input hashes, executable and
  static-asset manifests, actual native runtime and WebCIL bytes. Downloaded
  EnvironmentView, WebAssembly adapter, client and startup bytes match the
  canonical artifacts. Runtime output is .NET 10.0.13 / .NET 11 RC2 respectively.
- Canonical capture used normalized LF inputs at dirty source
  `a418cab9ee99e79030a9fd72c6cebf5aa6fe1611`. It is never relabelled as execution
  of a later commit. Browser verifier source and evaluated expression each hash
  to `d27b3e7352fe7724ccd8097f6ff8c0ba726af7e903f17b1782e16dbbe46f55a5`;
  the exact UTF-8 source, including its comment/newline, was evaluated without
  transformation. The HTTP verifier identity is recorded separately.
- [Integration equality](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-795.integration.evidence.json) proves EVERY own
  evidence input and executed artifact still equals that actual capture after
  integrating main `854f74cefd06af3126d1f4c09a3cace8e18bf361` at
  `8c9045966e010998e216cba44a0049047abce1ca`. Complete CultureHarbour, EstuaryBoard and
  Copperline methods/resources/steps, all earlier entries, docs and dispositions
  remain. Unchanged canonical behavior was reused without an unnecessary rerun.
  Independently removing the exact own EnvironmentChrome method from the merged
  steps file reproduces the ENTIRE main file, including complete CultureHandover
  and Estuary methods. CultureHarbour Host/Client/raw-resx, guidance, proof files,
  assets and inventory match main. No method, assertion or earlier entry was lost.
- [Inspector proof](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-795.inspector.evidence.json) records the literal block
  below, its output and server hashes, and a 19,425-character complete Markdown
  body: SHA-256 `73cf75a70d63f9ab5a57ede67d6af3b518db2e0556edc67fef9fd44e084bec21`.
  The rebuilt server DLL is byte-identical to the actual packed server:
  `36b79c47c38d26aec580acf692edc6ff7cc30691ce24e32a2a51760410dcec99`.
- [Standalone proof](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-795.standalone.evidence.json) reconstructs both complete
  release pairs solely from that real stdio Markdown's 18 code fences and shared
  file/SDK instructions. No checkout source, parent imports or lock files are
  copied. Both pairs restore/build; their actual server routes and all four real
  Chrome environment cases pass. Downloaded assets match the reconstruction,
  and every captured source/bin artifact remains unchanged after execution.
  This earlier standalone execution is retained with its original provenance;
  the r3 returned body, verifier and all 940 source/bin files still match it.
  [Test-CopperlineDeskStandalone.ps1](../../eng/Test-CopperlineDeskStandalone.ps1)
  is the exact recorded reconstruction verifier. Its identity is separate from
  the unchanged real-browser verifier above.
