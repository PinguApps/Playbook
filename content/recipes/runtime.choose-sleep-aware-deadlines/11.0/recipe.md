# Choose whether a deadline includes system sleep

## Goal

An unattended inspection station retries a sensor while it is working, but its inspection authorisation has an absolute expiry. Choose these policies separately: an elapsed retry budget can pause with system sleep, while a business expiry should be checked against UTC when work resumes. A timer callback is a prompt to check, not permission to act after expiry.

## Older approach

On Windows, the reviewed .NET 10 implementation uses GetTickCount64, which includes low-power time. Code that stored a boot-relative start tick could therefore appear to enforce a business deadline after resume. That assumption is not portable across runtimes/platforms and ticks do not survive reboot as a durable expiry. An explicit UTC expiry was already a valid .NET 10 solution; this is a policy migration, not a newly introduced UTC API.

The paired net10.0/C#14 and net11.0/C#15 console projects compile the same original InspectionDeadline policy. No added NuGet package is needed. TimeProvider is the existing clock seam; changing its test value does not change the machine clock or simulate actual power transitions.

## Newer approach

The official .NET 11 Windows compatibility note changes TickCount64 to QueryUnbiasedInterruptTime and excludes sleep/hibernation from elapsed time. TickCount follows the same source, truncated to 32 bits. This is source-backed behaviour, **not a physical sleep/resume observation from these samples**. Other platforms retain their existing behaviour; no cross-platform execution is claimed.

For a short in-process retry budget, retain a start value from Environment.TickCount64 and compare elapsed milliseconds. The .NET 11 Windows route measures awake time according to that contract:

{{snippet:budget}}

Place this static method inside RetryBudget; capture startedAt immediately before retrying. Use a nonnegative, bounded budget. It is not a persisted deadline, and does not promise exact scheduling or improved timer resolution. On the older Windows runtime the same calculation includes low-power time; do not describe the old implementation as an awake-only budget.

For the business expiry, construct BusinessExpiry with TimeProvider.System and a UTC DateTimeOffset from the inspection authorisation. Keep that absolute expiry in durable state when it must survive restart. The class has a private bool expired field, initially false, and constructor parameters TimeProvider clock and DateTimeOffset expiresAt:

{{snippet:expiry}}

Call IsExpired immediately before starting each protected action and after waits/resume. A forward wall-clock correction may expire early. A backward correction delays an expiry not yet observed until civil time catches up. Once this instance observes expiry, it latches and never reopens. Persist terminal state separately if that rule must survive restart; this small single-threaded helper supplies neither persistence nor synchronization. A clock jump between check and action remains possible. For externally enforced validity, the authoritative service must also validate at the operation boundary.

## Migration steps

1. Classify each timeout: awake retry allowance or absolute business expiry. Find code that treats Environment.TickCount/TickCount64 as civil time.
2. Retain TickCount64 elapsed arithmetic only for the bounded retry allowance, acknowledging the old/new Windows difference. Do not persist boot ticks as business expiry.
3. Store the absolute UTC expiry for the authorisation. Inject TimeProvider and check UTC at the point of action, including after resume and every timer callback.
4. Agree the clock-correction rule. This example accepts forward expiry and backward delay, then latches observed expiry. If backward delay is unacceptable, use an authoritative external expiry decision; UTC alone cannot provide monotonic civil time.
5. Run both actual consumers and the deterministic boundary/clock-jump checks. Keep those results separate from physical suspend evidence. Here sleep/resume, hibernation and waits spanning suspend are explicitly **not tested**, as approved by Pingu on 7 November 2026. Do not suspend the authoring workstation.

## Constraints and alternatives

Prefer TickCount64 over TickCount. The 32-bit value changes sign around 24.9 days and repeats after about 49.7 days. A short interval can use unchecked unsigned subtraction only with a known bound shorter than one full wrap; no subtraction recovers missed full wraps. Negative signed tick values are not failures. No 32-bit elapsed route is recommended here; its ordinary observation is diagnostic only.

Neither UTC nor DateTimeOffset is monotonic. NTP, administrative corrections, clock drift and restarting a process affect business policies. A timer or OS wait cannot run application code while the system is suspended; checking an already-passed UTC expiry after resume does not promise a callback during sleep. Stopwatch/TimeProvider timestamps are not substituted as unmeasured suspend-inclusive clocks. No hibernation-tested route or universal power-mode result is advertised.

Requirements: desktop/server .NET 11 runtime and matching SDK for the primary net11.0 route; the canonical sample selects C#15. The comparison selects .NET10/C#14. Framework System.Runtime references and Microsoft.NETCore.App supply these stable APIs; no provider, rendering mode, workload, package or experimental opt-in is needed. The Windows behaviour change does not establish supported Windows versions: use an OS supported by the selected .NET release. Browser/WASI and other operating systems are outside the execution evidence.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

InspectionDeadline builds/runs with SDK10.0.403, runtime/reference10.0.14 and SDK11.0.100, runtime/reference11.0.0, on Windows x64. These are actual current authoring artifacts, distinct from the audit's 10.0.13/RC2 source baselines. The capture runner records compiler, System.Runtime reference, CoreLib and native coreclr identities/hashes and exact commands. No extra package or native toolchain is required. RC3 execution is not GA proof.

Real local checks observe TickCount64, TickCount, UTC and an unsignaled OS wait (120ms requested; broad 80–30000ms scheduler tolerance). UTC delta is diagnostic because adjustments are permitted. Separate deterministic TimeProvider checks cover before/exact expiry, forward correction, backward delay and a latched expiry staying closed. Real UTC past/future checks also run. These checks do not measure any suspended interval, power-mode effect or hibernation behaviour.

Physical Windows sleep/resume, hibernation and suspended OS-wait outcomes remain an outstanding **not-tested exclusion**, approved for this delivery. No separate machine, Human handoff or new hardware is required. No fabricated suspend interval or power-mode measurement is supplied. Ordinary installed-tool inspection checks offline discovery ID/title/goal, full canonical snippets, edition11/target11 applicability and current sample evidence; target10 remains comparison-only. Existing identities and the eight .NET10 foundations remain unchanged. PIN-959 comprehensive coverage and PIN-962 actual-GA reconciliation retain their independent gates.

## Official references

- [Pinned compatibility note](https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/core-libraries/11/environment-tickcount-windows-behavior.md)
- [Pinned Windows implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Private.CoreLib/src/System/Environment.Windows.cs)
- [Windows unbiased interrupt-time contract](https://learn.microsoft.com/en-us/windows/win32/api/realtimeapiset/nf-realtimeapiset-queryunbiasedinterrupttime)
