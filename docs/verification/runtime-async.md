# Runtime async evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-757-980-981.md).

## Measured source differences and limits

- The public RC2 SDK's bare `UseRuntimeAsync=true` does not enable C# runtime async.
  Explicit `Features=runtime-async=on/off` does change emitted lowering. The
  property is meaningful in upstream repositories that translate it themselves;
  the recipe does not silently invent that mapping for a consumer.
- Runtime async reports cleaner live frames in this bounded observation. Both
  thrown-exception variants preserve the originating survey method; there is no
  universal stack/debugger guarantee. Removed environment variables set to zero
  do not disable already marked methods.
- Actual lifetime rejection is FS3919, while the matching F# document says FS3917.
  A pending generic Task upcast to non-generic Task produced InvalidProgramException
  during authoring. The bounded supported unit sample uses a real non-generic Task;
  the limitation remains explicit in discovery/detail. No AOT or production-complete
  builder is claimed. No unsupported path is presented as a verified route.
- The .NET 10 reference pack exposes some experimental runtime-async metadata.
  This does not prove a supported older application/runtime route; none is offered.
  The negative runtime-capability check deliberately uses .NET 9 references.
- FCS opt-out is a hosted correctness/configuration decision, not a memory cap or
  universal benchmark claim. No public ClearCaches method is advertised.

Saved evidence/locks are small and meaningful review inputs, so the
`pragent-ignore` assessment keeps them reviewable. No exclusion rules change.
Compilation/execution are real managed checks, not mocks or cross-compilation.
Native AOT, IDE stepping, browser/Mono, other-platform execution and final GA proof
are outside the claims. No missing targeted Human proof remains for these claims.
