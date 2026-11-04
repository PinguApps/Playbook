# Frozen comparison inputs

PIN-951 uses runs 1–2 of the [owner-approved protocol](../../../comparison-protocol.md):
without Playbook, then with Playbook. Each scored process has a 600-second deadline;
the external supervisor terminates its owned process tree on timeout. Preparation
and evaluator review are outside that budget. The other six runs remain separate
work; no retry or replacement of either scored attempt is permitted.

- Client: standalone Codex CLI 0.166.0, gpt-6.1-sol, medium reasoning.
- Scaffold: `6d53662cdba54551a74b3ade063c49009c63561c`, subtree
  `samples/OutcomeEvaluation`. Both exports have the same nine input-file hashes.
- SDK: 11.0.100-rc.3.26522.148, net11.0, C# 15.0. Microsoft release metadata
  checked on 4 November 2026 still identifies this SDK as latest. The NuGet
  QuickGrid index also still ends at 11.0.0-rc.3.26522.148; no input refresh was needed.
- Package: retained `PinguApps.Playbook.0.1.0-pin950.nupkg`, SHA-256
  `9A96E01473994BB854BB93D49C4E8B4CFE90805003DC249D7A7BE60505F2243C`.
  Its unchanged package/content inputs came from reviewed commit
  `b60a5e0e9aad7993ef037e02d0184ffb79720dac`. It was not rebuilt for this pair.
- CLI SHA-256:
  `9D563EE4E0441FF9D59D612013E0460DDF8E5709735C2D9F6B98B7EF8CA4EB42`.
- SDK archive SHA-512:
  `dae0a0450d92b5315034db134259acffe9ec0043d93dced4fb2a9b8700520e87a99005d6845ad80716f4a8f30df24ae5363ca7c9be391797b4f53170e9b2c088`.

Lookup sources: [Microsoft release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
and [QuickGrid versions](https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.components.quickgrid/index.json).
The package is retained here for the remaining with-condition runs too; its hash
must remain unchanged. Current evidence is RC3 evidence, not GA verification.

## Execution boundary

Each condition has its own exported workspace, dependency cache, temporary
directory, CLI home and .NET home outside the repository. Restore happened before
the clocks started. Ambient instructions/rules, skills, hooks, plugins, search,
apps and unrelated MCP connections were disabled. The exact effective configs,
prompts, input manifests and successful preflight transcripts are retained beside
each submission. Credentials are confined to runner authentication and excluded
from this bundle. Native helper tools remain listed in both contexts; the only
server-specific tools added in the with condition are Playbook list/get.

MXC enforces restricted filesystem roots and loopback-only command networking.
Final preflight exercised real shell execution, file edits, SDK build/application
execution and HTTP loopback. Repository/evaluator/other-run-output/user-config/
Playbook-binary reads, external HTTP, direct sockets and proxy bypass were denied.
Host controls observed example.com HTTP200 and a successful 1.1.1.1:443 connection.
Earlier unscored preparation failures involved missing NuGet environment paths;
they were resolved before either scored launch. No scored prompt was used for
preflight. The probe is retained as `preflight-probe.ps1.txt` for audit.

`supervisor.ps1.txt` records the actual launcher and capture procedure, not a
command to rerun these one-attempt evaluations. The `.txt` suffix on retained
projects/build files and scripts prevents accidental project discovery/execution.
Restore original names only in disposable evaluator copies. Submissions are never
repaired. `Oracle.cs.txt` is the unchanged private behavior oracle; baseline/reference
calibration logs show nine baseline failures and zero reference failures.

The submitted sources and final assessment remain reviewable. Machine transcripts
and the package have narrow review exclusions; integrity is checked separately.
Pingu owns the final four-pair assessment under PIN-955. This bundle authorizes
no deployment, release or general model-performance claim.
