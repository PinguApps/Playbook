# Reservation comparison: retained evidence

PIN-952 records two fresh attempts, with Playbook first, then without. Both
complete within 600 seconds without timeout, coaching, resumption or retry.
See [the results](../../../reservation-validation/results.md).

The parent discovered after launch that the removed preparation directories had
been replaced with installed SDK/server paths. Although the SDK version matches
and the retained package hash matches, the protocol requires the same absolute
SDK executable across all eight runs. Pingu explicitly authorized accepting a
verified relocation before the without launch. The frozen ZIP matches its SHA-512;
all 5,467 files match the relocated installation byte for byte. The identity receipt
is in controls/sdk-identity.json. The [protocol amendment](../../../comparison-protocol.md#pin-952-relocation-amendment)
discloses its post-launch timing. No scored attempt was replaced.

The attempt used the frozen reservation scaffold commit
`6d53662cdba54551a74b3ade063c49009c63561c`, Codex CLI 0.166.0 (protocol SHA-256),
gpt-6.1-sol, medium reasoning, and exact-hash package 0.1.0-pin950 extracted
without rebuilding. The configuration records actual relocated paths. Both
unscored MXC preflights passed shell/edit/build/loopback, protected-read denial
and external HTTP/socket/proxy-bypass denial. Host controls confirmed reachability.

The with attempt completed in 198.5460576 seconds, without timeout,
coaching, resumption or retry. It made one list_recipes call for validation and
one get_recipe call for minimal-api.await-model-rules, target 11. The full response
and applicability are retained in its transcript. It added AddValidation and a
model-declared AsyncValidationAttribute. The unchanged private oracle scored all
15 assertions passing in a disposable copy. Calibration gives 15 failures for
the unfinished scaffold and zero for the reference. These observations do not
remove the need to disclose the amended control. The without attempt completed
in 139.4623859 seconds and passes the same 15 assertions, using a model method
and endpoint filter. It had no Playbook calls. Both preserve Availability.cs,
instrumentation, project/framework/language pins and TASK.md.

CLI turn usage: 650590 input, 589312 cached input, 6180 output, 916 reasoning
output, and explicitly reported cache-write input 0. Fields overlap; these are
not disjoint billing totals. Preparation and evaluator usage are excluded.
The transcript retains blocked build-network requests and cleanup failures.
Submitted source and diagnostics are unchanged; generated bin/obj and module
caches are excluded from source capture. Project/build files have a .txt suffix
to prevent accidental repository project discovery. No scored attempt should
be relaunched from these audit records.

The without CLI reports input 236624, cached input 200576, output 4187, reasoning
output 517 and cache-write input 0. The same usage-accounting limitations apply.

User and machine PATH comparisons remain unchanged. No related Linear issues changed.
Estimate remains M (3). Review and publication status belong in the PR/Linear handoff.
Pingu owns the final assessment under PIN-955. No broad model, performance, GA
readiness or release claim follows from this observation.
