I’ll inspect the records, workflow, and project settings, then implement a closed outcome type and check the cases, JSON round trips, and invalid inputs.

The project pins .NET 11 and C# 15. I’ll preserve the three records and wrap them in a closed `SyncOutcome` value type whose default value means absent. A `Match` method will require handlers for all four states, and strict JSON parsing will reject invalid or ambiguous inputs.

The implementation and checks are in place. The SDK command is being blocked while contacting `builds.dotnet.microsoft.com`, even with restore disabled. I’m checking how to run the local pinned toolchain without that lookup so I can verify the build.

The pinned toolchain built successfully with zero warnings, and all 62 runtime checks passed, including 27 invalid JSON inputs. Invoking its local MSBuild assembly avoided the blocked lookup. I’ll now check that the compiler rejects unrelated values and calls with missing handlers, and document the limits of that guarantee.

Implemented `SyncOutcome` with all three preserved records, explicit default absence, required case handlers, and strict JSON persistence.

Verified using the pinned SDK:

- Build passed with zero warnings or errors.
- All 62 runtime checks passed, including 27 invalid JSON inputs.
- Compiler checks rejected unrelated values and omitted handlers.

Compiler limitation: required `Match` parameters enforce handler coverage; arbitrary switches over the struct are not compiler-proven exhaustive.

SDK, framework, and language settings remain unchanged. No packages added. Nothing unfinished. Details and rerun instructions are in `README.md`.
