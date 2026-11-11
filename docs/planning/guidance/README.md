# Guidance delivered with deferred behaviour checks

Pingu requested documentation first for PIN-807, PIN-1170 and PIN-1181 on
3 November 2026, then authorized usable served recipes with runtime qualification
deferred. The canonical recipes below supersede these planning drafts. All six comparison consumers now have matching-SDK compilation records.
The .NET 11 MuseumCaptions and ObservatoryLog receipts additionally record real
Interactive Server rendering with scripted local protocol/text and protected
session storage. Remote model/provider durability is outside that proof.

| Served recipe / edition | Canonical recipe | Remaining boundary |
| --- | --- | --- |
| `blazor.restore-agent-conversation/11.0` | [Restore completed turns](../../../content/recipes/blazor.restore-agent-conversation/11.0/recipe.md) | No host restart or remote-provider durability proof |
| `blazor.synchronize-agent-view-state/11.0` | [Synchronize state](../../../content/recipes/blazor.synchronize-agent-view-state/11.0/recipe.md) | Scripted endpoint only; no model/provider authentication or distributed merge proof |
| `hosting.select-direct-tls/11.0` | [Select HTTPS while direct TLS is unavailable](../../../content/recipes/hosting.select-direct-tls/11.0/recipe.md) | Withdrawn endpoint; socket compilation only. Actual supported direct-TLS availability remains mechanically pending for PIN-967; native Linux/OpenSSL is not-tested under policy |

PIN-1308/1309/1310 reconcile the earlier deferred work. The eight .NET 10
foundations remain unchanged. PIN-964's bounded audit is complete; PIN-967 owns
the final stable candidate. Historical observations below remain dated provenance.

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

Pingu also authorized documentation-based E25 delivery on 5 November 2026:
[Preserve missing non-entity join results](../../../content/recipes/ef.materialize-missing-projection/11.0/recipe.md).
The paired Plover consumers compile and reproduce RC failures; the explicit marker
works. Direct DTO/anonymous/nullable and default struct semantics need final-package
qualification. This is an intentional deferred behavior boundary, not GA proof.
