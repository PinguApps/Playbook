# Assembly deployments evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-742-744.md).

## Sources, reconciled facts and exclusions

Consulted: all target issues; SPEC.md at
`0a8989814ea11346354464b845c79c15bb71c5e9` and current SPEC.md; approved proposal
`b3778cc` keys SH_E01–SH_E03; glossary, ADRs 0001–0004, implementation,
coverage and verification plans, contribution/Inspector guidance, dependency
policy and language/library source register. Root PLAN.md, CONTEXT.md and
CONTEXT-MAP.md are absent. Explicit ticket provenance pointers were resolved.

Boundary-verified related reads that affected delivery:
[PIN-668](https://linear.app/pinguapps/issue/PIN-668) and its comment establish the
completed packed-tool prerequisite;
[PIN-678](https://linear.app/pinguapps/issue/PIN-678) and its comment establish
the available Windows x64 native authoring route while requiring per-recipe
checks. Both are Done and remain unchanged. No related issue was created or
modified.

Official library/compatibility/metadata/deployment guidance and immutable
Microsoft RC2 source reconcile two easy mistakes: SetAssemblyLocationOverride
is static, process-wide and set-once, and VisualBasic's identifier is
`Visual Basic`. Native AOT/GetCallingAssembly behavior differs by release and
StackTraceSupport, independently from physical location availability.
The source register's broader native-artifact migration remains explicitly
partial: runtimeconfig.dev precedence and Unix library prefixes are outside
these supplied targets. Historical planning claims that implementation has not
started are stale; the existing package/harness is repository truth.

The PIN-684 source audit landed on main during verification. Its direct custom
Type regression requirement refines PIN-742's existing nullable migration scope;
the actual .NET 10/11 samples were extended and both proofs recaptured. The audit's
other archive/path/clock dispositions and proposals are preserved when integrating
the latest base. This is repository context, not a related Linear ticket mutation.

All six new projects are version-specific examples inside the isolated samples/
build/package boundary. MetadataInspection alone restores the reviewed
MetadataLoadContext 10.0.9 / 11.0.0-rc.2.26504.105 pins, with lockfiles; the selected
newer asset is lib/net11.0. The two content-description projects and two deployment
projects have no ordinary managed package dependencies. Native publication packs
and toolchains are measured separately in each deployment record's output.
Root consumers, package permissions, workflows and host target stay as configured;
samples remain manually maintained. Dependency policy and real Renovate checks
must pass. No new automated update permission is needed.

Saved evidence/lockfiles remain reviewable under the existing .pragentignore:
their input hashes, exact versions, commands and bounded outputs are useful
review evidence. No policy exclusion is added.

## Evidence limits

Native checks are Windows x64 execution, not cross-compilation or a claim about
other native OS/architecture pairs. Metadata/custom-loader examples require a
file-based untrimmed CoreCLR host, independently of the smaller native diagnostic
sample. Editor UI, code parsing, data-payload decoding, media safety/playback and
external resources are excluded from the claims. .NET 11 artifacts are RC2;
GA reconciliation remains pending. There is no missing required Human proof for
the behavior delivered here. Existing unrelated catalogue proof gaps still
prevent a whole-product release-readiness claim.

Merging, release and publication remain Pingu's responsibility.
