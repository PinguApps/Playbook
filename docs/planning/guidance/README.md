# Guidance delivered with deferred behaviour checks

Pingu requested documentation first for PIN-807, PIN-1170 and PIN-1181 on
3 November 2026, then authorized usable served recipes with runtime qualification
deferred. The canonical recipes below supersede these planning drafts. Five
available consumers have compilation records; current direct TLS has an explicit
source-review record for the reproduced missing endpoint API. No sample execution
or GA support is claimed. Broader release readiness remains gated.

| Served recipe / edition | Canonical recipe | Deferred qualification |
| --- | --- | --- |
| `blazor.restore-agent-conversation/11.0` | [Restore completed turns](../../../content/recipes/blazor.restore-agent-conversation/11.0/recipe.md) | Storage/renderer execution and GA |
| `blazor.synchronize-agent-view-state/11.0` | [Synchronize state](../../../content/recipes/blazor.synchronize-agent-view-state/11.0/recipe.md) | Real AG-UI/provider, browser/reconciliation and GA |
| `hosting.select-direct-tls/11.0` | [Select direct TLS](../../../content/recipes/hosting.select-direct-tls/11.0/recipe.md) | Matching endpoint API, successful current compilation and native Linux TLS/GA execution |

The intended comparison is .NET 10 to .NET 11. The eight approved .NET 10
foundations and existing editions remain unchanged. PIN-959's coverage/evidence
gate and PIN-964's actual-GA reconciliation remain open.

## Evidence boundary

The 3 November check queried NuGet's version indexes, downloaded the stated
packages and read nuspec/XML documentation. Reading DirectTls DLL metadata
confirmed the missing public endpoint type. This was artifact inspection only:
no restore, compilation, renderer, provider or TLS execution. Disposable packages
are in ignored `artifacts/availability/`, not retained sample-evidence records.

The newer AI artifact changes the October availability finding; it does not prove
the application works. Preserve historical audit baselines and qualify the new
dependency set separately. Follow the [dependency policy](../../dependency-updates.md)
and [sample evidence workflow](../../contributing.md#choose-the-check-and-retain-its-result).
Current sample pins, locks, canonical snippets and fresh compilation/source-review
records live beside MuseumCaptions, ObservatoryLog and HarbourTls. Use
`eng/Record-AgentStateGuidance.ps1` to refresh those records, and the ordinary
installed-tool [Inspector procedure](../../inspect-guidance.md) to inspect delivery.
No new native/client/OS matrix is planned.
