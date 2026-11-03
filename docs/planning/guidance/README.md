# Guidance awaiting sample verification

Pingu requested documentation first for PIN-807, PIN-1170 and PIN-1181 on
3 November 2026. These original drafts explain the approaches before compilation
and behaviour qualification. They are outside the served catalogue and do not
establish compatible dependency closures, working samples or release readiness.

| Intended recipe / edition | Draft | Remaining qualification |
| --- | --- | --- |
| `blazor.restore-agent-conversation/11.0` | [Restore completed turns](restore-agent-conversation.md) | Canonical samples, storage/renderer execution, installed-tool retrieval |
| `blazor.synchronize-agent-view-state/11.0` | [Synchronize state](synchronize-agent-view-state.md) | Canonical samples, real AG-UI updates/reconciliation, installed-tool retrieval |
| `hosting.select-direct-tls/11.0` | [Select direct TLS](select-direct-tls.md) | Matching public endpoint API, native Linux execution, canonical samples, installed-tool retrieval |

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
Before serving these recipes, author canonical snippets and accurate metadata,
then reuse the ordinary installed-tool [Inspector procedure](../../inspect-guidance.md).
No new native/client/OS matrix is planned.
