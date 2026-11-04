I’ll inspect the project and consult Playbook, then implement and check the outcome model using the pinned SDK and existing project settings.

Playbook’s .NET 11/C# 15 union guidance matches this project. Applied and Unchanged share the same payload shape, so I’ll use an explicit JSON case tag and validate it strictly. That will preserve case identity and reject missing, duplicate, unknown, or conflicting fields.

The implementation is in place. The build is blocked before compilation by an attempted connection to `builds.dotnet.microsoft.com`. I’m checking whether the installed SDK can run fully offline while I add the executable demonstrations and rejection checks.

The app builds and passes the runtime checks through the pinned SDK’s managed MSBuild entry point. Compiler probes also found a limitation: omitting Rejected is diagnosed, but omitting the absent/default case compiles. I’ll document that gap and demonstrate the resulting runtime exception so callers know why the explicit `null` arm matters.

Implemented a closed `SyncOutcome` union while preserving the records and workflow method names.

- Strict tagged JSON preserves every case and payload; default round trips as `null` and describes as `absent`.
- Unknown, ambiguous, duplicate and malformed inputs are rejected.
- SDK, framework and language settings remain unchanged; no packages added.

`Verify.ps1` passed: build, all case/payload round trips, 22 invalid inputs, missing-case demonstrations and compiler probes.

Compiler limitation confirmed: omitting Rejected produces CS8509, but omitting absence compiles and throws at runtime. Production switches explicitly handle absence.

The CLI build launcher encountered blocked network access; the pinned SDK’s managed MSBuild entry point completed successfully. Nothing remains unfinished.
