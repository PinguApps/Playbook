# Redirect and session evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-776.md).

## Reconciliation and decisions

The original harbour permit example uses .NET 10's valid MVC TempData bridge and
explicit ISession, versus .NET 11 ITempData and supplied properties. No guide
implementation was copied. SDK pins/language modes are evidence baselines;
consumer requirements use release families where appropriate. Both APIs are
stable contracts with RC2 execution, no experimental opt-in and GA pending.

**Measured RC2 behavior:** an unchanged SupplyParameterFromTempData property
writes its received value back, repeating the message on the second request.
The reader copies the value for display and sets the bound property to null.
The initial failing sequential read reproduced this issue; the final checks
prove one-use display with cookie and session-storage TempData. The pinned
TempDataCascadingValueSupplier.PersistValues source corroborates the write-back.
Direct ITempData reads consume unless Peek/Keep is chosen deliberately.

Session values survive reads. Static SSR requires both session services and
middleware. An early subscription establishes a fresh cookie before streaming;
the late value is saved after rendering. Cookie TempData cannot persist late
writes; the session-storage provider can, with early session setup. These are
explicit applicability constraints, not extra recipes per overload.

Samples inherit only samples/Directory.Build.props and Directory.Packages.props,
not root tool packages. Web SDK Internal.Assets versions are locked at 10.0.13
and 11.0.0-rc.2.26504.105; no new PackageReference or automatic Renovate permission
is added. Root production host/test targets remain net10.0 and workload-free.
The test project embeds new canonical inputs only. All evidence/lockfiles are
small and reviewable; pragent-ignore assessment adds no exclusion.

## Actual evidence and limits

- Both projects compile with zero warnings/errors: SDK 10.0.402/C# 14.0 and
  11.0.100-rc.2.26504.105/C# 15.0. Their verification-evidence.json records are
  **compile**, with exact inputs, dependencies and SDK output.
- [Real Kestrel execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-776.http.evidence.json) covers the older bridge,
  newer cookie provider, newer session-storage provider and absent middleware.
  It records runtime versions, input and DLL/deps/runtimeconfig hashes.
- Actual antiforgery-bearing form POSTs return 302; first GET displays the
  confirmation, second GET shows No new outcome. Session progresses from
  North-12/step 1 through repeated reads to East-4/step 2. A separate cookie jar
  starts empty. This proves sequential consumption, not concurrent exactly-once
  delivery or atomic session updates.
- A held /stream returns Inspection pending and a session Set-Cookie before
  release. After release it renders Inspection saved with Headers started: True.
  The next review reads step 7. Late cookie TempData is absent (the real server
  also logs its streaming warning); session-backed TempData is read once.
- A cold /late-session request flushes headers before Session.Set and observes
  new-session-rejected-after-headers. The SkipSession=true app returns 500 for
  /progress, proving missing middleware fails rather than inventing progress.
- [Pinned official sources](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-776.sources.json) are source-review evidence,
  separate from compilation/execution. No mock or cross-compilation stands in
  for HTTP behavior. Browser UI, interactive activation, distributed cache,
  concurrent conflict handling and final GA are not claimed. No Human-only proof
  remains required for these scoped static SSR claims.
