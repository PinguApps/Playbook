# Source only sdk guidance evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-765-767.md).

## Revised delivery boundary

Pingu explicitly changed the request on 21 October 2026: no Podman acquisition,
tests or live checks; implement all three tickets theoretically and proceed with
creating the combined PR. This supersedes the targets' original compilation,
artifact/behavior and installed-retrieval gates **for this source-review delivery**.
It does not establish any of those results or authorize a public package release.

No build, test, sample command, engine publish, collector, archive operation or MCP
Inspector command below was executed. No Podman installation, new host, registry,
account or resource was provisioned. Earlier read-only discovery confirmed the
existing Docker engine and absent Podman; that is not sample behavior evidence.

## Source-only evidence and release gate

Each of the six sample projects has `global.json`, `verification.json` and a
`source-review.evidence.json`. Records contain actual source-input hashes and
reviewed reference URLs. Their `sdk`, `target` and `language` values describe
intended, unbuilt inputs: SDK 10.0.402/C# 14 or SDK
11.0.100-rc.2.26504.105/C# 15. No compiler/runtime output, native version,
dependency resolution, mock or observed artifact is invented.

The content generator permits missing compilation only for explicitly pending
samples whose supplied records are entirely `official-review`. It preserves
`kind: official-review`, uses `role: source-review`, and adds a consistent
theoretical/missing-proof warning to discovery and detailed applicability.
`freshness: current`, if produced by a future development build, means input
hashes matched; it does not mean the code compiled. Existing verified samples
retain their normal compilation rules. Pending mocks/execution-only records
cannot use this path. `-ReleaseReady` and `PlaybookReleaseReady=true` continue to
reject pending proof. Focused gate scenarios were authored but not executed.

The six new projects inherit only the isolated `samples/` build/package boundary;
SDKs, image inputs and examples remain manually maintained. There are no new
NuGet dependencies, tools, workloads, automated update permissions or CI jobs.
The test project adds embedded canonical sources only. `renovate.json` and its
reviewed consumer inventory therefore need no permission changes. Dependency
policy/Renovate checks were not run, following the no-tests instruction.

Source-review records are meaningful review inputs, so no `.pragentignore`
exclusion was added. They are not bulky compiled artifacts to hide from review.
