# Validate dates against the Japanese calendar boundary

## Goal

Import a fictional town archive's historical records without changing their stored
Gregorian dates when a Japanese-calendar projection is unavailable. Use this when
an upgrade rejects dates that an older runtime accepted. Calendar range validation
is separate from parsing a date string or choosing its display format.

Use a console project with implicit System imports, nullable enabled,
`using System.Globalization;` and namespace `Playbook.Samples`. The canonical
comparisons use net10.0/C#14 and net11.0/C#15; neither needs a NuGet package.

## Older approach

The archive queries the calendar's supported range before projecting a stored date
to an era and year. Unsupported records remain Gregorian and receive an explicit
`RetainGregorian` result. They are retained for review or Gregorian-only display,
rather than discarded. This approach already works on .NET 10.

{{snippet:older}}

On the measured .NET 10 runtime, the minimum is 8 September 1868. The four test
records dated 8 September, 1 October, 22 October and 23 October 1868 all produce
Meiji year 1. A record from 7 September is already outside that supported range.
Hardcoding the old minimum would confuse an earlier runtime's capability with
the application's continuing storage policy.

## Newer approach

Keep the Gregorian source record and the same range-based policy. The .NET 11
minimum is 23 October 1868. The first three records now take the Gregorian-only
route; the record on 23 October still produces Meiji year 1. The removed interval
is **[8 September 1868, 23 October 1868)**: the new minimum itself is supported.

{{snippet:newer}}

Calling `GetYear` directly for a date in that removed interval throws
`ArgumentOutOfRangeException` on the measured current runtime. A successful
comparison alone is not execution evidence: the checks call actual calendar
operations and verify the returned era/year or rejection.

## Migration

1. Find constants, validation rules, persisted projections and tests that assume
   the Japanese calendar starts on 8 September 1868. Keep the original Gregorian
   date available when reviewing any previously computed projection.
2. Decide what unsupported records mean for your application. This importer keeps
   the Gregorian value with no Japanese era/year. A validation error asking for
   review is also valid; silently moving the date to the minimum is not.
3. Obtain `MinSupportedDateTime` and `MaxSupportedDateTime` from the calendar used
   for the operation. Check the inclusive range before computing the projection.
   Deploy the policy with the runtime you tested; an SDK-only upgrade does not
   change the running calendar implementation.
4. Exercise the old boundary, an interior removed date, the day before the new
   boundary and the new boundary itself. Assert real `GetYear`/`GetEra` behavior
   and unchanged source values, including their `DateTime.Kind`.
5. Verify globalization capability in the deployment environment. Fail setup
   explicitly if `ja-JP` or `JapaneseCalendar` cannot be instantiated. Do not
   interpret missing globalization support as an ordinary unsupported record.
6. Refresh era-dependent expectations against the actual runtime and globalization
   configuration. The official mitigation documents no switch to restore the old
   minimum. Retaining historical Gregorian values remains valid on both releases.

## Constraints

This is validation of an existing Gregorian `DateTime`, not a historical lunar
calendar conversion or timezone conversion. Parsing, ambiguity, display policy
and other calendars need their own decisions. A retained Gregorian result must
remain visibly distinct from a valid Japanese-calendar projection downstream.
The policy preserves the original value; it does not validate the provenance of
previously stored era/year fields or promise their historical correctness.

Current guidance targets the .NET 11 runtime/framework. SDK11/C#15 are the sample
configuration, not new syntax requirements. The boundary is a runtime change;
there is no independent package, provider, rendering mode, service, workload,
experimental opt-in or native compiler requirement. The publisher does not mark
these calendar APIs experimental. The older comparison is not a ninth .NET 10
foundation or a claim that .NET 10 implements the newer boundary.

Measured execution uses Windows x64 CoreCLR JIT, `en-GB` current culture, usable
`ja-JP`, non-invariant globalization and ICU (`UseNls=False`). Invariant mode is
unsupported and deliberately fails the capability check. NLS, Linux, macOS, AOT
and other cultures/globalization configurations have not been executed here.

## Evidence

The paired original HistoryImport consumers build and run using SDK10.0.402 /
C#14 / runtime/ref10.0.13 and SDK11.0.100-rc.2.26504.105 / C#15 /
runtime/ref11.0.0-rc.2.26504.105. They verify all four boundary records, actual
calendar operations/rejections, inclusive maximum, an older unsupported date,
explicit routing and preservation of stored values. A separate real child with
`DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` must fail with
`CultureNotFoundException`, without inventing a calendar fallback.

`eng/Record-HistoryImport.ps1 -VerifySources` captures exact commands, dates,
results, globalization mode, resolved dependencies and compiler/reference/runtime
hashes in the existing sample records and checks four pinned official-source
hashes. The ordinary installed artifact is exercised through real stdio list/get
using the repository's Inspector harness. RC2 execution is not actual-GA proof;
PIN-959, PIN-962 and existing Human gates remain open.
