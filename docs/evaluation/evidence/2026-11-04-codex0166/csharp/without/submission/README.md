# Synchronization outcomes

`SyncOutcome` is a closed value type wrapping the existing, unchanged `Applied`,
`Unchanged`, and `Rejected` records. Only those records convert to an outcome.
Its private constructors permit exactly one case; `default(SyncOutcome)` and
`new SyncOutcome()` represent absence. Null records and null payloads are rejected;
empty string payloads are preserved.

`SyncWorkflow.Apply`, `Skip`, and `Reject` return `SyncOutcome`. `Describe`,
`Serialize`, and `Deserialize` now consume or return that type while keeping the
original method and parameter names. `Match` requires all three case handlers and
an absence handler. Rejection has its own branch throughout, including persistence.

The JSON format is exactly one of:

```json
{"case":"applied","revision":"r17"}
{"case":"unchanged","revision":"r18"}
{"case":"rejected","reason":"station offline"}
null
```

Property order is immaterial. Case names and property names are case sensitive.
Unknown cases, duplicate properties, extra properties, missing or mismatched
payloads, non-string payloads, invalid roots, comments, trailing commas, and
malformed JSON throw `JsonException`. The converter is attached to the outcome
type, so direct `System.Text.Json` serialization also preserves cases and absence.

The compiler guarantee comes from the closed construction API and required
`Match` parameters. This is an ordinary C# struct, not a compiler-recognized
discriminated union: C# does not prove arbitrary switches over it exhaustive, nor
does it prove that a handler does the right thing. Omitting a `Match` handler is
a compile error; passing a null delegate throws even when that branch is inactive.
Adding a future case would require updating the API and converter deliberately.

Run the checks using the supplied pinned SDK without changing PATH or persistent
environment settings:

```powershell
& 'C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe' -NoProfile -ExecutionPolicy Bypass -File ./Check.ps1
```

The script invokes the pinned SDK's local MSBuild assembly with the existing
restore assets, avoiding a CLI network lookup blocked in this sandbox. It builds
with the original net11.0, C# 15.0, nullable, and warnings-as-errors settings, runs
the demonstration and runtime checks, then compiles `CompileFailures.cs.txt`
separately and requires diagnostics for unrelated values and omitted rejection
and absence handlers. The intentionally invalid source is excluded from the
normal build by its `.txt` extension. No packages are added.
