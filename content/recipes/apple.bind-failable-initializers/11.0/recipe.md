# Expose failable native creation as a nullable factory

## Goal

Bind a native initializer that can return nil so C# callers can handle an absent object without treating construction as success.

## Newer approach

Oystercatcher owns a native SurveyPermit initializer that rejects station codes without SUR-. A C# constructor cannot return null. Bind the native initializer with FactoryMethod("TryCreate") and return-level NullAllowed so the generated public static method returns SurveyPermit?. Disable the default constructor because it would bypass this domain contract.

For a Constructor definition, FactoryMethod names the public factory and makes the generated constructor internal. For a named init binding method, use parameterless FactoryMethod; its managed name becomes the factory name. Supplying a name in that case produces BI1127. The export must be init or start with init followed by an uppercase letter; otherwise BI1126. An out NSError parameter without a nullable return produces BI1125. This recipe's owned initializer has no NSError parameter: null is the entire failure signal.

NullAllowed belongs on the return value, not the stationCode parameter. The generated factory checks the native handle, disposes the failed wrapper and returns null. The consumer handles null and disposes successful objects with using. Do not force a non-null assertion or catch every exception as a rejected station; argument errors and unexpected runtime faults remain distinct.

{{snippet:net11-oystercatcherbinding-csproj}}

## Older approach

The .NET10 definition exposes the native initializer as a constructor. It is a valid binding shape but cannot express nil as a nullable object result; native initialization failure follows the runtime's constructor-failure behavior. A native nullable class-method wrapper is an alternative for an older product that needs a nonthrowing API. No such shim is built or claimed by this comparison, and no FactoryMethod older-target route is advertised.

## Migration

1. Confirm the native header really declares nullable initialization and identify its success/failure conditions.
2. Preserve its selector and parameter contract; add FactoryMethod and return NullAllowed.
3. Regenerate the binding and inspect both the internal constructor and public nullable factory, rather than hand-writing the expected generated API.
4. Compile consumers using the actual generated output. Handle null before use and dispose owned wrappers.
5. With an available native toolchain, build the owned Objective-C source into a native library, attach it via NativeReference, then exercise SUR-001 and an invalid code. That native build/execution is explicitly not-tested here. The checked-in header/implementation are complete source, not a prebuilt native dependency.

## Canonical samples

### net10-apidefinition-cs

{{snippet:net10-apidefinition-cs}}

### net10-directory-build-props

{{snippet:net10-directory-build-props}}

### net10-oystercatcherbinding-csproj

{{snippet:net10-oystercatcherbinding-csproj}}

### net10-surveypermit-h

{{snippet:net10-surveypermit-h}}

### net10-surveypermit-m

{{snippet:net10-surveypermit-m}}

### net11-apidefinition-cs

{{snippet:net11-apidefinition-cs}}

### net11-consumer-cs

{{snippet:net11-consumer-cs}}

### net11-directory-build-props

{{snippet:net11-directory-build-props}}

### net11-oystercatcherbinding-csproj

{{snippet:net11-oystercatcherbinding-csproj}}

### net11-surveypermit-h

{{snippet:net11-surveypermit-h}}

### net11-surveypermit-m

{{snippet:net11-surveypermit-m}}

## Constraints

Applicability lists all runtime, SDK, language, workload, artifact, provider, platform, rendering and stability constraints. These are .NET11 Apple workload features, with stable API labels and RC preparation evidence; neither actual GA nor a complete RC3 Apple composition was tested. No preview language syntax or experimental opt-in is used. .NET10 comparison compilation is separate from current task/reference checks.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Run eng/Test-AppleAuthoring.ps1 for pinned-source/artifact checks, real typed plist and stripping task output, actual binding generation and generated-source/consumer reference compilation. The source manifest records exact SHA-256 identities. SDK11 RC3 runs RC2 Apple tasks and bgen on Windows; that does not establish native linking, signing or runtime support. Source-review receipts retain sample/input freshness; native exclusions apply to the named examples under PIN-699. No Human action is pending. Packed stdio list/get establishes offline guidance delivery only. The eight .NET10 foundations and the comprehensive/GA gates remain unchanged.
