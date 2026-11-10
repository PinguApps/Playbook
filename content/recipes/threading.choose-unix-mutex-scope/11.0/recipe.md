# Choose Unix named-mutex scope

## Goal

Coordinate Quay Lantern processes belonging to one user, or deliberately share a
named mutex across users. Provide writable storage for UID-only Linux containers.
The default changed in .NET10.0.13 and .NET11 RC2, as well as .NET8.0.32 and
.NET9.0.21. This is not an exclusively .NET11 feature.

## Older approach

Before that servicing change, default Unix named mutexes were global to users.
The actual .NET10.0.12 historical control coordinates two different UIDs in one
Unix session. Explicit user scope was already available in .NET10 through
NamedWaitHandleOptions. Existing same-user callers usually need no code change
when their user-specific storage remains accessible.

The original .NET10.0.13 sample below already has the new default. It uses the
same explicit global opt-in as the current sample; retaining this implementation
is valid. It must not be portrayed as a global-by-default older comparison.

{{snippet:older}}

## Newer approach

Use the default constructor for user isolation. Set CurrentUserOnly=false and
CurrentSessionOnly=false explicitly only when cross-user/global coordination is
intended. The audited documentation spells the opt-in NamedWaitHandleOptions.None,
but neither actual pinned reference set has that member. NamedWaitHandleOptions
is a struct with properties; the compiling canonical form is below.

{{snippet:newer}}

The global choice allows other users to participate and interfere. It is not a
permission boundary. The default also has session scope: two separate docker
exec calls can create different Unix sessions and therefore do not establish
same-user exclusion. The actual process tests launch all contenders in one shell
session and change only their UIDs.

## Migration

1. Identify whether different users actually need the same named mutex, and
   whether the callers share a session. Preserve a working same-user default.
   Compare actual servicing versions, not just net10.0 versus net11.0 targets.
2. For deliberate cross-user coordination, use the explicit property initializer
   shown above on .NET10 or later. All participants must use the same name and
   compatible scope options. Do not mix a global producer with user-only callers.
3. Provide an existing writable directory owned by the running UID and restrict
   it to that user (the Linux test uses mode0700). Set XDG_RUNTIME_DIR in the
   child/container environment before starting .NET. The runner creates separate
   /run/quay_a, /run/quay_b and /run/quay_unknown directories and changes ownership
   to the matching test UIDs. These are disposable container paths, not host edits.
4. Do not prescribe HOME as the user-database fallback or remedy. The source
   documentation specifies user-database home lookup; the measured RC2 unknown-UID
   control instead reported a wrapped storage failure under the supplied,
   inaccessible HOME. This is a pinned-artifact/source discrepancy, not a stable
   HOME contract. The verified remedy on both baselines is owned XDG_RUNTIME_DIR.
5. Acquire and release on the same thread and dispose the handle. The complete
   sample calls WaitOne(0), releases only when acquired, and reports storage
   failures including the measured RC2 TypeInitializationException wrapper.
   Real applications also need an explicit policy for abandoned mutexes; a mutex
   does not repair shared state after a crashed owner.
6. Run eng/Test-QuayMutex.ps1 -Record. It compiles both actual consumers and uses
   an isolated network-disabled Linux container, two known UIDs, an unknown UID,
   real competing processes and matching actual runtimes. Confirm same-user
   exclusion, default cross-user isolation and explicit global exclusion. Repeat
   unknown-UID creation without XDG_RUNTIME_DIR, then with its owned directory.
   The runner stops holders and removes only its uniquely named container.

## Constraints

Only Linux x64 Docker execution is measured. macOS per-user TMPDIR selection and
other Unix platforms are not execution claims. Both pinned baselines already
contain the changed default. The actual .NET10.0.12 runtime is an isolated
historical control, not a recommended rollback. SDK10.0.403/C#14 and SDK11 RC3/C#15
are exact comparison pins; no package, optional workload, provider or UI is needed.

The source also documents System.Threading.Mutex.UnsafeDefaultMutexScopeGlobal
as a broad runtime switch. Prefer the demonstrated per-call decision; this recipe
does not execute that process-wide alternative. Do not assume global mutexes
coordinate independent hosts or containers with separate backing storage. Names,
permissions, sessions and runtime directories all affect whether participants
refer to the same object. A build or mocked lock cannot establish these claims.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Saved sample-evidence records contain actual Linux .NET10.0.12, .NET10.0.13 and
.NET11.0.0 observations, exact image/archive hashes, build/run
commands, UIDs, scope inputs and cleanup. Same-session process tests verify the
older global default and the two pinned changed defaults; unknown-UID failure and
XDG_RUNTIME_DIR recovery execute for both pinned baselines. Official source and
actual reference contracts are pinned separately from supported prerequisites.
The ordinary installed tool is inspected through offline stdio list/get for
discovery ID/title/goal, complete canonical Markdown, explicit edition/target,
applicability and current evidence. No recurring OS matrix or .NET10 foundation
is added. Actual GA, PIN-959/PIN-962 and existing Human gates remain open.
