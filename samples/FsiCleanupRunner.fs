module Playbook.Samples.FsiCleanupRunner

open System
open System.Diagnostics
open System.IO
open System.Runtime.InteropServices

[<EntryPoint>]
let main arguments =
    if arguments.Length <> 0 then failwith "This verification runner accepts no arguments"
    let host = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "../../..", if OperatingSystem.IsWindows() then "dotnet.exe" else "dotnet"))
    if not (File.Exists host) || Path.GetFileNameWithoutExtension(host) <> "dotnet" then
        failwith "A verified dotnet host is required; never launch the sample executable as the host"
    let run script =
        let start = ProcessStartInfo(host)
        start.ArgumentList.Add "fsi"
        start.ArgumentList.Add "--exec"
        start.ArgumentList.Add script
        let directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."))
        start.WorkingDirectory <- directory
        start.UseShellExecute <- false
        start.RedirectStandardOutput <- true
        start.RedirectStandardError <- true
        start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] <- "0"
        use child = Process.Start start |> nonNull
        let output, errors = child.StandardOutput.ReadToEndAsync(), child.StandardError.ReadToEndAsync()
        if not (child.WaitForExit 30000) then child.Kill(true); failwith "FSI timed out"
        let text = output.GetAwaiter().GetResult() + errors.GetAwaiter().GetResult()
        printfn "%s" text
        child.ExitCode, text, directory
    let exitCode, output, directory = run "Cleanup.fsx"
    if exitCode <> 0 || not (output.Contains "PASS: script completion follows cleanup") then failwith "FSI lifetime check failed"
    if File.Exists(Path.Combine(directory, "UnsupportedUse.fsx")) then
        let exitCode, output, _ = run "UnsupportedUse.fsx"
        if exitCode = 0 || not (output.Contains "IDisposable") then failwith "Expected previous FSI asynchronous Using rejection"
        printfn "PASS: previous FSI rejects asynchronous Using; explicit awaited cleanup succeeds"
    if File.Exists(Path.Combine(directory, "ExplicitCleanup.fsx")) then
        let exitCode, output, _ = run "ExplicitCleanup.fsx"
        if exitCode <> 0 || not (output.Contains "PASS: script completion follows cleanup") then failwith "Explicit cleanup alternative failed on the newer FSI"
        printfn "PASS: newer FSI also supports explicit cleanup with propagated cleanup failures"
    0
