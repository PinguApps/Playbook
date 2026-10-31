# Report the failing directory without parsing messages

## Goal

A fictional seed import utility needs a structured path when a directory is missing.
Report useful diagnostics without extracting paths from translated exception text.

## Older approach

On .NET 10, retain the operation's attempted path yourself. This original helper
normalizes once, wraps the failure and stores that path in application-owned Data:

{{snippet:older}}

Call Read with the import file path. SelectPath can also accept the retained input
when handling an exception from another operation. This older route remains valid;
it never parses Message and preserves the original exception as InnerException.

## Newer approach

.NET 11 adds DirectoryNotFoundException.DirectoryPath and explicit-path constructors:

{{snippet:newer}}

Include the complete helper in a console/library project with ImplicitUsings and
Nullable enabled. When wrapping, preserve the available structured path and the
original failure; use the known attempted input if the property is null. The
three-argument constructor retains message, directoryPath and innerException.
The two-string constructor accepts message and directoryPath; use named arguments
for a null second argument to avoid ambiguity with the older Exception overload.

Real Windows missing-parent File.ReadAllText errors populate DirectoryPath with
the full attempted file path, including its filename. Directory.GetFiles and
Directory.Delete failures populate it with the attempted directory. The property
name does not imply that the value always names the exact first missing ancestor.
The selected operations passed under en-GB and fr-FR UI cultures without reading
Message; this does not assert which translated resources are installed.

## Migration

1. Preserve net10.0 operation context for older consumers. Use net11.0 for the new
   property/constructors; no additional historical foundation or package route.
2. Keep the attempted path at the operation boundary. Include the complete current
   helper and prefer DirectoryPath when supplied, then that input, then an explicit
   unknown marker. Null is legitimate on old/default/message-only exceptions.
3. When creating a contextual exception, pass directoryPath and innerException by
   name. Explicit constructor values are retained verbatim, including relative,
   empty or null paths; they are not automatically normalized or validated.
4. Normalize only under an application policy. Path.GetFullPath resolves against
   the current working directory; lexical normalization is not filesystem identity,
   link resolution, authorization or proof that a component exists.
5. Exercise actual missing-directory operations in an isolated temporary tree.
   Constructor-only tests cannot establish that filesystem failures set the property.
6. Redact or map paths before user-facing logs or responses when they reveal private
   directories. Exception.ToString can disclose the path too. Do not use formatter
   serialization or parse localized message/ToString output for structured data.

## Constraints

Primary route: net11.0/.NET 11 shared framework, canonical C#15. Older consumer:
net10.0/C#14. No package, provider, workload, rendering mode, external service or
experimental opt-in. Actual Windows x64 filesystem proof only. Other platforms,
other operations and their exception taxonomy are not established by these checks.

Default, message-only and older message/inner constructors leave DirectoryPath
unknown. The helper's null fallback is explicit; applications treating an empty
explicit path as unknown should apply that additional policy. Constructors accept
untrusted strings unchanged. Do not use the value as an access decision or claim
it is always normalized, safe to display, or the failing ancestor rather than the
attempted target. Catch only the intended exception; unrelated I/O failures remain
distinct. The old Data key is an application convention, not a framework property.

## Evidence

`eng/Test-TextBoundaries.ps1 -SampleNames SeedImports -Record -VerifySources`
executes real missing-parent read, directory enumeration and deletion under two
UI cultures, successful import, normalized attempted input, wrapper/inner retention
and known/unknown fallbacks. The current consumer separately checks two-string
and three-argument constructors, null and empty paths and verbatim relative input.
Only its newly created temporary tree is removed after validating the parent.

Exact proof pins: SDK10.0.402/ref-runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/ref-runtime11.0.0-rc.2.26504.105. These are reproduction
artifacts, separate from release-family prerequisites. No resolved packages.
Existing evidence records retain commands, inputs, compiler/CoreLib and original
audit-matching references. Pinned official exception/Windows error mapping sources
qualify availability. The ordinary installed package serves complete canonical
Markdown, ID/title/goal, applicability, target11/edition11 and current evidence
offline. RC2 is not GA proof; PIN-959/PIN-962 and existing Human gates stay open.
