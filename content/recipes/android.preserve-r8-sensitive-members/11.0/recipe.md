# Preserve Java names used by lookup under R8

## Goal

Keep only the Java classes and members accessed by name when migrating an R8-enabled Android app to optimization and private-member obfuscation.

## Newer approach

Breakwater accesses its owned Java Beacon class and private signal method by name.
R8 cannot generally infer string-based lookup from managed code. Include beacon.pro
as ProguardConfiguration and preserve the constructor and exact method signature.
The rule keeps the class name and those members, allows optimization, and permits
the unrelated unused method to be removed. Avoid broad rules preserving every
class/member. If a class is already reachable, a keepclassmembers rule may suffice
for its members; it does not itself keep an otherwise unreachable class alive.

In .NET11 GA, an R8-enabled build defaults to private-members: optimization is
active and private/package-private Java members may be renamed. Public/protected
member and class names remain protected for managed interoperability. This does
not make every private string lookup safe. Carry consumer rules with a Java
library whose consumers perform these lookups, and test its packaged artifact.

The canonical project states the policy explicitly. As a temporary compatibility
choice set AndroidR8ObfuscationMode to disabled. This retains shrinking while
restoring non-obfuscating, non-optimizing policy; it still needs keep rules for
otherwise unreachable code. Do not mistake disabling obfuscation for preserving
all Java members. Prefer precise rules once the dynamic entry points are known.

{{snippet:net11-namereader-cs}}

## Older approach

.NET10's R8 policy already supports explicit keep rules. Its canonical sample uses
the same rule and omits the new policy property. This is a valid older approach;
the migration changes the optimizer policy rather than introducing Java reflection.
GA does not ship experimental managed-assembly rewriting. Do not recommend that
design, runtime-remapping or label all R8 usage experimental.

## Migration

1. Recreate the canonical files with the correct SDK/workload and keep file.
2. Inventory class/member names reached through JNI, reflection and external libraries.
   Keep only necessary signatures, including constructors where created dynamically.
3. Build Release with R8 enabled; inspect the resulting DEX and mapping, not only
   project text. Confirm Beacon/signal survive with the exact name and unused is absent.
4. Run NameReader.Read on an available Android device and expect harbour-ready.
   Check the final packaged application; a successful optimizer run does not prove JNI.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 Android app builds. A separate real R8 classfile run of the canonical
Java input preserves Beacon's constructor and private signal method while removing
unused. This proves those rules on that optimizer input, not the .NET11 packaging
pipeline or Android execution. Current native compilation and both device lookups
are **not-tested** under PIN-699. No pending Human action or GA proof is claimed.

## Constraints

Requires .NET11 Android, C#15, Android API24+, matching Android SDK and JDK21/25.
Gradle invoked using JDK25 needs9.1+. Canonical SDK is GA; reviewed platform
assets are GA 37.2.29, not a tested GA composition. No extra NuGet
package, external provider/account or Blazor rendering mode is required. The
.NET10 comparison is not a supported older-target catalogue route. NativeAOT
is experimental and outside this sample. Recorded evidence does not certify GA.

## Canonical samples

### samples/net10.0/BreakwaterNames/beacon.pro

{{snippet:net10-beacon-pro}}

### samples/net10.0/BreakwaterNames/BreakwaterNames.csproj

{{snippet:net10-breakwaternames-csproj}}

### samples/net10.0/BreakwaterNames/Directory.Build.props

{{snippet:net10-directory-build-props}}

### samples/net10.0/BreakwaterNames/Directory.Build.targets

{{snippet:net10-directory-build-targets}}

### samples/net10.0/BreakwaterNames/Directory.Packages.props

{{snippet:net10-directory-packages-props}}

### samples/net10.0/BreakwaterNames/global.json

{{snippet:net10-global-json}}

### samples/net10.0/BreakwaterNames/MainActivity.cs

{{snippet:net10-mainactivity-cs}}

### samples/net10.0/BreakwaterNames/NameReader.cs

{{snippet:net10-namereader-cs}}

### samples/net10.0/BreakwaterNames/Java/Beacon.java

{{snippet:net10-beacon-java}}

### samples/net10.0/BreakwaterNames/Properties/AndroidManifest.xml

{{snippet:net10-androidmanifest-xml}}

### samples/net11.0/BreakwaterNames/beacon.pro

{{snippet:net11-beacon-pro}}

### samples/net11.0/BreakwaterNames/BreakwaterNames.csproj

{{snippet:net11-breakwaternames-csproj}}

### samples/net11.0/BreakwaterNames/Directory.Build.props

{{snippet:net11-directory-build-props}}

### samples/net11.0/BreakwaterNames/Directory.Build.targets

{{snippet:net11-directory-build-targets}}

### samples/net11.0/BreakwaterNames/Directory.Packages.props

{{snippet:net11-directory-packages-props}}

### samples/net11.0/BreakwaterNames/global.json

{{snippet:net11-global-json}}

### samples/net11.0/BreakwaterNames/MainActivity.cs

{{snippet:net11-mainactivity-cs}}

### samples/net11.0/BreakwaterNames/NameReader.cs

{{snippet:net11-namereader-cs}}

### samples/net11.0/BreakwaterNames/Java/Beacon.java

{{snippet:net11-beacon-java}}

### samples/net11.0/BreakwaterNames/Properties/AndroidManifest.xml

{{snippet:net11-androidmanifest-xml}}

