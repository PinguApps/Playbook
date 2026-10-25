# Process platforms evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-721-725.md).

## Measured contracts and limits

- SDK/runtime baselines: .NET 10 SDK **10.0.402**; .NET 11 SDK/runtime **11.0.100-rc.2.26504.105 / 11.0.0-rc.2.26504.105**. CBOR locks use **10.0.13 lib/net10.0** and **11.0.0-rc.2.26504.105 lib/net11.0**. Exact pins are test baselines, distinct from release-family consumer requirements.
- Windows x64 executes all eight canonical comparison projects. Separate Linux x64 execution compiles the same newer process inputs inside the existing digest-pinned SDK image, with networking disabled and an init process for orphan reaping. It preserves exact source hashes and host/SDK/output in `linux.evidence.json`. WSL initially compiled the project but then returned a VM connection timeout; it supplies no successful final Linux proof. No other service was provisioned.
- `RunAndCaptureTextAsync(ProcessStartInfo)` requires both redirection flags. Its direct-child cancellation policy differs from the older `Kill(entireProcessTree:true)` wrapper; cancellation can throw while reading or report a cancelled exit status while waiting. The test waits boundedly for OS termination rather than assuming Kill is synchronous.
- RC2 permits parent-bound lifetime on Windows/Linux/Android; this edition verifies Windows/Linux and does not advertise Android. macOS has independent/detached support but **no KillOnParentExit route**. This resolves the inventory’s Windows-only headline conflict without inventing universal support.
- Suspended startup is Windows/macOS only. **No Mac is available. Pingu explicitly authorized macOS support with incomplete native testing on 20 October 2026.** Guarded macOS code is compiled and its contract is source-reviewed; it is not executed, cross-compiled as an Apple application or presented as native proof. The six process/colour recipes retain `humanProof.status=pending`; release-ready packaging remains blocked. This accepted exception permits the combined implementation PR, not a product release.
- Linux redirected Console emits ESC under FORCE_COLOR and plain output under NO_COLOR. Windows emits no ANSI in either tested case. The recipe does not promise Windows FORCE_COLOR rendering, change IsOutputRedirected or rely on conflicting-variable precedence.
- RC2 `Base64.TryDecodeFromChars` throws FormatException for malformed input. The OperationStatus overload reports InvalidData; short destinations and non-final tails are distinct cases.
- CBOR incremental input requires Lax mode, preserves unread bytes at token boundaries and finalizes only when input ends. This parser accepts one measurement array, retains decoded results, rejects extra roots and is not a generic bounded-memory parser. Nesting-policy/custom-classifier proposals remain separate undelivered work.
- No mocks, GA execution, native callback implementation, UI/client matrix, trimming/AOT deployment or macOS execution is claimed. Exact input/metadata freshness gates remain active. No package, account or cloud resource is published/provisioned.
