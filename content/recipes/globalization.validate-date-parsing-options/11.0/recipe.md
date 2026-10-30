# Separate invalid date parsing options from bad data

## Goal

Validate configured date and time formats and styles before importing rows, so configuration failures stay separate from malformed customer data.

A fictional coastal booking importer accepts compact or slash-separated arrival dates and compact or colon-separated times. An unsupported importer setting is a configuration error. A customer entering February 31 is a rejected row. Keep those outcomes distinct even when both used to look like `false`.

## Older approach

On the measured .NET 10.0.13 runtime, the string and span style-bearing `DateOnly.TryParse` / `TimeOnly.TryParse` calls return `false` for `AssumeUniversal` and an unknown style bit. Their single- and multiple-format exact overloads do the same. A null formats array or a null/empty array entry also returns `false` in the isolated cases tested. That can hide configuration mistakes among ordinary row failures.

Explicit application validation is a valid .NET 10 approach. The complete reader below is compiled and executed unchanged on both targets. It permits only the importer's four known invariant formats, copies the supplied arrays, and accepts only `None` or combinations within `AllowWhiteSpaces`. It does not attempt to validate every custom .NET format.

{{snippet:reader}}

## Newer approach

{{snippet:usage}}

Create a reader once when loading the import configuration as shown, then reuse it in the row loop. Reject a constructor `ArgumentException` as a configuration failure before opening that loop. After successful construction, call `TryRead` for each row; `false` means at least one field is invalid. Ignore both output values when the result is false. No exception is used to classify a bad customer date.

With .NET 11 RC2, invalid style arguments throw `ArgumentException` for the tested non-null string and span entry points. A null multiple-formats array throws `ArgumentException`, but null/empty entries throw **FormatException**, including when an earlier format could match. This differs from the compatibility note's broad ArgumentException wording. The reader's explicit contract gives the same deliberate configuration failure on both runtimes.

Do not use a successful trial parse as an arbitrary-format validator. On both artifacts, `Q` as a single exact format throws `FormatException`, a custom format ending in a backslash returns `false`, and `Q` after an already matching format is not examined. Null string input can return `false` before invalid styles are checked. These observations are not a guarantee that every unsupported format follows one path. If arbitrary externally supplied formats are required, handle both `ArgumentException` and `FormatException` explicitly at that configuration boundary, without blanket-catching all row failures or inventing a complete format validator.

## Migration

1. Find the code that supplies formats, culture and `DateTimeStyles`; identify which settings the importer actually supports.
2. Validate that contract when configuration loads. This example rejects empty lists, unknown formats and non-whitespace style bits, then owns copies of the accepted arrays.
3. Keep the invariant provider and the same format/style policy in the row parser. Use `TryRead` string or span entry points; route a false result to the rejected-row workflow.
4. Test valid rows, impossible dates/times, missing fields and configuration mistakes separately. Include single/multiple formats and string/span calls in upgrade checks; do not assume a method beginning with Try never throws for bad options.
5. Recheck the actual deployed runtime. Preserve the documented RC2 format-exception discrepancy until final artifacts demonstrate otherwise.

## Constraints

The edition targets .NET 11; the .NET 10 comparison is not an additional foundation or an older-target route to new runtime behavior. Both use managed framework APIs, an SDK console project with implicit System imports, `System.Globalization`, and namespace `Playbook.Samples`. Canonical modes are C# 14 / SDK 10 and C# 15 / SDK 11; this reader requires no new C# 15 syntax. No extra NuGet package, native toolchain, workload, provider, browser, UI rendering mode or experimental opt-in is involved. Normal supported runtime platform requirements still apply.

Whitespace styles are the supported style family here, not timezone interpretation. DateOnly and TimeOnly do not encode an arrival timezone or a booking instant. The example intentionally restricts available formats rather than promising that all standard/custom date formats are accepted. Copying arrays protects a configured reader from later caller mutation; failed rows do not represent transactional booking writes.

## Evidence

`samples/net10.0/BookingImport/BookingReader.cs` is the canonical complete implementation. `BookingChecks.cs` executes both actual frameworks' raw APIs and the same configured importer, checking valid values, alternate formats, whitespace, malformed and missing fields, configuration rejection and array ownership. The fresh saved records include exact commands, input hashes, empty package closures, compiler/reference hashes and actual runtime assembly hashes. Both executions use the invariant provider on Windows x64 with ordinary globalization enabled; current UI culture is recorded separately. No mock, cross-compilation, native, provider or browser result substitutes for those executions.

Verification pins are SDK 10.0.402 / runtime and references 10.0.13, and SDK 11.0.100-rc.2.26504.105 / runtime and references 11.0.0-rc.2.26504.105. These reproduce the observation; they are not universal installation requirements. The selected APIs carry no experimental opt-in. RC2 proof is not actual-GA proof; comprehensive coverage, final artifact reconciliation and existing Human gates remain open.

- [Pinned Microsoft compatibility note](https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/dateonly-timeonly-tryparse-argumentexception.md).
- [Pinned DateOnly implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Private.CoreLib/src/System/DateOnly.cs) and [TimeOnly implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Private.CoreLib/src/System/TimeOnly.cs), qualified by the measured binary results above.
