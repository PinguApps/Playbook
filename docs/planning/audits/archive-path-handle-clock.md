# Archive, path, handle and clock migrations: PIN-684

Reviewed **21 October 2026**. This is a source-only audit. All ten entries represented by the two assigned inventory rows are dispositioned; adjacent empty-compression coverage is retained explicitly. Five additional delivery proposals await backlog approval. No recipe is delivered by closing this audit.

## Scope and evidence

[PIN-684](https://linear.app/pinguapps/issue/PIN-684), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 99, 103, 158 and 159; approved proposal `b3778cc`, key `SH_Q04`; inventory `migration.archive-streams` and `migration.paths-handles-and-clocks`. Read the full target, empty comments/attachments, cited spec/proposal, current planning README, coverage/implementation/verification plans, glossary, ADRs 0001–0004, contribution rules, [PIN-703's API audit](data-io.md), [PIN-683's proposal convention](numeric-and-parsing.md) and [archive delivery evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-731-733.md). Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. No requirement-bearing provenance pointer is unavailable. Historical planning statements that implementation has not begun are stale; the content and installed-tool harness exist.

Boundary: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. There is no native blocker. An archived-inclusive, paginated query returned **317** issues, all checked against both IDs. Full PIN-731/732/733/741/742 descriptions and comments were read after scoped discovery. The first three are Done; PIN-741 is Backlog and PIN-742 Planned. Their existing work is preserved. No separate owner for the five proposed goals was found; PIN-767's build-archive title is not a runtime TAR hard-link recipe. PIN-703's saved audit explicitly assigns these semantic findings here, including the newer TAR mitigation options.

The [35-file source manifest](archive-path-handle-clock.sources.json) pins:

- dotnet/docs `f0021fde66c59559fa1ba3c9a044148738f12faf`: the complete compatibility index, eleven full notes and SYSLIB0063 guidance. Old/new behavior, introduction, rationale, mitigation and affected APIs were read for every note. The index remains incomplete; this is a bounded audit, not an absence-of-change claim for the release.
- dotnet/dotnet `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` / `820230c386db5b0106c5430b3e5aee92a7826bf5`: .NET 10 / RC2 reference declarations for Runtime, Compression, TAR and Pipes; current implementation and packaging target composition qualify the notes. PIN-703's separate asset evidence identifies these as **10.0.13 / 11.0.0-rc.2.26504.105**; this audit does not rerun its binary comparison.

Source presence establishes a declared contract or implementation path, not compilation or execution of a consumer. Hash replay establishes the reviewed bytes, not their semantic correctness or that every source branch ships in every package asset. RC evidence remains RC evidence; final artifacts and GA reconciliation remain publication obligations. No project, dependency, workload, CI, MCP tool or served recipe changes here.

## Delivered follow-ons: PIN-1040 and PIN-1041

The historical proposals below retain their reviewed source scope; PIN-986 approved
A/B as PIN-1040/1041. Their target11 editions now provide complete offline guidance:
`archive.adapt-package-part-streams` and `archive.choose-hard-link-policy`.
Package proof executes package10/net10, RC2/net11 and RC2/net10, including actual
selected assembly identities/hashes, four access cases, bounded buffering and
failures. TAR proof executes both frameworks on Windows 11/NTFS with real aliases,
actual file IDs, ordered entry/data inspection, both writer/extractor policies,
sync/async path/stream extraction, overwrite rejection, cancellation, target
validation, partial extraction and cleanup. Canonical sources, locks and saved
`verification-evidence.json` records live in the MuseumPackage/MuseumPackageRc2
and ObservatoryTar samples. Ordinary installed-tool inspection is documented in
[the Inspector walkthrough](../../inspect-guidance.md#package-streams-and-tar-hard-link-policy).
No separate older-target catalogue route, link-incapable receiver, other-platform
or GA execution claim is made. Other retained owners remain open;
PIN-959 qualifying coverage and PIN-962 actual-GA reconciliation are not cleared.

## Delivered follow-ons: PIN-1042 and PIN-1043

Approved proposals C/D now deliver `archive.handle-async-initialization` and
`hosting.distinguish-invocation-paths`, edition11/target11. WeatherImport executes
both actual framework sets: two malformed directories with valid end records,
missing end records, real async-only stream reads, ownership/caller cleanup,
pre-cancellation and successful directory discovery with damaged Stored payload.
QuarryMaintenance records fourteen real Windows x64 launches across ordinary and
framework-dependent single-file apphosts, absolute/relative/real NTFS symlink and
dotnet DLL routes. Relative cases use cmd.exe /d /c; the observed spelling is not
a universal launcher contract. Older single-file executable names can retain an
alias basename even though ordinary older apphosts report the managed DLL.

SDK/compiler/ref/runtime identities accompany input-hashed ordinary sample records;
separate `deployment.evidence.json` records capture real apphost publications and
locations. Reproduce with `eng/Record-WeatherMaintenance.ps1 -VerifySources` and
inspect the ordinary installed artifact with the [Inspector walkthrough](../../inspect-guidance.md#zip-initialization-and-invocation-locations).
Both routes use framework assets, no added packages or experimental opt-ins.
No older-target catalogue, Unix, Native AOT, every-launcher or GA execution claim
is added. Existing recipe/edition IDs and the eight foundations remain unchanged;
PIN-959 and PIN-962 retain their independent comprehensive and actual-GA gates.

## Full mitigation dispositions

**Included** retains an authoring obligation. **Grouped** assigns it to an existing goal and distinguishes delivered checks from outstanding work. No listed finding has an unknown owner.

| Finding / introduction | Exact scope and action | Coverage / disposition |
| --- | --- | --- |
| [Compressed PackagePart streams][package-stream], Preview 7 | In a ReadWrite package, an unmodified compressed part opened for Read can be forward-only: seeking/Position assumptions fail; Length remains available. Stored or already modified parts remain seekable; read-only packages already used forward-only streams. Keep sequential readers, or explicitly copy to a bounded seekable buffer and rewind. Opening the package read-only does **not** restore seeking. | **Included → A.** PIN-731's direct ZIP access is supporting context, not OPC PackagePart coverage. `packaging-stream` selects `Open(FileAccess.Read)` under `NET11_0_OR_GREATER`; `packaging-part` supplies the access. Earlier TFM assets retain `Open()`. A package's broad TFM list is not proof of identical behavior. |
| [TAR header checksums][tar-checksum], Preview 1 | TarReader.GetNextEntry/GetNextEntryAsync and extraction can throw InvalidDataException for invalid headers. Handle failure around traversal/extraction, reject or quarantine damaged input; do not continue with partial results. Header checksums do not authenticate a sender or cover arbitrary entry payload corruption. | **Grouped → PIN-732**, delivered `archive.handle-corruption`: a structurally valid Ustar name is changed without recalculating its checksum; the older reader accepts it and RC2 rejects it. Extraction/staging limits remain explicit. No extra goal established by this note. |
| [TAR hard links][tar-hardlinks], Preview 3 | File-path WriteEntry/WriteEntryAsync can emit a HardLink after the first file sharing its identity instead of copying bytes. Use `TarWriterOptions.HardLinkMode = TarHardLinkMode.CopyContents` when independent entries are required. Extraction's separate `TarExtractOptions.HardLinkMode` chooses links or copies; unsupported link creation can fail. | **Included → B.** PIN-732's format/sparse recipe expressly excludes this policy. RC2 exposes `TarWriter(Stream,TarWriterOptions,bool)` and TarFile extraction options overloads for paths/streams, sync/async. Defaults are PreserveLink, PAX writer format, and OverwriteFiles=false. Do not invent an options overload on TarEntry.ExtractToFile; manual TarEntry writes are not the same as filesystem identity detection. |
| [ZIP asynchronous initialization][zip-eager], Preview 1 | `ZipArchive.CreateAsync` reads the central directory before returning in Read mode; malformed-directory errors can move from Entries access into the awaited factory. Keep initialization in the InvalidDataException boundary. Successful directory loading does not read/CRC-validate every payload. | **Included → C.** PIN-731 covers entry access/protection; PIN-732 covers full payload reads, neither proves this timing. `zip-initialization` awaits EnsureCentralDirectoryReadAsync in Read and nonempty Update modes; Create has no existing directory. Both reference baselines expose the same five-argument factory. Existing .NET 10 initialization failures must not be described as impossible. |
| [Apphost invocation name][apphost-name], Preview 7 | GetCommandLineArgs()[0] preserves invocation spelling, including relative/symlink names, when started through apphost. `dotnet application.dll` is unchanged. Choose ProcessPath for the executable and entry-assembly Location for the managed file, accounting for null/empty locations and single-file deployment. Keep argv[0] only for the invocation identity. | **Included → D.** PIN-742's assembly-location work is related but does not own apphost launch identity. No new ProcessPath API is implied. Treat executable, assembly and invocation as distinct values; none alone defines an application data directory or trusted path. |
| [Unix IsAsync][unix-isasync], Preview 3 | SafeFileHandle.IsAsync/FileStream.IsAsync reflect nonblocking descriptors; regular files can report false despite FileOptions.Asynchronous. Do not reject async-method use solely on that flag. FileStream-backed SendPacketsElement no longer requires an async stream on non-Windows; Windows retains that constructor guard. | **Grouped → PIN-741**, `io.use-pipe-handles`, still undelivered. Review regular files, both pipe ends, wrapping and ownership; `filehandle-unix` probes/caches O_NONBLOCK and sets regular files false, and `sendpackets` gates its rejection on Windows. This is not proof that the flag tracks later external fcntl changes or that Unix regular-file operations become kernel-asynchronous. |
| [Unix CurrentUserOnly permissions][unix-pipe-mode], Preview 4 | NamedPipeServerStream tightens its socket node to 0600. A CurrentUserOnly instance also tightens an existing shared path; a later unrestricted instance does not loosen it until that shared lifetime ends. Omitting the flag alone cannot guarantee cross-user access: directory permissions, umask and peer checks still matter. | **Grouped → PIN-741**, still undelivered. `pipe-unix` confirms bind-time permissions and one-way shared-server ratcheting. Keep same-user communication as the default; a claimed cross-user route needs isolated real-user evidence and its intended access policy. Do not silently remove a security restriction as a generic fix. |
| [Windows TickCount][windows-clock], Preview 1 | TickCount/TickCount64 exclude sleep/hibernation time on Windows, aligning with OS timeout waits. Windows TickCount64 uses QueryUnbiasedInterruptTime; other platforms retain their behavior. If a business deadline must include sleep, use an explicit UTC deadline and account for clock adjustments. TickCount's wrap remains relevant; higher timer resolution is not guaranteed. | **Included → E.** No existing clock goal was found. The change can be invisible to ordinary callers, but a persisted real-time expiry assumption needs authored migration. Do not substitute Stopwatch/TimeProvider timestamp claims for measured suspend behavior, or promise UtcNow is monotonic. |
| [Custom Type nullable inspection][custom-type], Preview 4 | Nullable.GetUnderlyingType delegates to virtual Type.GetNullableUnderlyingType; the Type base throws NotSupportedException. Custom Type implementations need a meaningful override: null only if never nullable, an appropriate underlying type in their reflection universe, or delegation to an inner type. Shipped overriding Type implementations are not blanket-broken. No reversion switch is documented. | **Grouped → PIN-742**, `reflection.inspect-loaded-metadata`, still undelivered. Both `nullable-implementation` and `type-implementation` confirm delegation/default throw; only the newer reference baseline has the virtual. Tests must include a direct custom Type that inherits the base method; TypeDelegator already overrides it. Never compare another reflection universe's Nullable definition only with runtime typeof(Nullable<>). |
| [ZIP CRC32][zip-crc], Preview 3 | Complete entry reads can throw InvalidDataException when recorded and computed CRC differ; merely opening a ZIP or reading a prefix does not prove integrity. | **Grouped → PIN-732**, delivered `archive.handle-corruption`, including changed Stored bytes with valid structure/recorded CRC. Retains EOF and authenticity limits. Adjacent cross-check, not a new goal. |
| [Empty Deflate/GZip][empty-compression], Preview 1 | Disposing a compression stream without writes now emits an empty compressed representation. Track original emptiness separately or deliberately encode an empty protocol message without compression when that is the protocol contract. | **Grouped → PIN-733**, delivered `compression.handle-empty-payloads`, including actual no-write 0/0 versus 2/20-byte observations and nonempty round trips. Those measured sizes are not universal output-size guarantees. Adjacent cross-check, not a new goal. |

The ten assigned entries are PackagePart, TAR checksum/hard links, ZIP initialization/CRC, apphost, Unix IsAsync/permissions, Windows clock and custom Type. Empty compression makes eleven reviewed notes. The related **SYSLIB0063** source/reference check retains PIN-703's existing assignment to PIN-741: replace the four-argument NamedPipeClientStream constructor containing ineffective isConnected with the three-argument handle constructor. This adds no new recipe goal and does not resolve PIN-689's release-wide obsolete index.

## Delivery proposals for backlog approval

**Approval only: none created or implemented.** Same team/project; existing **02 - Language and library guidance** milestone `c5232470-50ac-4f56-aea8-bd8ff7d13b41`, Backlog. The current allocated maximum is **02-053**. Titles below use provisional **02-063–02-067**, following PIN-683's uncreated proposals 02-054–02-062; these are not reservations. Repeat overlap/prefix checks and allocate the actual next free numbers in the approved creation order. No milestone, existing title or completed state changes are proposed.

Native estimates S=2/M=3/L=5 are established by the target and comparable in-scope tickets. Each estimate includes original samples/prose, compilation, claim-specific execution and installed offline retrieval. Each proposal is one independently deliverable PR; shared files and numerical order create no dependency. Relate approved follow-ons to PIN-684 and the comprehensive coverage gate, plus the existing owner named below.

### Shared delivery acceptance

Every proposal requires all of the following in addition to its focused checks:

1. Reconcile its full mitigation and exact selected package/framework declarations before authoring. Use an original fictional example and a fair same-goal .NET 10/.NET 11 comparison. Preserve existing stable IDs/editions; this adds no ninth .NET 10 foundation.
2. Deliver complete offline goal, older/newer approaches, numbered migration, constraints, alternatives, official references and snippets drawn from canonical source. Discovery must describe the reusable capability and when to apply it. State actual runtime, SDK, language, package asset, OS, rendering and stability requirements; check experimental attributes/opt-ins rather than inferring them from the RC label.
3. Reuse the existing sample/evidence harness. Starting reproducibility baselines are SDK **10.0.402**, runtime/ref **10.0.13**, C# 14/net10.0, and SDK **11.0.100-rc.2.26504.105**, runtime/ref **11.0.0-rc.2.26504.105**, C# 15/net11.0. Follow the dependency policy for actual new projects/pins and capture exact dependencies, commands/output and input hashes. No route is advertised from source presence or broad package TFMs alone.
4. Compile and execute the actual old/new consumers and focused failures below. Missing required OS, filesystem, suspend or deployment proof blocks that delivery PR. Source review, mocks, simulated sleep and cross-compilation cannot stand in for real claimed behavior. Reuse unchanged evidence; refresh affected inputs.
5. Build/pack the ordinary artifact, then use the existing installed stdio harness with an isolated feed/cache and unrelated working directory. Retrieve list ID/title/**goal** plus full get Markdown, canonical snippet identity, target edition, applicability and evidence offline; check unsupported target refusal. No new recurring OS/client/native matrix. Update the register; RC proof cannot clear GA/publication obligations. These proposals authorize no release or resource provisioning.

### A. 02-063 - Supply seekable package data only when required — M (3)

Proposed ID `archive.adapt-package-part-streams`. A fictional museum document importer supports sequential parsing and a parser that genuinely needs seeking. Use **System.IO.Packaging 10.0.13 / 11.0.0-rc.2.26504.105** with reviewed lockfiles and actual selected net10/net11 assets. No Open XML SDK dependency is needed for the bounded example.

Create/reopen real packages and check the full access matrix: ReadWrite + unmodified compressed part opened Read, stored part, part modified earlier in the session, and read-only package. Record CanSeek, Seek/Position failures, Length and sequential content; prove explicit bounded buffering/rewind enables random access without changing contents. Reject oversized buffering before unbounded allocation. Check the RC2 package on a real net10 consumer separately before claiming the conditional older-target route: `NET11_0_OR_GREATER` means its behavior must not be inferred from the package version. M covers this asset/access matrix and actual package evidence. Related: PIN-731. Source: [PackagePart][package-stream], `packaging-stream`, `packaging-part`, `packaging-project`.

### B. 02-064 - Choose TAR hard-link preservation or independent copies — M (3)

Proposed ID `archive.choose-hard-link-policy`. A fictional observatory exports two names for one calibration file. Use a real filesystem supporting hard links, create two actual hard links, and record filesystem/OS evidence. Compare .NET 10 and RC2 path-based writer entries: entry types, LinkName, data and order; ensure two unrelated files with equal bytes are not mistaken for hard links.

Exercise both PreserveLink and CopyContents during writing and TarFile extraction, sync/async where advertised, overwrite rejection, cancellation and cleanup. Prove content equality plus actual shared/independent file identity; selecting a TAR format or checking equal bytes is insufficient. A receiver without link support uses CopyContents; do not claim that receiver was tested without a real suitable filesystem. Keep path traversal/link-target and partial-extraction limits explicit. No arbitrary link creation outside an isolated test directory. Framework route first; a separate package/older-target claim needs its own consumer proof. M covers real file identity and the options lifecycle; unavailable filesystem proof remains a blocker. Related: PIN-732. Source: [hard links][tar-hardlinks], `system-formats-tar-10/11`, `tar-writer-options`, `tar-extract-options`, `tar-extraction`.

### C. 02-065 - Handle ZIP failures during asynchronous initialization — S (2)

Proposed ID `archive.handle-async-initialization`. A fictional weather importer includes the awaited factory and entry traversal inside its error boundary. Create original ZIP fixtures with a valid end record but malformed central-directory entries, so .NET 10 initialization versus later Entries failure and RC2 eager failure can be distinguished. Also test an archive already rejected at initialization by both releases.

Use a tracking stream that permits asynchronous reads and rejects synchronous reads, backed by real valid archive bytes. On RC2, prove the awaited Read-mode factory completes without synchronous reads and subsequent Entries access performs no reads. Separate metadata discovery from complete payload CRC validation, with a successful-directory/damaged-payload case. Check cancellation, leaveOpen ownership and disposal on failure. Trackers instrument real ZIP processing; they are not a mocked ZipArchive. S covers this bounded managed failure boundary and full installed delivery. Related: PIN-731/732. Source: [ZIP initialization][zip-eager], `zip-initialization`, `system-io-compression-10/11`.

### D. 02-066 - Distinguish invocation names from application file locations — M (3)

Proposed ID `hosting.distinguish-invocation-paths`. A fictional maintenance CLI chooses its command alias separately from the location of its executable and managed assembly. Launch actual .NET 10/11 apphosts by absolute and relative paths, through an alias/symlink where supported, and via `dotnet application.dll`. Capture argv[0], ProcessPath and entry-assembly Location from each child. Repeat a real single-file publish/run to show empty bundled Location, and handle nullable/unavailable locations without inventing a data-directory rule.

Use a suitable OS with actual symlink capability; record if a Windows privilege is unavailable and use a verified Unix route rather than claiming a simulated alias. Shell launch spelling/resolution is part of the evidence; do not promise identical relative spelling across every launcher. Avoid executing a reconstructed untrusted argv[0] path. M covers subprocess/apphost/single-file evidence. Related: PIN-742; retain its separate assembly-location goal and stable ID. Source: [invocation name][apphost-name], `system-runtime-10/11`.

### E. 02-067 - Choose whether a deadline includes system sleep — M (3)

Proposed ID `runtime.choose-sleep-aware-deadlines`. A fictional unattended inspection station distinguishes an awake-time retry budget from an absolute business expiry. Show the old/new Windows TickCount assumption, then select explicit UTC expiry for the latter with a documented policy for wall-clock jumps. Do not promise monotonic civil time or use a fake-clock result as suspend proof.

Run both actual runtime consumers on a **separate Windows test machine** through real sleep/resume, collecting TickCount64, TickCount, UTC and OS wait outcomes with tolerances. This audit does not suspend Pingu's workstation. Record measured OS/power mode, suspend interval and commands; hibernation needs separate actual evidence before advertising that tested route. The assigned Agent obtains a named Human/environment handoff if it cannot safely perform this check; missing proof blocks delivery. Add deterministic forward/backward-clock-jump policy checks at the existing clock seam, clearly labelled separately from physical execution. Prefer TickCount64 for elapsed budgets; retain 32-bit wrap constraints if showing TickCount. M includes the bounded power-state evidence/handoff and ordinary offline delivery. Source: [clock][windows-clock], `environment-windows`.

## Reproduce source verification

PowerShell 7 and public network access are required. This exact block was tested from an unrelated directory. It downloads immutable source bytes into a new temporary directory, checks all **35** hashes and all eleven notes' mitigation sections, then displays the five proposals. No inspected code is executed, and no installer or SDK setup is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-72f4af4b'
$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath './docs/planning/audits/archive-path-handle-clock.sources.json' -Raw | ConvertFrom-Json
$auditTemp = Join-Path ([IO.Path]::GetTempPath()) ('pin684-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $auditTemp | Out-Null
try {
    if ($manifest.sources.Count -ne 35) { throw 'Expected 35 reviewed sources' }
    $noteIds = @('package-stream', 'tar-checksum', 'tar-hardlinks', 'zip-eager', 'zip-crc', 'empty-compression', 'apphost-name', 'unix-pipe-mode', 'unix-isasync', 'windows-clock', 'custom-type')
    foreach ($source in $manifest.sources) {
        $file = Join-Path $auditTemp ($source.id + '.txt')
        Invoke-WebRequest -Uri $source.url -OutFile $file
        if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) {
            throw "Source hash mismatch: $($source.id)"
        }
        if ($source.id -in $noteIds) {
            $body = Get-Content -LiteralPath $file -Raw
            foreach ($section in @('Previous behavior', 'New behavior', 'Recommended action', 'Affected APIs')) {
                if ($body -notmatch ('(?m)^## ' + [regex]::Escape($section) + '\r?$')) {
                    throw "Missing mitigation section: $($source.id) / $section"
                }
            }
        }
    }
    Get-Content -LiteralPath './docs/planning/audits/archive-path-handle-clock.md' |
        Select-String -Pattern '^### [A-E]\. '
    'PASS: 35 source hashes; eleven full mitigation notes; five approval-only delivery proposals'
}
finally {
    $resolvedTemp = [IO.Path]::GetFullPath($auditTemp)
    if (-not $resolvedTemp.StartsWith([IO.Path]::GetTempPath(), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected cleanup path' }
    Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
}
```


[package-stream]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/packagepart-getstream-non-seekable.md
[tar-checksum]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/tar-checksum-validation.md
[tar-hardlinks]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/tarwriter-hardlink-entries.md
[zip-eager]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/ziparchive-createasync-eager-load.md
[zip-crc]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/ziparchive-entry-crc32-validation.md
[empty-compression]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/deflatestream-gzipstream-empty-payload.md
[apphost-name]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/environment-getcommandlineargs-apphost-invocation-name.md
[unix-pipe-mode]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/namedpipeserverstream-unix-permissions.md
[unix-isasync]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/safefilehandle-isasync-unix.md
[windows-clock]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/environment-tickcount-windows-behavior.md
[custom-type]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/nullable-getunderlyingtype-throws.md
