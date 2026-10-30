# Compose a filesystem watcher with the intended root

## Goal

A botanical garden watches incoming observation files under a configured directory.
Keep the token root aligned with the native watcher, and support a directory created
after the watcher is constructed. Fail early for unrelated paths.

## Older approach

An equal, explicitly normalized existing root is a valid .NET 10 approach. The
garden creates that directory before watching and adds a trailing separator for
the older implementation's relative event-path handling. The returned watcher owns
the native watcher and must be disposed.

{{snippet:older}}

Call `GardenWatch.Open(configuredDirectory)`, keep the result in `using`, obtain
`CreateFileChangeToken("observation.json")` before changing the file, and react when
that token changes. Tokens are one-shot; obtain another for subsequent observations.
Use the existing consumer callback recipe under PIN-745 for rearming and async work.

## Newer approach

The current constructor normalizes a relative root itself. A native watcher with
an empty path supports waiting for that root to be created; this factory therefore
does not create the directory as a side effect.

{{snippet:newer}}

Both actual public overloads were checked: the three-argument form uses Sensitive
exclusions; the four-argument form selects them explicitly above. Default
`pollForChanges=false` is deliberate. When a nonempty native path is supplied it
must equal the root, be its ancestor, or be its descendant. An ancestor watches
subdirectories so a file inside the root can notify. A descendant only observes
its own subtree: request a token such as `beds/observation.json`, relative to the
root. Acceptance does not widen its coverage to sibling beds.

| Input | Previous constructor | Current constructor / measured result |
| --- | --- | --- |
| Null root | Accepted at construction; unusable input | ArgumentNullException |
| Root containing NUL | Accepted at construction; unusable input | ArgumentException |
| Relative root | Accepted without normalization | Normalized; real token changes |
| Equal native path | Working normalized older factory | Real token changes |
| Ancestor native path | Construction acceptance only; no older monitoring guarantee | Real token changes inside root; subdirectories enabled |
| Descendant native path | Real token in that subtree | Real token in that subtree |
| Unrelated sibling | Accepted; not functional monitoring evidence | ArgumentException |
| Shared text prefix, root versus root-other | Accepted; not functional monitoring evidence | ArgumentException |
| Empty native path, existing root | Token creation throws FileNotFoundException on measured old build | Real token changes |
| Empty native path, nonexistent root | Token creation throws FileNotFoundException on measured old build | Create root later, wait for native watcher to arm, then real file token changes |

## Migration

1. Reference `Microsoft.Extensions.FileProviders.Physical` and use a local filesystem
   supporting OS file events. The samples lock 10.0.13/net10 and 11RC2/net11.
2. Choose the intended token root, rather than an unrelated native directory whose
   string happens to share a prefix. Pass relative token patterns within that root.
3. For an existing directory, use an equal native path. When an ancestor or
   descendant is intentional, retain its limited coverage and verify a matching token.
4. For deferred creation, pass an empty `FileSystemWatcher.Path` and the future
   root. Register the token before creation; after creation, wait until monitoring
   is armed before depending on subsequent file changes. The creation event itself
   is not a promise that every simultaneous write will be retained.
5. Dispose the owning watcher and consumer registrations. Recreate tokens after
   changes. Test on the actual filesystem; file watching is a notification hint,
   not a durable event queue.

## Constraints

Measured on Windows x64 NTFS. Both old and current constructors, actual files and
matching tokens run in independently pinned consumers. SDK10.0.402/C#14/runtime10.0.13
and SDK11.0.100-rc.2.26504.105/C#15/runtime11RC2 are verification baselines. Exact
package closures, net10/net11 assets and compiler/reference/runtime hashes are saved.
Full Physical remains a package even though FileProviders abstractions joined the
net11 framework. No newer-package-on-net10 route is claimed.

The older ancestor setup can fail asynchronously on parent-directory events; its
construction acceptance is not taught as a monitoring contract. The previous
canonical factory deliberately uses equal paths. Relative old inputs likewise need
caller normalization. Current validation is path-relationship checking, not a
symlink/junction escape defence or filesystem authorization policy.

No universal event order/count, polling latency, network-share behavior or browser,
WASI, iOS/tvOS support is promised. Null native watcher with polling is outside this
recipe; `pollForChanges=false` requires a native watcher. For environments without
file events, separately choose a supported polling/import strategy. No workload,
rendering mode or experimental opt-in is needed. RC proof remains GA-pending.

## Evidence

`eng/Record-FileChain.ps1 -VerifySources` runs both overloads, null/invalid roots,
unrelated and prefix siblings, old construction boundaries and real supported
layouts. Current relative, empty and deferred layouts produce matching tokens;
ancestor recursion is asserted. The deferred case creates the directory after
registration and waits for the native watcher to arm before writing. Canonical
factories execute too. Every wait is bounded and watchers/temp directories are
cleaned up. Saved compilation/execution records retain actual inputs and commands.

The installed ordinary package's real offline stdio list/get checks discovery
ID/title/goal, full snippets, target11/edition11, applicability and fresh evidence.
The .NET10 comparison adds no ninth foundation. PIN-959/PIN-962 and existing Human
gates remain open; no recurring platform matrix is added.
