# Migrate build XML to the current DTD policy

## Goal

Keep a custom dispatch-manifest build working when its XML tasks adopt the current DTD default. Prefer ordinary element data; retain a narrowly scoped exception only for trusted files that cannot yet lose their document type declaration.

## Older approach

The .NET 10 comparison reads and edits this original trusted manifest. Its DTD declares structure; the destination is ordinary text, not an entity reference. Save it as trusted.xml:

{{snippet:input}}

Save this transformation as route.xsl:

{{snippet:transform}}

The older XmlPeek and XmlPoke calls accept this file without an explicit task flag. The older XslTransformation path already rejects a DTD-bearing XML input, so it uses the equivalent DTD-free flat.xml below. There is no claim that all three older tasks share the same effective default.

{{snippet:older}}

## Newer approach

XmlPeek, XmlPoke and XslTransformation now default ProhibitDtd to true. The same DTD-bearing input fails: the measured diagnostics are MSB3733 for peek/poke and MSB3703 for transformation. For trusted input, explicitly set ProhibitDtd=false on each affected call:

{{snippet:newer}}

This **ignores** the DTD. It does not validate its declarations, resolve external entities or expand internally declared entities. An earlier entity-based test failed with an undeclared-entity error; changing the flag was not a substitute for migrating entity data.

Prefer removing the declaration and materializing the intended value in flat.xml:

{{snippet:flat-input}}

The ordinary default then works without an exception:

{{snippet:flat}}

## Migration

1. Find every XmlPeek, XmlPoke and XslTransformation call and its actual input, including imported targets, XML supplied as a string and transformation inputs. Establish whether each file is trusted and whether entities provide data.
2. Prefer DTD-free XML with explicit values, as flat.xml demonstrates. Regenerate upstream manifests where possible; preserve encodings, namespaces and the intended query results.
3. If a trusted structural declaration must remain temporarily, apply ProhibitDtd=false only to those task calls. Never use this as a DTD-validation or entity-expansion switch.
4. Put the chosen targets inside a Project file alongside the three XML files. Run the selected SDK with `dotnet msbuild BuildInputs.proj -t:ReadTrusted` or `-t:ReadFlat`. Both report destination=QUAY-7; ReadTrusted also writes QUAY-8 to working.xml and creates route.xml containing QUAY-7.
5. Exercise the old input with the default to establish the failure, then check the migrated output and remove unnecessary exceptions. Keep the unrelated build's warning/error policy intact.

## Constraints

The XML itself is language-neutral. The compiled authoring harness uses C# 14/net10.0 and C# 15/net11.0, but these XML tasks do not require an application language upgrade. The declared route is target 11 using its SDK; no down-level application route or .NET Framework MSBuild.exe execution is established. No package, provider, rendering mode or native workload is required.

Do not infer that ignoring a DTD sanitizes an untrusted file. Trust includes the provenance of the XML and stylesheet. XSLT script/document-function permissions and XML namespaces remain separate concerns. Removing a declaration without materializing entity-based data can change or break the document.

## Evidence

Both original managed harnesses actually compile and execute with their pinned SDKs. They run real MSBuild tasks, check the queried value, edited XML and transformation output, and verify the DTD-free route. RC2 also reproduces all three default-rejection diagnostics. This is Windows x64 execution using SDK 10.0.402 and 11.0.100-rc.2.26504.105, not cross-compilation, a mock or GA proof. The packed-tool scenario separately checks the complete served body, applicability and target edition offline.
