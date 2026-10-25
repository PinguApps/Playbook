# Keep the fossil desk usable during specimen scoring

## Goal

Let people mark trays in the fictional Westmere fossil collection while a lengthy local calculation runs. Put CPU work in a real browser worker, await the result on the page, and give the worker a clear owner. An asynchronous method alone does not move CPU work off the page's rendering thread. The .NET 11 `blazorwebworker` template supplies the client, messaging and worker runtime boot code; the calculation and ownership below are original.

This is the G25 capability and its E03 expansion: generated value and void calls, creation/invocation cancellation and timeout, plus termination. Keep the distinction between ending a wait and stopping computation visible.

## Older approach

.NET 10 supports ordinary JavaScript Web Workers. This complete comparison performs the calculation in JavaScript and calls it through JS interop. That remains useful when the calculation is already JavaScript or starting a second .NET runtime would cost more than the work. A custom .NET worker bootstrap is another possible design, but is not the older route verified here. SDK 10.0.402 has no `blazorwebworker` template; a documentation version selector or a newer SDK targeting net10.0 cannot establish otherwise.

Create a separate FossilDesk directory. Save these files as FossilDesk.csproj and Directory.Packages.props. Add global.json with sdk.version 10.0.402, sdk.rollForward disable and sdk.allowPrerelease false. The actual language is C# 14 and runtime host is 10.0.13; the explicit application packages are 10.0.9. All project settings are included, without repository parent imports.

{{snippet:project-10}}

{{snippet:packages-10}}

Program.cs and Desk.razor follow. Start browser-only interop after rendering. Release worker resources on an explicit button and component disposal. A pending call is rejected when its owner terminates the worker.

{{snippet:program-10}}

{{snippet:desk-10}}

Save wwwroot/fossil-client.js and wwwroot/fossil-score.js. The client serializes calls, bounds them with a five-second timeout and terminates on timeout. This older client's timeout deliberately has a different policy from the newer generated transport. Its synchronous calculation runs inside the worker, never in a page event handler.

{{snippet:client-10}}

{{snippet:worker-10}}

Use the complete shared document and observer below too. The observer only records actual native workers; it does not implement computation or supply synthetic results.

## Newer approach

Use SDK 11.0.100-rc.2.26504.105 with runtime/packages 11.0.0-rc.2.26504.105. Create adjacent FossilDesk and FossilWorker directories. From a directory with that SDK selected, run dotnet new blazorwebworker -n FossilWorker -o FossilWorker --no-restore. The generated artifact has WebWorkerClient.cs, WorkerMethods.cs and both wwwroot/dotnet-web-worker scripts. Keep the generated client/transport; replace only its starter method and apply the explicit project/package pins below. The complete generated files are also included offline later in this body.

In BOTH directories add global.json with sdk.version 11.0.100-rc.2.26504.105, sdk.rollForward disable and sdk.allowPrerelease true. Set Directory.Packages.props to the following identical contents. The packages and SDK are author-maintained RC2 pins, not a promise about a future GA template.

{{snippet:packages-11}}

Save FossilDesk/FossilDesk.csproj. The project reference packages the actual library assets under _content/FossilWorker. Keep the relative sibling layout.

{{snippet:project-11}}

Save FossilWorker/FossilWorker.csproj. This is a Razor class library, explicitly OutputType Library because it must also build independently outside this repository. AllowUnsafeBlocks enables generated JSExport marshalling; SupportedPlatform browser declares its platform. Interpreted WASM needs no installed wasm-tools workload or native compiler. No AOT build is claimed.

{{snippet:worker-project}}

Save FossilWorker/WorkerMethods.cs. The worker's .NET runtime has its own static state. Label is a real void export, Report returns a string, and Score performs bounded CPU work. The duration makes responsiveness observable on different machines; it is not a benchmark or a meaningful fossil-scoring model. Use supported primitive/string export shapes. Serialize complex data explicitly and account for copying; worker code has no DOM or page DI container.

{{snippet:worker-methods}}

Save FossilDesk/Program.cs and FossilDesk/Desk.razor. The generated client is created after rendering. Production ownership retains the creation task's eventual result: if disposal wins, immediately dispose the late client. The sample intentionally does not pass a lifetime token into CreateAsync because cancellation of the JS await can lose the reference to a still-starting worker. Its startup timeout remains bounded.

{{snippet:program-11}}

{{snippet:desk-11}}

The component uses InvokeVoidAsync to set the shelf, InvokeAsync for calculation/results, and DisposeAsync for actual worker termination. CancelAfter ends a call's managed wait; it does not inject cooperative cancellation into the synchronous Score export. The following diagnostic-only WorkerProbe.cs makes actual generated creation cancellation/timeout observable from browser verification. It is not a recommended production creation pattern. The browser observer cleans up the intentionally orphaned cancellation probe.

{{snippet:probe}}

## Complete shared browser files

Both applications use this wwwroot/index.html, including complete CSS, and wwwroot/fossil-observe.js. Desk.razor declares all imports and its namespace; no _Imports.razor, router or unseen layout is needed. The observer subclasses the native Worker solely to journal creation, messages and termination. Its optional initialization hold is used only by the deterministic cancellation experiment; normal application use leaves it false. Remove diagnostics together with WorkerProbe and its Program assignment when adapting the production app.

{{snippet:document}}

{{snippet:observer}}

## Actual generated .NET 11 client and assets

These are the real RC2 template outputs after namespace substitution, LF normalization and canonical-region markers. They are not a handwritten substitute and do not copy a guide's worked example. Keep these file names and _content paths with the FossilWorker assembly name. If renamed, regenerate consistently. The SDK template package and immutable ASP.NET source were compared; exact hashes and generation provenance are recorded with this recipe's verification.

FossilWorker/WebWorkerClient.cs:

{{snippet:generated-client}}

FossilWorker/wwwroot/dotnet-web-worker-client.js:

{{snippet:generated-transport}}

FossilWorker/wwwroot/dotnet-web-worker.js:

{{snippet:generated-worker}}

The page import map resolves fingerprinted dotnet.js before passing its URL into the worker. Workers do not inherit that import map. Serve both worker JS files and the matching runtime/assembly assets; missing _content files, blocked worker-src/script/WASM policies, or mixed releases prevent boot. This is a same-origin local demonstration with no credentials, account, provider or persistence service.

## Migration

1. Isolate CPU work and its transferable inputs/results. Retain JavaScript workers when they suit the existing calculation. Do not assume Task.Run or an async signature makes browser CPU work nonblocking.
2. On .NET 11 generate the current blazorwebworker library, reference it from the WASM client, preserve both generated scripts and add original JSExport methods. Supply all SDK/package/project/document files above.
3. Create after rendering, retain ownership during creation, serialize this RC2 client's calls and dispose the client when its owner ends. Use call cancellation and timeout to bound waiting; use termination when all worker execution must end. Design a separate cooperative job protocol if partial results or graceful cancellation are required.
4. From each FossilDesk directory, set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0, restore with its matching absolute dotnet executable, then run dotnet run -c Release --no-restore --no-launch-profile --urls http://localhost:5831 for .NET 10 or http://localhost:5832 for .NET 11. Restore downloads pinned WebAssembly SDK/runtime packs. The supplied packages.lock.json captures the authoring closure; standalone reconstruction generates its lock on restore.
5. Select Score specimens and immediately Mark another tray. The count must advance while Computing is still visible. On .NET 11, timeout/cancel the wait and observe Report's completion count still advance. Release worker interrupts its pending CPU call. Reload creates fresh in-memory state.

## Constraints

- WASM/CSR; current blazorwebworker template; see E03. This edition selects .NET 11 only. The actual .NET 10 JavaScript worker comparison is not a new .NET 10 foundation or a compatible generated-client route. Static SSR cannot execute it; Interactive Server is a different server-side design. A hosted application's client can adopt a worker, but hosted/Auto composition is not executed here.
- The reviewed generated client/JSExport route has no Experimental diagnostic. The actual package baseline is RC2, with C# 15 mode and .NET 11 assets; this is prerelease evidence, not GA verification. DevServer is only the local test host. RC2 emits a deprecation warning recommending future Gateway migration; that separate hosting change is outside this recipe.
- Creation timeout rejects and calls native terminate. In-flight creation cancellation cancels the managed JS await, which can lose the reference while the JavaScript creation promise continues. Retain and await eventual creation for ownership; the diagnostic probe intentionally shows the hazard and explicitly cleans it up.
- Invocation timeout removes a pending result; the worker may still finish. Cancellation ends the managed JS await without cancelling the export. The real worker's completed counter proves work continued in both cases. Generated DisposeAsync invokes terminate and releases its JS reference: this forcibly ends the worker, not graceful per-job cancellation or an awaited business rollback.
- RC2 invocation timeout cleanup reads the latest request ID instead of retaining the timed-out call's ID. Concurrent timed-out calls can disturb another pending request. This example serializes operations and does not claim concurrent timeout correctness or patch framework code. A bounded timeout is a waiting policy, not proof of job cancellation or a general memory-cleanup guarantee.
- Separate worker .NET startup/memory and message copying have costs. No speedup, background-tab timing, mobile suspension, AOT or production deployment proof is claimed. Browser refresh/close does not guarantee asynchronous component disposal or durable work; authoritative persistence and durable scheduling need a backend.

## Evidence

Actual SDK 10.0.402/C#14 compiles the older standalone WASM application. Actual SDK 11.0.100-rc.2.26504.105/C#15 compiles the consuming app and generated worker library independently with locked dependencies. These records are compilation, not desktop execution or cross-compilation. No mocked worker/client substitutes for the runtime.

Separate foreground Chrome/native-Worker execution checks rendering and a counter update while the 1200 ms worker calculation is still running, value/void exports and per-worker state, creation timeout and cancellation, invocation timeout and cancellation, and explicit disposal during pending CPU work. The creation cancellation probe temporarily holds the REAL worker's init message to make the race deterministic; after cancellation it releases the message, observes successful real .NET boot, then terminates that intentionally orphaned worker. Neither a held init message nor source review is labelled normal unscheduled timing evidence. The generated invocation path itself is unmodified. Sent Report messages precede the timed/cancelled Score results, proving the caller's wait ended before actual CPU completion.

Exact source inputs, verifier/expression identities and executed binary/WASM/JS assets, including dirty capture provenance, are recorded in https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-792.execution.evidence.json. Reuse requires equality or refreshed execution after relevant changes. The existing actual packed-tool harness independently expands this full canonical body and checks every authored route property, sources, stability, target edition and current compilation evidence through offline real stdio list/get. It rejects target 10.0 for this recipe. Local real-browser proof supplies the required behavioral evidence; no Human-only prerequisite remains for this bounded local route.
