# Preserve JSON Patch operation serialization intent

## Goal

A glasshouse service edits garden records using small JSON update documents.
Its author needs required null values to survive serialization while irrelevant
fields stay out of the message. Compare actual wire bytes and applied records
before removing custom serializer rules.

## Requirements

Use Microsoft.AspNetCore.JsonPatch.SystemTextJson with System.Text.Json. The
examples run actual loopback HTTP PATCH requests through Kestrel, deserialize
JsonPatchDocument<GardenRecord> and apply it to a fresh working copy. This is a
real serializer/request pipeline, not reflection-only proof or a mocked patch.

Exact reproduction: SDK10.0.402/framework10.0.13/C#14 with package10.0.13, versus
SDK11.0.100-rc.2.26504.105/framework11.0.0-rc.2.26504.105/C#15 and the matching RC2
package. Use the matching framework/package family; these precise sample pins
are evidence, not minimum supported servicing versions. Windows x64 is the
executed managed lane. No package-native assets, workloads, browser rendering
or external provider are involved. This is stable API guidance prepared with
RC2 artifacts, not proof of actual GA. No experimental opt-in is needed.

The separate Microsoft.AspNetCore.JsonPatch implementation uses Newtonsoft.Json
and its own contracts. It is unchanged by this migration. Do not install both
serializers indiscriminately or assume this type-info modifier applies to
Newtonsoft's contract resolver. No Newtonsoft behavior change is claimed.

## Constraints

The actual older default serializer writes from:null for non-move/copy operations
and value:null for remove/move/copy. The original .NET10 type-info modifier below
retains its valid conditional serialization: from only for Move/Copy; value only
for Add/Replace/Test, including a meaningful null. It uses the older valid
ShouldSerializeFrom method. The newer package supplies conditional metadata and
value-holder serialization itself. Its from attribute omits NULL, rather than
checking the operation type: an add with a non-null spurious from still writes
that member. NONNULL-FROM demonstrates this on both actual packages. Keep an
operation-aware modifier or validate/normalize input if that stronger omission
policy is required. Its direct ShouldSerializeFrom calls warn
with default CS0618; use OperationType.Move/Copy checks instead. The paired
consumers produce the same corrected semantic message without requiring the
newer package's property order to be a general JSON contract.

Do not use global WhenWritingNull to approximate that policy. A replace with
value:null must retain the member; a remove must omit value altogether. Inspect
from/value metadata as context, then check actual serialized bytes and results.

Both measured packages deserialize a replace with an omitted value as null,
and the fixture applies it successfully just like explicit null. That acceptance
does not establish strict RFC member-presence validation. If your API rejects
missing value/from members, validate the raw JSON shape before deserializing;
you cannot recover presence merely by inspecting the resulting null property.

JSON Patch does not supply an application security policy. Authenticate and
authorize the record owner before accepting updates. Allow-list both path and
from, restrict writable types/operations, bound payload and operation complexity,
and validate the resulting domain object. The loopback fixture bounds its body
and count, permits four literal paths, rejects unknown/missing source paths and
applies only to a fresh working copy. It neither saves data nor models ownership.
A failed test/type conversion can follow earlier mutations: never persist that
partially modified copy. This is not a transactional ApplyTo guarantee.

## Migration

1. Confirm which JSON Patch package and serializer the application actually uses.
   Keep an existing Newtonsoft route separate. Pin and build the actual old/new
   consumers, including their standalone build/package boundaries.
2. For .NET10, keep the explicit operation-aware type-info modifier below if
   these wire omissions are required. It preserves valid null values rather than
   globally discarding them. DEFAULT-WIRE prints the unmodified old control.
3. For the matching .NET11 package, replace direct ShouldSerializeFrom calls
   with OperationType.Move/Copy checks. Remove the old modifier only after
   comparing actual wire bytes for your inputs. Keep the modifier if callers can
   supply non-null from on other operations; retain other custom policies too.
4. Copy every corresponding file below. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0
   in the child, select the project directory and build Release using its matching
   absolute SDK executable. Initial restore creates its package lock; retain it
   for subsequent --locked-mode restores. Execute the built GlasshousePatch.dll
   from bin/Release/net10.0 or net11.0 with the same executable.
5. Expect all six operations in the round trip and HTTP-applied label=shade,
   note=null, backup=null, seeds=0. Required value:null remains present; from
   exists only on copy/move. Compare the explicit-null/omitted-value HTTP controls
   honestly. Invalid path/from, missing source, failed test, partially failed
   update and incompatible numeric value must return 400 without saving anything.
6. Retain ownership/path/complexity/domain validation in the production API.
   Recheck actual GA artifacts separately; PIN-959/PIN-964 and unrelated Human
   gates remain open. Passing these local examples grants no new security promise.

## Older approach: .NET 10

Copy every file below at its relative path into an independent directory.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### GlasshousePatch.csproj

{{snippet:n10-glasshousepatch-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach: .NET 11

Copy every file below at its relative path into an independent directory.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### GlasshousePatch.csproj

{{snippet:n11-glasshousepatch-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Evidence

Run eng/Test-WebResourceRecipes.ps1 -RecordEvidence from the task worktree to
refresh the six actual consumers, diagnostic controls and audited artifact hashes.
The saved sample records include exact commands, locked assets, native identities,
inputs and observed output. Run the installed-artifact Inspector and returned-body
consumer commands in docs/inspect-guidance.md. No recurring OS/client/native matrix
is added. The eight approved .NET10 foundations and existing edition IDs remain
unchanged; this comparison is part of one new 11.0 edition.

## References

- [Operation from contract and obsolete method](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Features/JsonPatch.SystemTextJson/src/Operations/OperationBase.cs)
- [Operation value serialization](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Features/JsonPatch.SystemTextJson/src/Operations/Operation.cs)
- [Previous operation contract](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Features/JsonPatch.SystemTextJson/src/Operations/OperationBase.cs)
- [JSON Patch and application security responsibilities](https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-11.0)
