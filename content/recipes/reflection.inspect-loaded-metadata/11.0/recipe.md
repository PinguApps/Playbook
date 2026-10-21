# Inspect metadata-only assemblies and handle unavailable caller information

## Goal

Inspect a fictional harbour's parcel plug-in without executing its code. Keep metadata-only reflection separate from a running application's caller diagnostics: they have different deployment requirements.

Use an untrimmed, file-based SDK console project. Set TargetFramework=net10.0 / LangVersion=14.0 for the comparison and net11.0 / 15.0 for the newer sample; enable implicit usings and nullable. Put declarations in namespace Playbook.Samples and import System.Reflection. The newer code also uses System.Runtime.Loader for the companion location example.

## Older approach

Reference System.Reflection.MetadataLoadContext 10.0.9 (the measured comparison pin). A standalone project can add a PackageReference with that Version; a centrally managed project instead declares that PackageVersion in Directory.Packages.props and a versionless PackageReference in the project. Do not inherit the Playbook tool's package settings.

The resolver receives both the host framework paths and the parcel assembly. Keep the owning context alongside the assembly: .NET 10 has no reverse lookup method. Recognise a nullable value type by comparing its generic definition with the definition loaded inside that same metadata context. A runtime typeof(Nullable<>) belongs to another reflection universe.

{{snippet:older}}

A constructor has no method type parameters, including on Parcel<int>. Its declaring type can have generic arguments. The older code records zero without calling ConstructorInfo.GetGenericArguments, whose old base implementation can throw. It does not claim to retrieve type parameters from the constructor.

## Newer approach

Use the .NET 11 framework and System.Reflection.MetadataLoadContext 11.0.0-rc.2.26504.105 for the measured RC2 sample. The selected package asset is lib/net11.0. The package also contains down-level assets; their existence alone does not establish that all these APIs work on an older target. No such application route is advertised here.

{{snippet:newer}}

GetLoadContext recovers the metadata context from an assembly it owns; ordinary runtime assemblies are outside that context. Type.GetNullableUnderlyingType dispatches to the type implementation, so metadata-only nullable types can answer within their own universe. It returns null for a non-nullable type. The new ConstructorInfo override returns an empty array; inspect DeclaringType.GetGenericArguments separately when you need the type's parameters.

Nullable.GetUnderlyingType now calls the new virtual method too. If you author a direct custom Type subclass, implement GetNullableUnderlyingType for its own universe or return null when it cannot represent nullable types. The base implementation throws NotSupportedException. A delegating wrapper should forward to the wrapped type; do not accidentally compare metadata-only types with runtime types.

### A direct custom Type migration

The harbour's diagnostic token is metadata only and never represents Nullable<T>. This complete minimal Type implementation deliberately inherits the base nullable virtual on .NET 11. It exposes no executable members; it is not a general replacement for runtime reflection. Import System.Globalization as well as System.Reflection.

{{snippet:custom-base}}

The .NET 10 specialization needs no nullable override:

{{snippet:custom-older}}

On .NET 11, Nullable.GetUnderlyingType(new HarbourTokenType()) throws because it reaches Type's unimplemented virtual. The updated specialization provides the correct answer for this non-nullable model:

{{snippet:custom-newer}}

Nullable.GetUnderlyingType(new NullableAwareHarbourTokenType()) returns null on both releases. A custom reflection universe that can represent Nullable<T> must instead return its appropriate underlying type. TypeDelegator already implements the virtual and would not reproduce the direct-subclass failure.

## Caller diagnostics are a separate deployment case

The .NET 10 comparison handles unsupported caller inspection honestly:

{{snippet:caller-older}}

The .NET 11 implementation uses the same explicit fallback:

{{snippet:caller-newer}}

Import System.Runtime.CompilerServices, System.Reflection and System.Diagnostics.CodeAnalysis. Call DeploymentReport.Caller from the diagnostic boundary you actually want to observe. NoInlining protects the shown method boundary; caller identity can still depend on surrounding inlining/tail calls and execution paths. It is unsuitable for security or business identity. Passing an explicit assembly or application identifier is often more predictable.

In .NET 10 Native AOT, GetCallingAssembly throws PlatformNotSupportedException. In .NET 11 Native AOT it can return the caller when stack-trace support exists. With StackTraceSupport=false, .NET 11 throws NotSupportedException on both Native AOT and CoreCLR. The catch handles both exception types because PlatformNotSupportedException derives from NotSupportedException. Keep the unavailable reason distinct from a successful identity.

## Migration

1. Retain explicit metadata resolver paths, including the inspected assembly's dependencies. The sample's TRUSTED_PLATFORM_ASSEMBLIES assumption is for its file-based CoreCLR host.
2. On .NET 11, use the context lookup and virtual nullable query. Audit custom Type implementations and constructor queries.
3. Dispose the metadata context after inspection; do not retain its Type objects for execution or compare them as though they were runtime Type objects. MetadataLoadContext cannot execute inspected methods.
4. For caller diagnostics, keep StackTraceSupport enabled or handle its absence explicitly. Removing an exception does not make caller identity a stable contract.
5. Validate the intended deployment independently. Native AOT does not support dynamic plug-in loading, and this metadata-only sample is not claimed as trimmed, single-file or Native AOT compatible.

## Constraints

Metadata inspection requires an untrimmed, file-based CoreCLR host and explicit dependency paths. Caller inspection has its own Native AOT and stack-trace constraints. Do not use caller identity for authorization, or treat metadata-only Type objects as executable runtime types.

## Evidence

The two actual MetadataInspection projects compile and execute metadata-only ownership, nullable Int32, constructor arity zero and the companion loader/location checks. Separate DeploymentInspection records publish and run Windows x64 single-file, Native AOT with stack traces enabled/disabled, and CoreCLR with stack traces disabled. They assert the release-specific caller outcome and physical/unavailable assembly location. Native tool versions, resolved publication dependencies, commands and output are labelled separately from the ordinary managed-build prerequisites.

For a standalone DeploymentInspection project, set OutputType=Exe and the target/language above; use PublishAot=true, RuntimeIdentifier=win-x64 and StackTraceSupport=true or false for the native comparison. Install the platform's supported native compiler/linker and SDK. The measured authoring route uses MSVC 14.51 and Windows SDK 10.0.26100.0; it invokes tools explicitly without modifying PATH. No additional package is required by the caller snippet; the SDK restores its native compilation packs during publication.

No mocks, cross-compilation, editor UI or other native OS/architecture execution is presented as proof. These are .NET 11 RC2 results, not GA verification. No provider, rendering mode, service account or resource provisioning is involved.
