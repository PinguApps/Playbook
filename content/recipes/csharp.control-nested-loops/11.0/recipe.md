# Stop or continue an enclosing loop directly

## Goal

Choose which loop a nested decision should affect without losing cleanup or the work after the loop. This fictional radio archive reviews batches of readings: positive values are accepted, zero skips the rest of that batch, and a negative value ends the entire review. A completed batch gets a completion entry; skipped or stopped batches do not. Accepted earlier readings remain accepted.

## Older approach

In .NET 10 / C# 14, ordinary break exits the nearest loop or switch and ordinary continue advances the nearest loop. Flags can carry the inner decision to the outer loop. This is valid, explicit code; extracting a helper or using a carefully placed goto can also suit an existing design. C# 14 has no labeled break/continue syntax.

Create a console project targeting net10.0 with LangVersion 14.0, ImplicitUsings enabled and Nullable enabled. The following declarations use the file-scoped namespace Playbook.Samples:

{{snippet:older}}

The inner break ends only the readings foreach. The subsequent flag checks decide whether to break or continue the batch for loop. Keep their order and the completion entry's position when migrating.

## Newer approach

Target net11.0 with LangVersion 15.0 and the same implicit-usings and nullable settings. Use the same namespace. Label the outer for directly and name that label at each inner decision:

{{snippet:newer}}

For example, call `TransmissionReview.Review([[21, 0, 22], [31], [-1, 41], [51]])`. The accepted readings are 21 and 31; only batch 1 completes. Reading 22 and every reading after the negative sentinel are never visited. Both implementations return exactly this ordered trace:

    accept:21
    dispose:reading:21
    dispose:reading:0
    finally:0
    dispose:batch:0
    accept:31
    dispose:reading:31
    complete:1
    finally:1
    dispose:batch:1
    dispose:reading:-1
    finally:2
    dispose:batch:2
    finished


`continue batchesLoop` skips the rest of the batch body, then performs the for iterator and tests its condition. `break batchesLoop` exits both loops and reaches the finished entry. Neither returns from the method. A plain break inside a nested switch would exit only that switch; naming the enclosing loop makes the intended target explicit.

## Migration

1. Identify the intended exit and continuation points, including any work after each loop. Give the target loop a descriptive label immediately before its for, foreach, while or do statement.
2. Replace only the inner branches that propagated an outer decision with labeled break or continue. Remove the associated flag and outer flag checks once no other branch uses them.
3. Preserve resource scopes and finally blocks. In this example each reading is disposed before the batch finally runs, then the batch scope is disposed. Resources outside the exited scopes remain alive.
4. Check a skipped batch followed by a normal batch, an early stop followed by an unvisited batch, and normal completion. Compare both results and cleanup order before removing the older implementation.

## Constraints and alternatives

The label must belong to an enclosing target. Only the immediately labeled statement has that label: a label on a block containing a loop does not label the loop, and in `a: b: while (...)` the loop is labeled b. Labeled continue requires an iteration statement; labeled break may also target an enclosing switch. These jumps cannot cross a lambda, local-function or other function boundary. They cannot transfer control out of a finally block.

Cleanup follows the scopes exited by the jump: intervening finally blocks execute, using scopes dispose, and foreach disposes its enumerator when required. The trace demonstrates explicit using resources and finally; it does not separately instrument an enumerator. For continue, an outer resource declared outside the loop remains alive. A finally or Dispose that throws can prevent reaching the jump destination; this example's cleanup does not throw.

The documented application route uses .NET 11 runtime/reference assemblies and the .NET 11 SDK with C# 15. No preview flag, experimental opt-in, extra package or package asset, workload, provider, UI framework or rendering mode is needed. The compiler feature is stable in the canonical RC notes; verification uses RC2 artifacts and is not GA verification. A successful down-level compilation alone would not establish a supported C# 15/.NET 10 route; none is declared here. .NET 10 is the comparison, not an applicable edition for the new syntax.

Keep the flags for an older target when they already express the flow clearly. A helper returning a decision is useful when the traversal deserves its own responsibility. A goto can preserve a specific existing exit point, but jumping back to a loop label restarts that statement rather than performing the for continuation step. Do not replace an outer continue with that jump blindly.

## Evidence

The original .NET 10/C# 14 and .NET 11 RC2/C# 15 projects compile and execute identical expected-trace checks. They exercise partial acceptance before skip, skip followed by a completed batch, stop before later batches, empty batches/input, normal completion and inside-out finally/using cleanup. Saved evidence identifies exact SDKs, sources/configuration hashes, commands, runtime output and dependencies. No mocks, native cross-compilation, GA execution, service resources or extra client/platform matrix are claimed. The portable checks require no targeted Human proof. Official references and full applicability accompany this body in get_recipe.
