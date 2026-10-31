# Create file aliases with hard links

## Goal

A survey exporter needs an approved filename and a working filename to refer to
the same stored report. Writes through either name must be visible through the
other; removing one name must leave the remaining report usable. Use this when
sharing mutable file data is deliberate. A hard link is not an independent backup.

## Older approach

.NET10 has no matching managed creation members in the inspected reference pack.
A Windows native wrapper is a legitimate way to create the alias. Use absolute
paths, the Unicode entry point and the real last-error code. Restrict DLL lookup
to the system directory.

{{snippet:older}}

Pass the unused approved path first and the existing report path second. The
wrapper throws Win32Exception on native failure. It neither copies data nor
emulates a hard link with a symbolic link. Its Windows-specific maintenance remains
reasonable for an application that must keep targeting .NET10.

## Newer approach

The framework supplies creation methods, removing that native wrapper:

{{snippet:newer}}

`File.CreateHardLink(aliasPath, existingPath)` returns FileSystemInfo for the new
name; the sample needs only the side effect. For FileInfo, construct the receiver
with the **new alias path**, then pass the **existing file path**. The receiver
does not describe the original file. Both variants execute the same lifecycle
checks on the actual .NET11 runtime.

## Migration

1. Confirm that the application wants shared mutable file data. If it needs an
   independent snapshot, choose a copy with its own consistency policy instead.
2. Select an existing regular file and an unused alias path on the same supported
   local volume. This delivered route requires Windows NTFS. Resolve both paths
   deliberately; caller-controlled paths still need the application's normal
   access and directory policy.
3. Replace the Windows wrapper call with `SurveyAlias.Create(alias, existing)`,
   or use `CreateWithInfo` with the new-name receiver. Remove native interop only
   after all its remaining callers have a supported target.
4. Handle creation failures at the actual operation. On the measured NTFS fixture,
   an occupied destination, an absent target and a directory target are rejected.
   Do not delete an existing destination to retry, or silently copy on failure:
   either action changes the caller's data or alias contract.
5. Write through each name and read through the other. Delete each name first in
   separate cases and verify the remaining bytes. Dispose open streams before
   cleanup. Remove only the temporary files/directories created by the check.

## Constraints

The primary route is net11.0/.NET11 with the framework File/FileInfo APIs. The
canonical mode is C#15; there is no extra package, workload, rendering mode,
experimental opt-in or service. Previous comparison is net10.0/C#14 and Windows
kernel32 CreateHardLinkW. Exact test pins are separate from release-family
prerequisites. No ninth .NET10 foundation or older-target managed route is added.

Applicability is **Windows x64, local NTFS, same-volume regular files**. Do not
apply this route to directories, arbitrary network shares or other filesystems.
Unix, unsupported-filesystem and cross-volume failures were not executed here;
no particular exception/result for those environments is promised. Check those
routes independently before advertising them. No symbolic-link privilege or
Developer Mode setup is used by this fixture.

Changes to the existing file through either name affect the shared data. A
separate operation that replaces a pathname with a different file can break that
relationship; this recipe does not promise that editor/atomic-replacement workflows
preserve it. It makes no concurrency, storage-reclamation timing, security-boundary
or performance claim. TAR archive link representation belongs to a separate recipe.

## Evidence

`eng/Record-CertificateLinks.ps1 -VerifySources` compiles and runs the actual
SDK10.0.402/C#14/runtime10.0.13 native consumer and SDK11RC2/C#15/runtime11RC2 managed
consumer on Windows x64, local C: NTFS. Saved records identify exact reference
packs, compiler/CoreLib/kernel32 versions and hashes, empty package dependencies,
input bytes/hashes, commands and output.

Native creation and both current managed choices prove bidirectional writes and
remaining bytes after deletion in either order. Occupied-destination, absent-file
and directory-target attempts fail without overwriting the report or creating the
missing alias. Native errors and managed exception/HResult values are recorded
as observations, not a portable error taxonomy. Only the created temporary tree is
removed, after verifying its resolved parent. The source manifest reconciles the
old/current Runtime declarations and current File/FileInfo/Windows implementation.

The ordinary locally installed package returns ID/title/goal, exact expanded
canonical Markdown, target11/edition11 applicability and fresh sample records
offline. Other OS/filesystem/cross-volume and final-GA behavior remain unverified;
PIN-959/PIN-962 and existing Human gates remain open. No native CI matrix is added.
