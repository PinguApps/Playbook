# Authorization and clock evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-809-810.md).

## Actual evidence

All four isolated consumers use SDK **10.0.402** / **11.0.100-rc.2.26504.105**,
C# **14.0** / **15.0**, and actual runtimes **10.0.13** /
**11.0.0-rc.2.26504.105**. Empty local build/package boundaries prevent root
analyzer/package inheritance. There are no PackageReferences or workloads;
matching ASP.NET Core framework packs supply the APIs. The explicit language
modes and SDK pins reproduce tests, rather than establishing minimum feature
requirements. Sample dependencies remain manually maintained; root automatic
update permissions did not change.

[Saved execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-809-810/execution.json) records matching-SDK builds, DLL
hashes and real outputs. Each sample also retains its independent current
`verification-evidence.json`: exact inputs, source commit/dirty marker, SDK,
language, dependency inventory and actual compilation. Compilation is not
relabeled as browser or database execution.

- **JettyAccess:** real ephemeral Kestrel requests return 403 for the denied
  authenticated user and 200 for the permitted user; anonymous API access is 401.
  Actual SignalR WebSocket method invocations return an authorization error or
  the manifest. Real `HtmlRenderer` output from the view adapter and
  `AuthorizeRouteView` agrees with those server decisions. The metadata helper
  verifies that the dual-interface attribute contributes one `CrewPermit`.
- **Legacy MVC control:** with `EnableEndpointRouting=false`, the .NET 11
  requirement attribute alone returns 200 to the denied authenticated user.
  The explicit named-policy fallback returns 403. The valid older named-policy
  comparison protects both MVC paths. This restriction is prominent in discovery,
  detail and source comments; the negative-control endpoint is not production code.
- **MooringClock:** actual `UserManager<IdentityUser>` locks after two failures
  and calculates the end from the injected clock. RC2 remains locked one tick
  before and exactly at the end, then unlocks one tick later without changing
  the stored timestamp. No sleep or mocked time-sensitive manager is involved.
  The actual .NET 10 control ignores clock advancement and instead tests an
  arranged past stored end, honestly distinguishing the older fixture technique.

The initial exclusive-end expectation failed; source review and execution both
established the inclusive boundary. A reused cascading authentication-state source
also initially retained the denied fixture state: separate rendering scopes fixed
that test setup. Compiler/analyzer findings and generated-lockfile CRLF were
corrected, then affected compilation proof was recaptured. None of those failed
attempts is counted as passing evidence.

`AuthorizeView.Policy` remains a policy-name string. The demonstrated `PermitView`
uses its protected `GetAuthorizeData` adapter; it does not invent an object-valued
parameter. The new `AuthorizationPolicy.CombineAsync` metadata overload takes
`IEnumerable<object>`; the older typed overload has different semantics.
