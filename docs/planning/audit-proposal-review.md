# Audit proposal decisions and delivery ownership

Review date: **26 October 2026**. Checkpoint: [PIN-986](https://linear.app/pinguapps/issue/PIN-986).
Source baseline: Playbook main `a7fd46cd658708c25a7e0d032735760c3fe6d63f`.
Destination: **PinguApps / Playbook**, team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, project `b22380c7-6fde-49f4-8826-e5631dc5df7a`.

## Decision and scope

Pingu explicitly authorized autonomous review decisions and creation of the resulting
backlog tickets on 26 October 2026, after clarifying that each proposal represents
one recipe delivery ticket. That authorization supersedes the earlier per-batch
approval pause. The first six language/compiler proposals were presented separately;
the remaining area batches and decisions are recorded below.

**194 historical draft entries are reconciled: 188 approved delivery tickets,
five already-delivered F# goals and one bundled-template proposal grouped into an
existing active ticket.** No confirmed qualifying recipe is excluded or waived.
These are proposal/PR boundaries, not a claim that 188 additional recipe IDs must
be published: the numeric representation follow-on extends an existing recipe,
and supported consumer qualification can establish an evidenced disposition.

This checkpoint creates tracked work and saves the decision record. It does not
implement the resulting recipes, run their consumer/native/database proofs, approve
publication or establish GA evidence. Each ticket retains its original focused
scenarios, prerequisites, limitations and shared delivery acceptance, not merely
its title. Approved work starts in Backlog, unassigned, with Agent / Feature /
New Guidance labels and the existing milestone appropriate to its outcome.

## Readiness, reconciliation and estimates

The nine native source-audit blockers on PIN-986 were re-read as Done:
[PIN-689](https://linear.app/pinguapps/issue/PIN-689), [PIN-690](https://linear.app/pinguapps/issue/PIN-690), [PIN-692](https://linear.app/pinguapps/issue/PIN-692), [PIN-693](https://linear.app/pinguapps/issue/PIN-693), [PIN-694](https://linear.app/pinguapps/issue/PIN-694), [PIN-695](https://linear.app/pinguapps/issue/PIN-695), [PIN-708](https://linear.app/pinguapps/issue/PIN-708), [PIN-709](https://linear.app/pinguapps/issue/PIN-709), [PIN-710](https://linear.app/pinguapps/issue/PIN-710).
All nineteen proposal-producing audit owners are also Done. No newly identified
pending preparation audit is required to decide these recorded proposals. Required
consumer/artifact qualification stays explicit in the delivery slices; actual GA
reconciliation stays with its existing family tickets.

The archived-inclusive, paginated project refresh contained **321 issues** before
creation; every returned team/project ID matched the destination. Full source-audit
handoffs, relevant existing issue criteria, current recipes and the three planning
registers were used to distinguish delivery from audit completion. The historical
116-plus-sixteen count is superseded by the enumerated draft set below; the original
406 source rows / 310 candidate rows remain historical inventory counts rather
than a new-ticket total.

Native estimates use the established **S = 2, M = 3, L = 5** scale. Each retained
size covers original samples/prose, artifact qualification, real focused checks,
evidence and installed offline retrieval. Claim-specific complexity and sizing
remain in the linked source draft and saved ticket. No provisional scale, enabled
estimation setting, renamed existing ticket or new milestone is involved.
New prefixes append to the existing allocations: 02-054–128, 03-013–050,
04-071–111, 05-048–078 and 07-039–041. Old draft numbers were not reservations.

## Delivery and publication rules

Every new delivery ticket is natively related to its source audit and blocks
[PIN-959](https://linear.app/pinguapps/issue/PIN-959) plus its applicable final family reconciliation. Only actual
pending environment/native-provider prerequisites are native blockers; file sharing,
adjacent titles and milestone order add no dependency. The table records those
pending prerequisites. All tickets preserve the source draft's additional artifact,
OS/provider/credential and focused positive/failure proof requirements.

Two explicit availability gaps remain owned: [PIN-1170](https://linear.app/pinguapps/issue/PIN-1170)
requires a real AI conversation-restore artifact; [PIN-1181](https://linear.app/pinguapps/issue/PIN-1181)
requires a usable matching direct-TLS package/contract. They remain Backlog with
hard availability criteria and native publication edges. No invented interface,
source-only placeholder or deferral clears qualifying coverage.

The bundled-template draft is grouped into [PIN-819](https://linear.app/pinguapps/issue/PIN-819), preserving its active
state, `web.scaffold-mcp-host` identity and real generated-consumer / advertised
transport requirements. No `sdk.scaffold-mcp-server` duplicate is created.
The five historical F# drafts retain Done [PIN-980](https://linear.app/pinguapps/issue/PIN-980), [PIN-981](https://linear.app/pinguapps/issue/PIN-981), [PIN-982](https://linear.app/pinguapps/issue/PIN-982), [PIN-983](https://linear.app/pinguapps/issue/PIN-983), [PIN-984](https://linear.app/pinguapps/issue/PIN-984)
and their delivered recipes/evidence. Numeric draft B extends
`numerics.preserve-conversion-intent` from [PIN-737](https://linear.app/pinguapps/issue/PIN-737); its complete conversion
matrix, legacy representation policy and compiler-folded older-target checks are
not already established by that recipe's bounded evidence.

## Proposal decisions by source area

Every original draft key has exactly one retained owner. “Approved” means a new
bounded delivery ticket, not completed recipe evidence. Existing owner states are
preserved. The source links retain the full reusable goal, scope, concrete
prerequisites, compilation/behaviour acceptance and sizing rationale.

### compiler-contracts

[PIN-682](https://linear.app/pinguapps/issue/PIN-682) · [source drafts and shared acceptance](audits/compiler-contracts.md) · 5 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `compiler-contracts:A` | Preserve span storage across nested scopes | approved | [PIN-1032](https://linear.app/pinguapps/issue/PIN-1032) | M (3) | — |
| `compiler-contracts:B` | Restore readonly delegate and local-function references | approved | [PIN-1033](https://linear.app/pinguapps/issue/PIN-1033) | M (3) | — |
| `compiler-contracts:C` | Preserve dynamic logical operator binding | approved | [PIN-1034](https://linear.app/pinguapps/issue/PIN-1034) | M (3) | — |
| `compiler-contracts:D` | Inspect null-coalescing conversions in C# and VB | approved | [PIN-1035](https://linear.app/pinguapps/issue/PIN-1035) | L (5) | — |
| `compiler-contracts:E` | Inspect declared type layout through compiler symbols | approved | [PIN-1036](https://linear.app/pinguapps/issue/PIN-1036) | M (3) | — |

### fsharp-contracts

[PIN-681](https://linear.app/pinguapps/issue/PIN-681) · [source drafts and shared acceptance](audits/fsharp-contracts.md) · 5 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `fsharp-contracts:A` | Author an experimental runtime-async task builder | already covered | [PIN-980](https://linear.app/pinguapps/issue/PIN-980) | L (5) | — |
| `fsharp-contracts:B` | Choose imported-assembly sharing in an F# compiler host | already covered | [PIN-981](https://linear.app/pinguapps/issue/PIN-981) | M (3) | — |
| `fsharp-contracts:C` | Migrate two-dimensional array indexing for Native AOT | already covered | [PIN-982](https://linear.app/pinguapps/issue/PIN-982) | M (3) | — |
| `fsharp-contracts:D` | Await asynchronous cleanup in F# Interactive | already covered | [PIN-983](https://linear.app/pinguapps/issue/PIN-983) | S (2) | — |
| `fsharp-contracts:E` | Build an experimental pull-driven F# asynchronous sequence | already covered | [PIN-984](https://linear.app/pinguapps/issue/PIN-984) | L (5) | — |

### automatic-exclusions

[PIN-690](https://linear.app/pinguapps/issue/PIN-690) · [source drafts and shared acceptance](audits/automatic-exclusions.md) · 3 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `automatic-exclusions:A` | Adapt an opaque curried callback once | approved | [PIN-1037](https://linear.app/pinguapps/issue/PIN-1037) | M (3) | — |
| `automatic-exclusions:B` | Choose a contract for IEEE-decimal function results | approved | [PIN-1038](https://linear.app/pinguapps/issue/PIN-1038) | M (3) | — |
| `automatic-exclusions:C` | Bind Linux Negotiate authentication to TLS | approved | [PIN-1039](https://linear.app/pinguapps/issue/PIN-1039) | L (5) | — |

### archive-path-handle-clock

[PIN-684](https://linear.app/pinguapps/issue/PIN-684) · [source drafts and shared acceptance](audits/archive-path-handle-clock.md) · 5 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `archive-path-handle-clock:A` | Supply seekable package data only when required | approved | [PIN-1040](https://linear.app/pinguapps/issue/PIN-1040) | M (3) | — |
| `archive-path-handle-clock:B` | Choose TAR hard-link preservation or independent copies | approved | [PIN-1041](https://linear.app/pinguapps/issue/PIN-1041) | M (3) | — |
| `archive-path-handle-clock:C` | Handle ZIP failures during asynchronous initialization | approved | [PIN-1042](https://linear.app/pinguapps/issue/PIN-1042) | S (2) | — |
| `archive-path-handle-clock:D` | Distinguish invocation names from application file locations | approved | [PIN-1043](https://linear.app/pinguapps/issue/PIN-1043) | M (3) | — |
| `archive-path-handle-clock:E` | Choose whether a deadline includes system sleep | approved | [PIN-1044](https://linear.app/pinguapps/issue/PIN-1044) | M (3) | — |

### numeric-and-parsing

[PIN-683](https://linear.app/pinguapps/issue/PIN-683) · [source drafts and shared acceptance](audits/numeric-and-parsing.md) · 9 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `numeric-and-parsing:A` | Separate invalid date parsing options from bad data | approved | [PIN-1045](https://linear.app/pinguapps/issue/PIN-1045) | S (2) | — |
| `numeric-and-parsing:B` | Preserve deliberate legacy numeric representations | approved | [PIN-1046](https://linear.app/pinguapps/issue/PIN-1046) | M (3) | — |
| `numeric-and-parsing:C` | Choose explicit fractional rounding semantics | approved | [PIN-1047](https://linear.app/pinguapps/issue/PIN-1047) | M (3) | — |
| `numeric-and-parsing:D` | Select a small integer conversion policy | approved | [PIN-1048](https://linear.app/pinguapps/issue/PIN-1048) | M (3) | — |
| `numeric-and-parsing:E` | Handle complex nonfinite results deliberately | approved | [PIN-1049](https://linear.app/pinguapps/issue/PIN-1049) | M (3) | — |
| `numeric-and-parsing:F` | Align tensor shapes without losing metadata | approved | [PIN-1050](https://linear.app/pinguapps/issue/PIN-1050) | L (5) | — |
| `numeric-and-parsing:G` | Copy tensor views with explicit storage ownership | approved | [PIN-1051](https://linear.app/pinguapps/issue/PIN-1051) | M (3) | — |
| `numeric-and-parsing:H` | Validate dates against the Japanese calendar boundary | approved | [PIN-1052](https://linear.app/pinguapps/issue/PIN-1052) | S (2) | — |
| `numeric-and-parsing:I` | Validate tensor clamp bounds consistently | approved | [PIN-1053](https://linear.app/pinguapps/issue/PIN-1053) | S (2) | — |

### hosting-configuration

[PIN-685](https://linear.app/pinguapps/issue/PIN-685) · [source drafts and shared acceptance](audits/hosting-configuration.md) · 3 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `hosting-configuration:A` | Preserve background-service failures at the host boundary | approved | [PIN-1054](https://linear.app/pinguapps/issue/PIN-1054) | S (2) | — |
| `hosting-configuration:B` | Observe configuration load failures separately from changes | approved | [PIN-1055](https://linear.app/pinguapps/issue/PIN-1055) | M (3) | — |
| `hosting-configuration:C` | Compose a filesystem watcher with the intended root | approved | [PIN-1056](https://linear.app/pinguapps/issue/PIN-1056) | M (3) | — |

### cryptography-platforms

[PIN-686](https://linear.app/pinguapps/issue/PIN-686) · [source drafts and shared acceptance](audits/cryptography-platforms.md) · 7 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `cryptography-platforms:A` | Replace a macOS DSA signing contract | approved | [PIN-1057](https://linear.app/pinguapps/issue/PIN-1057) | L (5) | [PIN-698](https://linear.app/pinguapps/issue/PIN-698) |
| `cryptography-platforms:B` | Gate Windows composite signatures by the exact algorithm | approved | [PIN-1058](https://linear.app/pinguapps/issue/PIN-1058) | L (5) | [PIN-696](https://linear.app/pinguapps/issue/PIN-696) |
| `cryptography-platforms:C` | Enforce an explicit HKDF input contract on Windows | approved | [PIN-1059](https://linear.app/pinguapps/issue/PIN-1059) | M (3) | [PIN-696](https://linear.app/pinguapps/issue/PIN-696) |
| `cryptography-platforms:D` | Supply certificate intermediates without relying on excess AIA hops | approved | [PIN-1060](https://linear.app/pinguapps/issue/PIN-1060) | M (3) | — |
| `cryptography-platforms:E` | Update decoded ASN.1 extension state through supported APIs | approved | [PIN-1061](https://linear.app/pinguapps/issue/PIN-1061) | S (2) | — |
| `cryptography-platforms:F` | Make legacy RSA encryption padding explicit | approved | [PIN-1062](https://linear.app/pinguapps/issue/PIN-1062) | M (3) | [PIN-696](https://linear.app/pinguapps/issue/PIN-696) |
| `cryptography-platforms:G` | Validate TLS client certificates with an explicitly supplied chain | approved | [PIN-1063](https://linear.app/pinguapps/issue/PIN-1063) | M (3) | — |

### native-deployment

[PIN-687](https://linear.app/pinguapps/issue/PIN-687) · [source drafts and shared acceptance](audits/native-deployment.md) · 2 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `native-deployment:A` | Preserve production runtime settings while allowing development overrides | approved | [PIN-1064](https://linear.app/pinguapps/issue/PIN-1064) | M (3) | — |
| `native-deployment:B` | Keep Unix native-library packaging and callers aligned with output names | approved | [PIN-1065](https://linear.app/pinguapps/issue/PIN-1065) | L (5) | [PIN-698](https://linear.app/pinguapps/issue/PIN-698) |

### sdk-package-hosts

[PIN-688](https://linear.app/pinguapps/issue/PIN-688) · [source drafts and shared acceptance](audits/sdk-package-hosts.md) · 9 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `sdk-package-hosts:A` | Select the intended SDK when executing a tool | approved | [PIN-1066](https://linear.app/pinguapps/issue/PIN-1066) | S (2) | — |
| `sdk-package-hosts:B` | Configure an explicit legacy Mono launch | approved | [PIN-1067](https://linear.app/pinguapps/issue/PIN-1067) | M (3) | — |
| `sdk-package-hosts:C` | Choose a compliant package identity during migration | approved | [PIN-1068](https://linear.app/pinguapps/issue/PIN-1068) | M (3) | — |
| `sdk-package-hosts:D` | Replace a dependency selected from legacy Android assets | approved | [PIN-1069](https://linear.app/pinguapps/issue/PIN-1069) | M (3) | [PIN-697](https://linear.app/pinguapps/issue/PIN-697) |
| `sdk-package-hosts:E` | Restore dependencies independently of next-best cache entries | approved | [PIN-1070](https://linear.app/pinguapps/issue/PIN-1070) | M (3) | — |
| `sdk-package-hosts:F` | Retarget an embedded template-engine host | approved | [PIN-1071](https://linear.app/pinguapps/issue/PIN-1071) | L (5) | — |
| `sdk-package-hosts:G` | Own JSON dependencies in test projects and extensions | approved | [PIN-1072](https://linear.app/pinguapps/issue/PIN-1072) | M (3) | — |
| `sdk-package-hosts:H` | Read VSTest messages through the supported typed contract | approved | [PIN-1073](https://linear.app/pinguapps/issue/PIN-1073) | M (3) | — |
| `sdk-package-hosts:I` | Select affected tests with a verified experimental map | approved | [PIN-1074](https://linear.app/pinguapps/issue/PIN-1074) | L (5) | [PIN-696](https://linear.app/pinguapps/issue/PIN-696) |

### data-io

[PIN-703](https://linear.app/pinguapps/issue/PIN-703) · [source drafts and shared acceptance](audits/data-io.md) · 10 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `data-io:D1` | Create file aliases with hard links | approved | [PIN-1075](https://linear.app/pinguapps/issue/PIN-1075) | M (3) | — |
| `data-io:D2` | Choose integer division and remainder rules | approved | [PIN-1076](https://linear.app/pinguapps/issue/PIN-1076) | M (3) | — |
| `data-io:D3` | Compute integer decimal magnitude without rounding | approved | [PIN-1077](https://linear.app/pinguapps/issue/PIN-1077) | S (2) | — |
| `data-io:D4` | Compute the checksum a binary format requires | approved | [PIN-1078](https://linear.app/pinguapps/issue/PIN-1078) | M (3) | — |
| `data-io:D5` | Preserve colour channel order across binary boundaries | approved | [PIN-1079](https://linear.app/pinguapps/issue/PIN-1079) | M (3) | — |
| `data-io:D6` | Find extrema directly in an existing span | approved | [PIN-1080](https://linear.app/pinguapps/issue/PIN-1080) | S (2) | — |
| `data-io:D7` | Cancel asynchronous text output explicitly | approved | [PIN-1081](https://linear.app/pinguapps/issue/PIN-1081) | S (2) | — |
| `data-io:D8` | Convert international domain labels into caller buffers | approved | [PIN-1082](https://linear.app/pinguapps/issue/PIN-1082) | S (2) | — |
| `data-io:D9` | Report the failing directory without parsing messages | approved | [PIN-1083](https://linear.app/pinguapps/issue/PIN-1083) | S (2) | — |
| `data-io:D10` | Choose reduced-precision binary values deliberately | approved | [PIN-1084](https://linear.app/pinguapps/issue/PIN-1084) | M (3) | — |

### serialization

[PIN-704](https://linear.app/pinguapps/issue/PIN-704) · [source drafts and shared acceptance](audits/serialization.md) · 2 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `serialization:S1` | Bound nesting when reading and writing CBOR | approved | [PIN-1085](https://linear.app/pinguapps/issue/PIN-1085) | M (3) | — |
| `serialization:S2` | Choose JSON variants from payload values | approved | [PIN-1086](https://linear.app/pinguapps/issue/PIN-1086) | M (3) | — |

### runtime-security

[PIN-705](https://linear.app/pinguapps/issue/PIN-705) · [source drafts and shared acceptance](audits/runtime-security.md) · 25 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `runtime-security:R1` | Verify message authenticators | approved | [PIN-1087](https://linear.app/pinguapps/issue/PIN-1087) | M (3) | — |
| `runtime-security:R2` | Encode registered identifiers in certificates | approved | [PIN-1088](https://linear.app/pinguapps/issue/PIN-1088) | S (2) | — |
| `runtime-security:R3` | Inspect typed DNS records beyond service selection | approved | [PIN-1089](https://linear.app/pinguapps/issue/PIN-1089) | M (3) | — |
| `runtime-security:R4` | Race IPv4 and IPv6 connections deliberately | approved | [PIN-1090](https://linear.app/pinguapps/issue/PIN-1090) | M (3) | — |
| `runtime-security:R5` | Parse and format endpoints in UTF-8 buffers | approved | [PIN-1091](https://linear.app/pinguapps/issue/PIN-1091) | S (2) | — |
| `runtime-security:R6` | Monitor network changes with a supported fallback | approved | [PIN-1092](https://linear.app/pinguapps/issue/PIN-1092) | M (3) | — |
| `runtime-security:R7` | Choose a supported threading path | approved | [PIN-1093](https://linear.app/pinguapps/issue/PIN-1093) | M (3) | — |
| `runtime-security:R8` | Update enum flags atomically | approved | [PIN-1094](https://linear.app/pinguapps/issue/PIN-1094) | M (3) | — |
| `runtime-security:R9` | Match native C structure and union layouts | approved | [PIN-1095](https://linear.app/pinguapps/issue/PIN-1095) | L (5) | — |
| `runtime-security:R10` | Emit typed function-pointer calls | approved | [PIN-1096](https://linear.app/pinguapps/issue/PIN-1096) | M (3) | — |
| `runtime-security:R11` | Exchange floating-point buffers with JavaScript | approved | [PIN-1097](https://linear.app/pinguapps/issue/PIN-1097) | L (5) | — |
| `runtime-security:R12` | Retain native exports with their source types | approved | [PIN-1098](https://linear.app/pinguapps/issue/PIN-1098) | L (5) | — |
| `runtime-security:R13` | Generate COM property contracts | approved | [PIN-1099](https://linear.app/pinguapps/issue/PIN-1099) | L (5) | [PIN-696](https://linear.app/pinguapps/issue/PIN-696) |
| `runtime-security:R14` | Migrate custom browser runtime hosts | approved | [PIN-1100](https://linear.app/pinguapps/issue/PIN-1100) | L (5) | — |
| `runtime-security:R15` | Select cryptographic providers for key operations | approved | [PIN-1101](https://linear.app/pinguapps/issue/PIN-1101) | L (5) | — |
| `runtime-security:R16` | Inspect an encryption instance's key size | approved | [PIN-1102](https://linear.app/pinguapps/issue/PIN-1102) | S (2) | — |
| `runtime-security:R17` | Remove a verified manual Windows-authentication downgrade | approved | [PIN-1103](https://linear.app/pinguapps/issue/PIN-1103) | L (5) | [PIN-680](https://linear.app/pinguapps/issue/PIN-680) |
| `runtime-security:R18` | Resolve target names through a SOCKS proxy | approved | [PIN-1104](https://linear.app/pinguapps/issue/PIN-1104) | M (3) | — |
| `runtime-security:R19` | Map file extensions and content types | approved | [PIN-1105](https://linear.app/pinguapps/issue/PIN-1105) | S (2) | — |
| `runtime-security:R20` | Replace synchronous mobile HTTP sends | approved | [PIN-1106](https://linear.app/pinguapps/issue/PIN-1106) | L (5) | [PIN-697](https://linear.app/pinguapps/issue/PIN-697) |
| `runtime-security:R21` | Decode Zstandard HTTP responses | approved | [PIN-1107](https://linear.app/pinguapps/issue/PIN-1107) | M (3) | — |
| `runtime-security:R22` | Allocate aligned native memory with type lifetime | approved | [PIN-1108](https://linear.app/pinguapps/issue/PIN-1108) | M (3) | — |
| `runtime-security:R23` | Choose Unix named-mutex scope | approved | [PIN-1109](https://linear.app/pinguapps/issue/PIN-1109) | L (5) | — |
| `runtime-security:R24` | Bound a 32-bit managed heap | approved | [PIN-1110](https://linear.app/pinguapps/issue/PIN-1110) | M (3) | — |
| `runtime-security:R25` | Bound XML signature references | approved | [PIN-1111](https://linear.app/pinguapps/issue/PIN-1111) | M (3) | — |

### extensions

[PIN-706](https://linear.app/pinguapps/issue/PIN-706) · [source drafts and shared acceptance](audits/extensions.md) · 6 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `extensions:E1` | Map environment-variable names to intended configuration keys | approved | [PIN-1112](https://linear.app/pinguapps/issue/PIN-1112) | M (3) | — |
| `extensions:E2` | Supply a provider without application services | approved | [PIN-1113](https://linear.app/pinguapps/issue/PIN-1113) | S (2) | — |
| `extensions:E3` | Reference Extensions correctly after shared-framework inclusion | approved | [PIN-1114](https://linear.app/pinguapps/issue/PIN-1114) | M (3) | — |
| `extensions:E4` | Migrate an authored validation resolver to the new metadata contracts | approved | [PIN-1115](https://linear.app/pinguapps/issue/PIN-1115) | M (3) | — |
| `extensions:E5` | Give a dynamic event provider an explicit identity | approved | [PIN-1116](https://linear.app/pinguapps/issue/PIN-1116) | S (2) | — |
| `extensions:E6` | Preserve trace randomness metadata independently of recording | approved | [PIN-1117](https://linear.app/pinguapps/issue/PIN-1117) | M (3) | — |

### sdk-build

[PIN-707](https://linear.app/pinguapps/issue/PIN-707) · [source drafts and shared acceptance](audits/sdk-build.md) · 29 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `sdk-build:B01` | Bind typed parameters in a safe custom build task | approved | [PIN-1118](https://linear.app/pinguapps/issue/PIN-1118) | L (5) | — |
| `sdk-build:B02` | Evaluate only the project information a tool needs | approved | [PIN-1119](https://linear.app/pinguapps/issue/PIN-1119) | M (3) | — |
| `sdk-build:B03` | Register a closed set of build extensions in an AOT host | approved | [PIN-1120](https://linear.app/pinguapps/issue/PIN-1120) | L (5) | — |
| `sdk-build:B04` | Consume structured assembly-conflict and server events | approved | [PIN-1121](https://linear.app/pinguapps/issue/PIN-1121) | M (3) | — |
| `sdk-build:B05` | Migrate a custom MSBuild server client | approved | [PIN-1122](https://linear.app/pinguapps/issue/PIN-1122) | M (3) | — |
| `sdk-build:B06` | Read experimental build file-access records correctly | approved | [PIN-1123](https://linear.app/pinguapps/issue/PIN-1123) | M (3) | — |
| `sdk-build:B07` | Upload packages to a V3 staging endpoint | approved | [PIN-1124](https://linear.app/pinguapps/issue/PIN-1124) | L (5) | — |
| `sdk-build:B08` | Invalidate custom NuGet caches between reused builds | approved | [PIN-1125](https://linear.app/pinguapps/issue/PIN-1125) | M (3) | — |
| `sdk-build:B09` | Inspect restore's analyzer assets without assuming execution | approved | [PIN-1126](https://linear.app/pinguapps/issue/PIN-1126) | M (3) | — |
| `sdk-build:B10` | Remove dependency on an obsolete credential-plugin diagnostic helper | approved | [PIN-1127](https://linear.app/pinguapps/issue/PIN-1127) | S (2) | — |
| `sdk-build:B11` | Replace removed VSTest session orchestration | approved | [PIN-1128](https://linear.app/pinguapps/issue/PIN-1128) | L (5) | — |
| `sdk-build:B12` | Construct immutable VSTest assembly-resolution context | approved | [PIN-1129](https://linear.app/pinguapps/issue/PIN-1129) | S (2) | — |
| `sdk-build:B13` | Pass custom VSTest runsettings through the current task contract | approved | [PIN-1130](https://linear.app/pinguapps/issue/PIN-1130) | S (2) | — |
| `sdk-build:B14` | Check the resolved SDK's vulnerability and EOL status | approved | [PIN-1131](https://linear.app/pinguapps/issue/PIN-1131) | M (3) | — |
| `sdk-build:B15` | Publish a selected partial ReadyToRun set | approved | [PIN-1132](https://linear.app/pinguapps/issue/PIN-1132) | L (5) | — |
| `sdk-build:B16` | Supply a deliberate native-tool package RID matrix | approved | [PIN-1133](https://linear.app/pinguapps/issue/PIN-1133) | L (5) | — |
| `sdk-build:B17` | Avoid writing a disposable CI restore graph | approved | [PIN-1134](https://linear.app/pinguapps/issue/PIN-1134) | S (2) | — |
| `sdk-build:B18` | Include referenced symbols in published output | approved | [PIN-1135](https://linear.app/pinguapps/issue/PIN-1135) | S (2) | — |
| `sdk-build:B19` | Inspect the actual build import tree | approved | [PIN-1136](https://linear.app/pinguapps/issue/PIN-1136) | S (2) | — |
| `sdk-build:B20` | Verify the signature of a selected .NET base image | approved | [PIN-1137](https://linear.app/pinguapps/issue/PIN-1137) | M (3) | — |
| `sdk-build:B21` | Migrate an authored Azure Linux base-image selection | approved | [PIN-1138](https://linear.app/pinguapps/issue/PIN-1138) | L (5) | — |
| `sdk-build:B22` | Consume VSTest collector events without mutating removed setters | approved | [PIN-1139](https://linear.app/pinguapps/issue/PIN-1139) | M (3) | — |
| `sdk-build:B23` | Resolve workloads using an explicit host environment | approved | [PIN-1140](https://linear.app/pinguapps/issue/PIN-1140) | M (3) | — |
| `sdk-build:B24` | Migrate custom dependency-file task inputs | approved | [PIN-1141](https://linear.app/pinguapps/issue/PIN-1141) | M (3) | — |
| `sdk-build:B25` | Choose console encoding for redirected SDK output | approved | [PIN-1142](https://linear.app/pinguapps/issue/PIN-1142) | M (3) | — |
| `sdk-build:B26` | Create an MCP server from the bundled template | grouped | [PIN-819](https://linear.app/pinguapps/issue/PIN-819) | M (3) | — |
| `sdk-build:B27` | Configure a registry authentication challenge that the SDK accepts | approved | [PIN-1143](https://linear.app/pinguapps/issue/PIN-1143) | M (3) | — |
| `sdk-build:B28` | Preserve custom hooks in optimized native publishing | approved | [PIN-1144](https://linear.app/pinguapps/issue/PIN-1144) | M (3) | — |
| `sdk-build:B29` | Select the intended public API comparison surface | approved | [PIN-1145](https://linear.app/pinguapps/issue/PIN-1145) | M (3) | — |

### blazor-removed-apis

[PIN-691](https://linear.app/pinguapps/issue/PIN-691) · [source drafts and shared acceptance](audits/blazor-removed-apis.md) · 9 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `blazor-removed-apis:R1` | Keep route matching after removing the inert router setting | approved | [PIN-1146](https://linear.app/pinguapps/issue/PIN-1146) | S (2) | — |
| `blazor-removed-apis:R2` | Keep service-backed validation when migrating an EditContext | approved | [PIN-1147](https://linear.app/pinguapps/issue/PIN-1147) | M (3) | — |
| `blazor-removed-apis:R3` | Bound an uploaded file without obsolete stream options | approved | [PIN-1148](https://linear.app/pinguapps/issue/PIN-1148) | M (3) | — |
| `blazor-removed-apis:R4` | Build typed event callbacks without removed extensions | approved | [PIN-1149](https://linear.app/pinguapps/issue/PIN-1149) | S (2) | — |
| `blazor-removed-apis:R5` | Allocate a custom web renderer's identity through its supported hook | approved | [PIN-1150](https://linear.app/pinguapps/issue/PIN-1150) | M (3) | — |
| `blazor-removed-apis:R6` | Preserve WebAssembly navigation state through supported application APIs | approved | [PIN-1151](https://linear.app/pinguapps/issue/PIN-1151) | M (3) | — |
| `blazor-removed-apis:R7` | Initiate WebAssembly logout through supported navigation | approved | [PIN-1152](https://linear.app/pinguapps/issue/PIN-1152) | L (5) | — |
| `blazor-removed-apis:R8` | Construct a custom remote authentication service with logging | approved | [PIN-1153](https://linear.app/pinguapps/issue/PIN-1153) | M (3) | — |
| `blazor-removed-apis:R9` | Resume token acquisition without legacy redirect members | approved | [PIN-1154](https://linear.app/pinguapps/issue/PIN-1154) | M (3) | — |

### blazor

[PIN-708](https://linear.app/pinguapps/issue/PIN-708) · [source drafts and shared acceptance](audits/blazor.md) · 16 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `blazor:B01` | Expand asset paths through reusable markup | approved | [PIN-1155](https://linear.app/pinguapps/issue/PIN-1155) | M (3) | — |
| `blazor:B02` | Supply scoped values through a custom cascading attribute | approved | [PIN-1156](https://linear.app/pinguapps/issue/PIN-1156) | M (3) | — |
| `blazor:B03` | Reuse a base-item rendering fragment for derived items | approved | [PIN-1157](https://linear.app/pinguapps/issue/PIN-1157) | S (2) | — |
| `blazor:B04` | Replace a staged renderer attribute deliberately | approved | [PIN-1158](https://linear.app/pinguapps/issue/PIN-1158) | M (3) | — |
| `blazor:B05` | Choose metadata for application-owned circuit JSON | approved | [PIN-1159](https://linear.app/pinguapps/issue/PIN-1159) | M (3) | — |
| `blazor:B06` | Connect a custom server form rule to browser validation | approved | [PIN-1160](https://linear.app/pinguapps/issue/PIN-1160) | M (3) | — |
| `blazor:B07` | Declare how a reusable child behaves inside a cached section | approved | [PIN-1161](https://linear.app/pinguapps/issue/PIN-1161) | M (3) | — |
| `blazor:B08` | Let component events schedule their own rerender | approved | [PIN-1162](https://linear.app/pinguapps/issue/PIN-1162) | S (2) | — |
| `blazor:B09` | Bind each repeated action to its own item | approved | [PIN-1163](https://linear.app/pinguapps/issue/PIN-1163) | S (2) | — |
| `blazor:B10` | Expose an intended JavaScript callback with the right visibility | approved | [PIN-1164](https://linear.app/pinguapps/issue/PIN-1164) | S (2) | — |
| `blazor:B11` | Handle JavaScript invocation failures without hiding application errors | approved | [PIN-1165](https://linear.app/pinguapps/issue/PIN-1165) | M (3) | — |
| `blazor:B12` | Keep a custom Razor tool constructing and reading generated documents | approved | [PIN-1166](https://linear.app/pinguapps/issue/PIN-1166) | M (3) | — |
| `blazor:B13` | Copy intermediate Razor nodes before a custom transformation | approved | [PIN-1167](https://linear.app/pinguapps/issue/PIN-1167) | M (3) | — |
| `blazor:B14` | Select the hosted generator in an authored Razor build task | approved | [PIN-1168](https://linear.app/pinguapps/issue/PIN-1168) | M (3) | — |
| `blazor:B15` | Clean up resources owned by an InputFile subclass | approved | [PIN-1169](https://linear.app/pinguapps/issue/PIN-1169) | S (2) | — |
| `blazor:B16` | Restore a completed agent conversation after restarting its view | approved; availability gated | [PIN-1170](https://linear.app/pinguapps/issue/PIN-1170) | M (3) | — |

### web

[PIN-709](https://linear.app/pinguapps/issue/PIN-709) · [source drafts and shared acceptance](audits/web.md) · 16 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `web:W01` | Protect data directly into an owned buffer | approved | [PIN-1171](https://linear.app/pinguapps/issue/PIN-1171) | M (3) | — |
| `web:W02` | Supply an application-specific cross-origin verdict | approved | [PIN-1172](https://linear.app/pinguapps/issue/PIN-1172) | L (5) | — |
| `web:W03` | Read parsed route contracts in an API explorer extension | approved | [PIN-1173](https://linear.app/pinguapps/issue/PIN-1173) | M (3) | — |
| `web:W04` | Generate contracts for dynamically named API documents | approved | [PIN-1174](https://linear.app/pinguapps/issue/PIN-1174) | M (3) | — |
| `web:W05` | Query an HTTP.sys request property safely | approved | [PIN-1175](https://linear.app/pinguapps/issue/PIN-1175) | L (5) | [PIN-680](https://linear.app/pinguapps/issue/PIN-680) |
| `web:W06` | Give a generated static endpoint deliberate route precedence | approved | [PIN-1176](https://linear.app/pinguapps/issue/PIN-1176) | M (3) | — |
| `web:W07` | Keep registration aligned with confirmation policy | approved | [PIN-1177](https://linear.app/pinguapps/issue/PIN-1177) | M (3) | — |
| `web:W08` | Move customized Identity UI off Bootstrap 4 | approved | [PIN-1178](https://linear.app/pinguapps/issue/PIN-1178) | M (3) | — |
| `web:W09` | Preserve encoded route segments in proxy-style requests | approved | [PIN-1179](https://linear.app/pinguapps/issue/PIN-1179) | M (3) | — |
| `web:W10` | Replace the retired gRPC Swagger integration | approved | [PIN-1180](https://linear.app/pinguapps/issue/PIN-1180) | L (5) | — |
| `web:W11` | Select an available direct TLS transport | approved; availability gated | [PIN-1181](https://linear.app/pinguapps/issue/PIN-1181) | L (5) | — |
| `web:W12` | Configure HTTP/3 connection budgets without preview scaffolding | approved | [PIN-1182](https://linear.app/pinguapps/issue/PIN-1182) | M (3) | — |
| `web:W13` | Preserve JSON Patch operation serialization intent | approved | [PIN-1183](https://linear.app/pinguapps/issue/PIN-1183) | M (3) | — |
| `web:W14` | Configure a web integration test through the application builder | approved | [PIN-1184](https://linear.app/pinguapps/issue/PIN-1184) | M (3) | — |
| `web:W15` | Remove reliance on obsolete browser token binding | approved | [PIN-1185](https://linear.app/pinguapps/issue/PIN-1185) | S (2) | — |
| `web:W16` | Publish F# MVC views without runtime compilation | approved | [PIN-1186](https://linear.app/pinguapps/issue/PIN-1186) | M (3) | — |

### mobile-api-gaps

[PIN-694](https://linear.app/pinguapps/issue/PIN-694) · [source drafts and shared acceptance](audits/mobile-api-gaps.md) · 2 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `mobile-api-gaps:A` | Edit nullable values with an explicit converter culture | approved | [PIN-1187](https://linear.app/pinguapps/issue/PIN-1187) | L (5) | [PIN-696](https://linear.app/pinguapps/issue/PIN-696) |
| `mobile-api-gaps:B` | Handle absent results and errors from native bindings | approved | [PIN-1188](https://linear.app/pinguapps/issue/PIN-1188) | L (5) | [PIN-698](https://linear.app/pinguapps/issue/PIN-698) |

### ef

[PIN-710](https://linear.app/pinguapps/issue/PIN-710) · [source drafts and shared acceptance](audits/ef.md) · 31 entries.

| Original draft | Reusable goal | Decision | Retained ticket | Native estimate | Pending native prerequisites |
| --- | --- | --- | --- | --- | --- |
| `ef:E1` | Select tracked entries without an extra detection pass | approved | [PIN-1189](https://linear.app/pinguapps/issue/PIN-1189) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E2` | Replace a configured database provider deliberately | approved | [PIN-1190](https://linear.app/pinguapps/issue/PIN-1190) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E3` | Reuse shared configuration in a pooled factory | approved | [PIN-1191](https://linear.app/pinguapps/issue/PIN-1191) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E4` | Avoid saving a scalar value that was not loaded | approved | [PIN-1192](https://linear.app/pinguapps/issue/PIN-1192) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E5` | Preserve relational automatic-transaction intent | approved | [PIN-1193](https://linear.app/pinguapps/issue/PIN-1193) | M (3) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E6` | Preserve collection-parameter translation configuration | approved | [PIN-1194](https://linear.app/pinguapps/issue/PIN-1194) | M (3) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E7` | Migrate raw SQL calls while preserving parameters | approved | [PIN-1195](https://linear.app/pinguapps/issue/PIN-1195) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E8` | Replace the removed Azure SQL defaults helper | approved | [PIN-1196](https://linear.app/pinguapps/issue/PIN-1196) | M (3) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E9` | Preserve partition keys when replacing singular Cosmos helpers | approved | [PIN-1197](https://linear.app/pinguapps/issue/PIN-1197) | M (3) | [PIN-677](https://linear.app/pinguapps/issue/PIN-677) |
| `ef:E10` | Remove ineffective EF write-content configuration | approved | [PIN-1198](https://linear.app/pinguapps/issue/PIN-1198) | S (2) | [PIN-677](https://linear.app/pinguapps/issue/PIN-677) |
| `ef:E11` | Migrate a custom scalar type mapping and comparer | approved | [PIN-1199](https://linear.app/pinguapps/issue/PIN-1199) | L (5) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E12` | Generate partial JSON updates using structured paths | approved | [PIN-1200](https://linear.app/pinguapps/issue/PIN-1200) | L (5) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E13` | Replace removed value-generator selectors | approved | [PIN-1201](https://linear.app/pinguapps/issue/PIN-1201) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E14` | Replace legacy materializer/query-root extension hooks | approved | [PIN-1202](https://linear.app/pinguapps/issue/PIN-1202) | L (5) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E15` | Retain rows whose modeled principal may be missing | approved | [PIN-1203](https://linear.app/pinguapps/issue/PIN-1203) | M (3) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E16` | Migrate a custom constructor-binding consumer | approved | [PIN-1204](https://linear.app/pinguapps/issue/PIN-1204) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E17` | Inspect mapped columns across entity and complex paths | approved | [PIN-1205](https://linear.app/pinguapps/issue/PIN-1205) | L (5) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E18` | Migrate a custom type-ignore convention | approved | [PIN-1206](https://linear.app/pinguapps/issue/PIN-1206) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E19` | Run EF design-time commands for a file-based app | approved | [PIN-1207](https://linear.app/pinguapps/issue/PIN-1207) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E20` | Keep collection-existence predicates on the database | approved | [PIN-1208](https://linear.app/pinguapps/issue/PIN-1208) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864), [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E21` | Parse stored numeric text inside a SQL query | approved | [PIN-1209](https://linear.app/pinguapps/issue/PIN-1209) | M (3) | [PIN-676](https://linear.app/pinguapps/issue/PIN-676) |
| `ef:E22` | Filter SQLite rows by time components | approved | [PIN-1210](https://linear.app/pinguapps/issue/PIN-1210) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E23` | Store half-precision values in SQLite | approved | [PIN-1211](https://linear.app/pinguapps/issue/PIN-1211) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E24` | Compose database queries over tuple-valued projections | approved | [PIN-1212](https://linear.app/pinguapps/issue/PIN-1212) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E25` | Preserve missing non-entity join results | approved | [PIN-1213](https://linear.app/pinguapps/issue/PIN-1213) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E26` | Produce a migration bundle with central package management | approved | [PIN-1214](https://linear.app/pinguapps/issue/PIN-1214) | L (5) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E27` | Run design-time tooling with customized output names | approved | [PIN-1215](https://linear.app/pinguapps/issue/PIN-1215) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E28` | Reconcile a snapshot made by an older EF version | approved | [PIN-1216](https://linear.app/pinguapps/issue/PIN-1216) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |
| `ef:E29` | Keep keys in Cosmos identity-resolution projections | approved | [PIN-1217](https://linear.app/pinguapps/issue/PIN-1217) | M (3) | [PIN-677](https://linear.app/pinguapps/issue/PIN-677) |
| `ef:E30` | Compose Cosmos numeric conversions in server queries | approved | [PIN-1218](https://linear.app/pinguapps/issue/PIN-1218) | M (3) | [PIN-677](https://linear.app/pinguapps/issue/PIN-677) |
| `ef:E31` | Select generated model code using explicit options | approved | [PIN-1219](https://linear.app/pinguapps/issue/PIN-1219) | M (3) | [PIN-864](https://linear.app/pinguapps/issue/PIN-864) |

## Retained qualifications and other audit outcomes

These are bounded source/availability questions, not silently excluded recipes.
The named owner and PIN-959 must keep them visible until the relevant qualifying
coverage is resolved. Their existence does not fabricate a supported consumer or
expand a concrete delivery slice into an arbitrary API catalogue.

| Evidence / remaining question | Retained ownership and decision |
| --- | --- |
| Compiler main versus RC2 provenance, experimental markers and still-unqualified generator/debugger/checksum/analyzer API workflows | [PIN-682 audit](audits/compiler-contracts.md), [PIN-961](https://linear.app/pinguapps/issue/PIN-961) and [PIN-959](https://linear.app/pinguapps/issue/PIN-959); five supported draft boundaries tracked above. Residual source rows stay unresolved, not approved invented APIs. |
| Runtime U1/U2 native ABI, profiler/cDAC/Mono/perf-map and other unqualified external consumer routes | [PIN-705 audit](audits/runtime-security.md), [PIN-962](https://linear.app/pinguapps/issue/PIN-962) and [PIN-959](https://linear.app/pinguapps/issue/PIN-959); R9–R15 retain their exact native consumer proof. Other residuals need factual qualification before another bounded recipe is justified. |
| SDK U1–U5 public host/extension support, build XML interpretation and already-serviced package introduction | [PIN-707 audit](audits/sdk-build.md), [PIN-963](https://linear.app/pinguapps/issue/PIN-963) and [PIN-959](https://linear.app/pinguapps/issue/PIN-959); supported draft boundaries are tracked, implementation helpers are not pronounced automatic or compatible. |
| EF U1/U3 residual annotations and model/convention/query/scaffolding/provider-test extension contracts | [PIN-710 audit](audits/ef.md), [PIN-965](https://linear.app/pinguapps/issue/PIN-965) and [PIN-959](https://linear.app/pinguapps/issue/PIN-959); E11–18/E31 own their demonstrated consumers. Remaining raw groups are unresolved, not a semantic closure or a single unknown-sized implementation ticket. Any confirmed additional consumer needs a narrow sized delivery owner before comprehensive publication. |
| Direct-TLS/TypeScript final contracts and absent later AI UI APIs | [web audit](audits/web.md), [Blazor audit](audits/blazor.md), [PIN-964](https://linear.app/pinguapps/issue/PIN-964), [PIN-805](https://linear.app/pinguapps/issue/PIN-805), [PIN-806](https://linear.app/pinguapps/issue/PIN-806), [PIN-807](https://linear.app/pinguapps/issue/PIN-807) and the two availability-gated new tickets; actual package contracts and real checks remain required. |
| DBSC removed from the pinned RC2 artifacts | [availability audit](audits/dbsc-availability.md), Done [PIN-692](https://linear.app/pinguapps/issue/PIN-692); retain unavailable disposition and RC1 history. [PIN-964](https://linear.app/pinguapps/issue/PIN-964) must recheck final artifacts; no recipe for removed RC APIs. |
| WindowsDesktop public changes and exclusions | [desktop audit](audits/windowsdesktop.md), Done [PIN-693](https://linear.app/pinguapps/issue/PIN-693); no additional proposal. Existing [PIN-890](https://linear.app/pinguapps/issue/PIN-890)–[PIN-896](https://linear.app/pinguapps/issue/PIN-896) and Human [PIN-897](https://linear.app/pinguapps/issue/PIN-897)/[PIN-898](https://linear.app/pinguapps/issue/PIN-898) retain delivery/proof ownership. |
| Mobile composition, native-only exclusions and matched toolchain/status constraints | [composition audit](audits/mobile-baselines.md), [API audit](audits/mobile-api-gaps.md), Done [PIN-695](https://linear.app/pinguapps/issue/PIN-695)/[PIN-694](https://linear.app/pinguapps/issue/PIN-694); two new delivery slices above, existing [PIN-903](https://linear.app/pinguapps/issue/PIN-903)/[PIN-913](https://linear.app/pinguapps/issue/PIN-913)/[PIN-932](https://linear.app/pinguapps/issue/PIN-932) and [PIN-966](https://linear.app/pinguapps/issue/PIN-966) retain their separate scopes. Native null-handling checks do not establish real NFC/device operations. |
| Obsolete-index rows and partial automatic-exclusion reversals | [ownership map](audits/obsolete-index.md), [exclusion recheck](audits/automatic-exclusions.md); pipe [PIN-741](https://linear.app/pinguapps/issue/PIN-741), crypto E/F and three explicit exclusion-reversal tickets retain ownership. Automatic residuals stay excluded for the source-backed reasons. |
| Unix static-library output naming | [native deployment audit](audits/native-deployment.md), [PIN-963](https://linear.app/pinguapps/issue/PIN-963) and [PIN-959](https://linear.app/pinguapps/issue/PIN-959); static naming is source-confirmed, supported static publication remains unresolved. The new shared-library ticket requires real Linux/macOS consumption and does not invent static support. |

Actual final-release comparisons remain [PIN-961](https://linear.app/pinguapps/issue/PIN-961), [PIN-962](https://linear.app/pinguapps/issue/PIN-962), [PIN-963](https://linear.app/pinguapps/issue/PIN-963), [PIN-964](https://linear.app/pinguapps/issue/PIN-964), [PIN-965](https://linear.app/pinguapps/issue/PIN-965), [PIN-966](https://linear.app/pinguapps/issue/PIN-966).
Assigning a final recheck does not waive a current qualifying coverage gap or allow
PIN-959 to claim comprehensive readiness from counts, Done audits or RC metadata.

## Sources, drift and verification boundary

Consulted: PIN-986 and source audit descriptions/comments; the explicit immutable
PIN-708 report at `24b399868e337d0e02afc55d8b23d5c94d93dfad` compared with current
delivered corrections; SPEC.md; planning README, coverage/inventory/verification
plans; ADRs 0001–0004; contribution policy; current recipe and sample-evidence
pointers. Optional root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent.
The original scope request is represented by PIN-986's provenance and PIN-708's
handoff; its separate original chat transcript was not supplied. Shared Linear
agent notes returned `Tool get_agent_context not found`; repository evidence and
scoped tracker reads supplied the usable context.

The current report supersedes old “awaiting approval” statements and provisional
prefixes in the historical audits. Historical statements that the host or a
subsequently delivered recipe does not exist are not current repository truth.
The three registers preserve fictional original examples, canonical source,
conditional applicability, exact eight foundations, deliberate experimental
inclusion and separate actual GA evidence.

Ticket read-back: **188/188 verified** on 26 October 2026. All new
identities, stories/scope, milestones/prefixes, labels, native estimates, Backlog
states, unassigned fields and source/publication/prerequisite edges passed live
read-back. Linear's issue mentions, automatic links and Markdown styling are
normalized only for content comparison. Any additional concurrent links are
preserved. Full live receipts stay in ignored artifacts rather than this record.

The repeatable local check is `eng/Test-AuditProposalReview.ps1`: it compares
current audit draft keys with this mapping, rejects missing/duplicate/unowned rows
and validates local source links and native-scale entries. It is an ongoing input
check for PIN-959, not a runtime/recipe test or proof of future live tracker state.
