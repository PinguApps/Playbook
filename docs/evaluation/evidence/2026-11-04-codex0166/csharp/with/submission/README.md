# Synchronization outcomes

`SyncOutcome` is a C# 15 union of the unchanged, unrelated `Applied`, `Unchanged`
and `Rejected` records. The workflow factories and consumers use that closed type.
There is no object overload or conversion from unrelated values. `Describe`
handles each record and default storage explicitly; default is `absent`.

JSON uses an explicit, case-sensitive tag because `Applied` and `Unchanged` both
carry `Revision` and cannot be distinguished by property names alone:

```json
{"case":"applied","revision":"r17"}
{"case":"unchanged","revision":"r17"}
{"case":"rejected","reason":"sensor offline"}
null
```

These are separate example documents. The converter requires exactly the tag
and the matching non-null string payload. It rejects unknown cases/properties,
duplicate properties (including duplicate tags), conflicting case payloads,
missing fields, wrong JSON types, and malformed JSON. Empty strings are valid;
no additional domain constraint was specified. Invalid persistence throws
`JsonException`; it never becomes a success. JSON null alone restores default.
The same converter applies to direct `JsonSerializer` use.

## Verification

From this directory run:

```powershell
& 'C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe' -NoProfile -ExecutionPolicy Bypass -File ./Verify.ps1
```

The script uses the supplied absolute SDK path and its managed MSBuild entry
point, preserving global.json, net11.0, C# 15.0 and warnings-as-errors. The CLI
`dotnet build` launcher attempted a blocked connection to
`builds.dotnet.microsoft.com`; invoking that SDK's MSBuild.dll directly works
offline. No packages or SDK installation are needed.

The executable checks every case, exact descriptions, null/default, payload
escaping/Unicode/empty strings, round trips, 22 invalid persisted inputs and
missing-case failures. Compiler probes under `obj/CompilerChecks` check that
an omitted `Rejected` arm produces CS8509 and an unrelated string cannot convert
to `SyncOutcome` (CS0029).

The pinned RC3 compiler **does not diagnose an omitted null/default arm** when
all three record cases are present. The script verifies this limitation and the
executable demonstrates the resulting `SwitchExpressionException` for default
storage. Production switches include the null arm. Catch-all arms also suppress
missing-case diagnostics; the demonstration throws explicitly from such an arm
instead of treating a rejection as success. Exhaustiveness analysis does not
validate payloads or persisted input. This is verification on the supplied RC3
SDK, not a claim about a final release or other compilers.

Before implementation, Playbook recipes `csharp.closed-results` and
`json.serialize-closed-results` were retrieved for target release 11. Their
native union/null guidance applies here. Their structural-classifier example
does not distinguish these two revision cases, so this implementation uses the
documented explicit-converter option for selection by tag value.
