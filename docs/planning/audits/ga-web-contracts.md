# GA web framework reconciliation

PIN-964, 11 November 2026. This bounded release-decision audit does not deliver
the follow-on recipes or authorize a release. PIN-967 remains open.

## Authority and coverage

Scope follows SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`,
stories 41–44, 47, 93–95, 130, 132–133, 146 and 158–160; proposal `b3778cc`
(`GA_WEB`), ADRs 0001–0004 and the release plan. Root PLAN.md, CONTEXT.md and
CONTEXT-MAP.md are absent. The explicit spec/proposal pointers resolve. All
native delivery prerequisites on PIN-964 are Done in the verified PinguApps /
Playbook boundary. Done describes delivery, not successful execution of deferred
checks. PIN-807, PIN-1170 and PIN-1181 explicitly deferred runtime qualification.

The [generated register](ga-web-contracts.sources.json) binds the complete prepared
web family: 117 editions selected through `aspnetcore`, `blazor` and `hosting`,
including the retained .NET 10 request-validation foundation. The remaining seven
foundations belong to other families and are unchanged. All 116 .NET 11 editions
are retained: 108 stable and eight experimental. Hosting must be included explicitly
to retain `hosting.select-direct-tls`. IDs and metadata/Markdown hashes bind the
existing baseline and additions, not just recently authored recipes.

## Actual GA and retained baselines

The [official release feed](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
and [release notes](https://github.com/dotnet/core/blob/33918e8affbb517bf654c15559ea6f813c08bbae/release-notes/11.0/11.0.0/11.0.0.md)
identify SDK 11.0.100 / ASP.NET Core 11.0.0, released 10 November 2026.
The actual installed SDK identifies VMR `297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f`;
132 actual reference assemblies are hashed. ASP.NET source uses the v11.0.0
commit `1def4cdc07dc10fc267136060e3ef5c1a96d4a99`. The register separately pins
the inspected documentation revisions.

All 266 unique retained source URLs from the Blazor and web manifests were
downloaded and their original hashes checked before comparison. Of these, 206
are unchanged, 59 changed and one GA documentation path returns 404. The retained
URLs include the two older-release analyzer controls; their differences from GA
are not newly introduced RC-to-GA contracts. For the RC2 ASP.NET source baseline,
only the Identity UI registration/external-login confirmation checks change;
the existing PIN-1177 recipe already teaches the GA `CanSignInAsync` policy.
Other changed documentation mostly fixes links/front matter or moves the long
AI tutorial to its dedicated page. Two new breaking-change entries need goals
below. This is source comparison, not execution proof.

The existing web metadata baseline already identifies GA. Running
`Inspect-WebApis.ps1` against actual GA and its older control reproduces
`web.assets.json` byte-for-byte, including package, template and TypeScript
inspection. No historical RC report is rewritten.
The audit also retrieves the immutable RC3/RC2 web report from commit
`e0d31f00bca55c5c8afb05a934ed6468fb43314a`, before the GA baseline refresh.
Its public-API delta is confined to IISServerOptions nullable-context attribute
placement, not a new authoring goal. Template/tooling deltas remain separately
recorded, including version changes. Comparing only the current GA report with
itself would not establish the required RC-to-GA comparison.

Blazor retains its distinct historical SDK RC3 / independent package RC2
baseline. The GA metadata comparison finds one added compiler version marker
(`RazorLanguageVersion.Version_12_0`) and the newer experimental AI surface.
The [GA disposition overlay](ga-web.dispositions.json) excludes that compiler
marker: a public member alone is not an external authoring goal. PIN-1166–1168's
withdrawn compiler-host recipes stay withdrawn. The report also retains changed
template/package text, including version updates and email-confirmation markup;
those are not automatically new recipe goals.

## Availability, stability and dispositions

| Surface | Artifact/source decision | Delivery owner |
| --- | --- | --- |
| Identity external provider keys | Newly documented ordinal verification after indexed lookup; case/collation/trailing-space differences require original guidance and real-provider controls | [PIN-1306](https://linear.app/pinguapps/issue/PIN-1306), L (5) |
| OIDC `max_age` / signed `auth_time` | Newly documented handler freshness validation, effective challenge value, PAR timing and process-wide compatibility switch require a focused goal | [PIN-1307](https://linear.app/pinguapps/issue/PIN-1307), L (5) |
| Shared AI state / MuseumCaptions | Actual Components.AI `0.1.0-preview.1.26466.103`, AGUI.Client `0.0.6`, GA host; retain experimental label and pending protocol/browser/conflict/cleanup proof from PIN-807 | [PIN-1308](https://linear.app/pinguapps/issue/PIN-1308), L (5) |
| Conversation restoration / ObservatoryLog | Same actual AI package; retain PIN-1170's pending renderer/storage/recreation proof and completed-turn scope | [PIN-1309](https://linear.app/pinguapps/issue/PIN-1309), L (5) |
| DirectTLS / HarbourTls | GA documentation include was removed; independent package index still offers RC1, with no 11.0.0 and no documented `DirectTlsEndpoint` in the inspected package. Preserve explicit availability wait, ordinary HTTPS fallback and pending native qualification | [PIN-1310](https://linear.app/pinguapps/issue/PIN-1310), M (3) |
| Virtualize/QuickGrid anchoring | GA docs still mention ASP0030, but actual GA metadata has no corresponding experimental attributes. PIN-783/PIN-784 and current stable labels follow actual artifacts; do not reintroduce obsolete suppressions | Retain existing delivered recipes |
| Other AI conversation/tool/approval goals | Already experimental; existing GA compilation/evidence remains its recorded scope. The five AI recipes are not graduated merely by the host reaching GA | Retain existing evidence; refresh only changed inputs |
| Remaining web/Blazor goals | Recorded source/API comparison and existing evidence stay distinct. Neither hash equality nor metadata inspection upgrades a behavioral claim | PIN-967 final candidate/freshness gate |

The AI index has both the older `11.0.0-preview.7.26425.128` and newer
`0.1.0-preview.1.26466.103`; numeric major ordering is not an API currency test.
The selected package's nuspec identifies source `b2f96bd7ac75c07fb8067c05c54767d3d5e12e8d`,
Components.Web RC1 minimum and AI.Abstractions 10.6.0. Actual GA host composition
does not make that independent package stable. Autopause and Gateway packages
do have 11.0.0 artifacts. Their availability is recorded independently.

The five follow-ons are Todo in current cycle 5 (9–16 November 2026), as requested
by Pingu. Each is in milestone 04, related to PIN-964 and blocks PIN-967; the three
deferred-proof follow-ons also link their original delivery issues. Estimates
include implementation, relevant checks and installed-artifact proof. Existing
completed deliveries remain completed; PIN-964 retains L (5). No release gate,
preview exception, served recipe, dependency pin or sample evidence is changed.

## Reproduce

Run with PowerShell 7 and the actual indicated SDK roots. These commands only
inspect metadata/downloaded archives; they do not execute sample applications.
The older portable SDK is an existing local control, not an installer requirement.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-79a10b9d-aa61c0c89225d81b'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$older = 'V:\_worktrees\Playbook\t3-1ead8027\artifacts\dotnet10'
$ga = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet'
& ./eng/Inspect-WebApis.ps1 -Net10Root $older -Net11Root $ga `
  -OutputPath ./artifacts/ga-web.assets.json `
  -CompareWith ./docs/planning/audits/web.assets.json
& ./eng/Inspect-BlazorApis.ps1 -Net10Root $older -Net11Root $ga `
  -AiVersion '0.1.0-preview.1.26466.103' `
  -AdditionalDispositionManifest ./docs/planning/audits/ga-web.dispositions.json `
  -OutputPath ./artifacts/ga-blazor.assets.json
& ./eng/Inspect-GaWebContracts.ps1 -DotnetRoot $ga
```

Expect both metadata checks to pass and the source audit to report 266 source
comparisons, 117 editions and 132 reference assemblies. Default audit output is
disposable; explicitly select the committed register only when refreshing a
release decision. Its missing DirectTLS documentation entry is a recorded 404,
not a skipped transport/API verification. Transient network failures fail the
audit. Mutable package availability can change on a later run and must be
reconciled rather than relabelled as this observation.

These checks establish the artifact/source reconciliation. They do not execute
the deferred browser, AG-UI, storage or Linux/OpenSSL scenarios. The final stable
candidate must wait for the five follow-ons and PIN-967's own evidence checks.
