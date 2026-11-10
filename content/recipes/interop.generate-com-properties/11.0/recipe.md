# Generate COM property contracts

## Goal

Expose a fictional Mere Beacon instrument reading and caption through a generated
IUnknown interface. Use .NET 11 property declarations when the native contract is
expressed as accessors. Preserve the HRESULT signatures, vtable order, string
encoding and allocation ownership that the real consumer expects.

## Older approach

.NET 10's COM generator uses explicit methods for this goal. They remain valid
on .NET 11. These are real .NET 10 methods, not a newer generator retargeted to an
older framework. Put the following in a separate file with
`using System.Runtime.InteropServices;`, `using System.Runtime.InteropServices.Marshalling;`
and the `Playbook.Samples` namespace.

{{snippet:older}}

## Newer approach

The .NET 11 reference pack's COM generator accepts complete abstract get/set
property declarations and generates the accessors. The generated class implements
the interface, forwarding calls to the managed state. The framework attributes and
property support used here require no experimental suppression or preview language
switch. The sample uses C# 15.0, net11.0, AllowUnsafeBlocks=true, nullable and implicit
usings; .NET 10 uses C# 14.0 and its own generator.

{{snippet:newer}}

The same independent Windows x64 C consumer calls both actual generated interfaces.
Slots 0–2 are IUnknown; slots 3–6 are reading get/set and caption get/set for these
specific declarations. Each accessor returns an HRESULT. A getter's result is an
out parameter. The consumer checks initial values, writes 29 and `Quay Ω`, reads
them back and frees every returned UTF-16 allocation through a CoTaskMem callback.

{{snippet:native}}

The managed check creates a Beacon, obtains its interface pointer through
ComInterfaceMarshaller<IBeacon>.ConvertToUnmanaged, loads the native helper by its
absolute path and calls exercise. It keeps both object and DLL alive, releases the
interface with ComInterfaceMarshaller<IBeacon>.Free in finally, then unloads the
helper. This is a direct native vtable proof; no COM registration is needed.

{{snippet:consumer}}

## Migration

1. Establish the existing IID, slot order, signatures, HRESULT handling, string
   encoding and ownership. Use a new IID when changing the binary contract. Property
   syntax alone does not certify compatibility with an existing interface. The
   paired declarations here deliberately match the measured native consumer.
2. Keep explicit methods for net10.0. For net11.0, use the matching .NET 11 SDK,
   framework references and COM generator. No additional managed NuGet package,
   workload, provider or rendering integration is required. Windows x64 and a C
   compiler are needed for this sample's real boundary check.
3. Replace a verified get/set method pair with a property whose accessors are both
   declarations. Do not mix a declaration with a default accessor body. A property
   implemented entirely in the interface is managed default behavior, not a request
   to generate half a native property. The actual RC2 compiler rejects the mixed
   sample with CS0525; the source audit also describes an all-or-nothing generator
   diagnostic. Do not depend on a particular diagnostic winning first.
4. Keep marshalling explicit. StringMarshalling.Utf16 controls this interface's
   strings; the native caller frees returned text with CoTaskMemFree. A count-only
   MarshalUsing on an accessor is rejected with SYSLIB1090. On an accessor, combine
   the marshaller type and count in one attribute; supported property-level count
   and indirection settings are distinct. Do not suppress a rejected contract.
5. Build the actual native consumer and run both targets. eng/Test-BeaconContracts.ps1
   uses the absolute MSVC compiler with /c /GS- /TC, then the matching linker with
   /DLL /NOENTRY /NODEFAULTLIB /MACHINE:X64. It copies beacon.dll beside each managed
   output. Its C source requires neither SDK headers nor a CRT. Recheck another ABI
   against its own compiler and consumer before using these slot assumptions there.
6. Test memory-safety evolution separately. The ordinary property route above does
   not enable updated-memory-safety-rules. The isolated preview compiler check in
   eng/Test-BeaconDiagnostics.ps1 enables that feature only for a LibraryImport
   declaration: the unmodified declaration fails CS9389, while explicit `safe`
   compiles; without the opt-in the original declaration compiles. The imported
   function is never invoked. This is neither a property prerequisite nor proof of
   every safety annotation. The source audit's SYSLIB1064 is not asserted as the
   diagnostic emitted by this actual compiler path.
7. Refresh evidence after changing an interface, marshaller, compiler, generator or
   native consumer. Never infer a successful boundary call from generated-source
   presence or a successful managed compilation.

## Constraints and alternatives

This is IUnknown vtable interop with a generated managed implementation. It does
not provide IDispatch, a type library, registration, out-of-process activation,
COM apartment policy, or compatibility with every native client. It does not
establish custom collection marshalling, indexers, ref-return properties or init
accessors. Retain explicit methods when they express the existing binary contract
more clearly. Validate errors and lifetimes for the real component protocol.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The sample-evidence workflow records SDK10.0.403/runtime10.0.14 and
SDK11.0.100-rc.3.26522.148/runtime11.0.0-rc.3.26522.148, actual reference/compiler/
generator identities, native MSVC19.51.36260.0/linker14.51.36260.0/helper hashes,
inputs, commands, empty managed dependency sets and observed native checks.
eng/Test-BeaconContracts.ps1 -Record and eng/Test-BeaconDiagnostics.ps1 run the
bounded successful and rejected cases. Immutable publisher contracts are listed
in docs/planning/audits/host-contracts.sources.json and reconcile with the original
runtime-security audit. Exact verification pins are measured artifacts, not a new
support promise for arbitrary compilers or targets.

The ordinary packed tool is installed from an isolated local feed/cache and
inspected through offline stdio list/get from an unrelated directory. Discovery,
full canonical Markdown, target11/edition11, applicability and fresh evidence are
checked. Other ABIs, activation models and final GA remain unverified. The exact
eight .NET 10 foundations, PIN-959/PIN-962 and existing Human gates retain their
own obligations. No recurring native matrix or public publication is introduced.
