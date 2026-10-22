module Playbook.Samples.CompilerHostChecks

open System
open System.IO
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Diagnostics

// snippet:host
// Keep a checker for a workspace lifetime, rather than recreating it per file.
let checkProject (checker: FSharpChecker) loadedAt projectFile references sourceFile =
    let arguments =
        Array.concat [
            [| "--target:library"; "--targetprofile:netcore"; "--noframework" |]
            references |> Array.map (fun path -> "-r:" + path)
            [| sourceFile |]
        ]
    let options = checker.GetProjectOptionsFromCommandLineArgs(projectFile, arguments, loadedTimeStamp = loadedAt)
    checker.ParseAndCheckProject options |> Async.RunSynchronously
// endsnippet:host

let require condition message = if not condition then failwith message

let run (makeCheckers: unit -> (string * FSharpChecker) array) =
    let scratch = Path.Combine(Path.GetTempPath(), "playbook-survey-host-" + Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory scratch |> ignore
    try
        let references =
            (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") |> string).Split(Path.PathSeparator)
            |> Array.filter (fun path -> not ((Path.GetFileName(path) |> string).StartsWith("FSharp.Compiler", StringComparison.Ordinal)))
        let compiler = FSharpChecker.Create()
        let contract = Path.Combine(scratch, "SurveyContract.dll")
        let extension = Path.Combine(scratch, "SurveyExtension.dll")
        let compile (path: string) (code: string) =
            let source = Path.ChangeExtension(path, ".fs") |> string
            File.WriteAllText(source, code)
            let args = Array.concat [ [| "fsc.exe"; "--target:library"; "--targetprofile:netcore"; "--noframework"; "--out:" + path |]; references |> Array.map (fun r -> "-r:" + r); [| source |] ]
            let diagnostics, _ = compiler.Compile args |> Async.RunSynchronously
            require (File.Exists path && diagnostics |> Array.forall (fun d -> d.Severity <> FSharpDiagnosticSeverity.Error)) (sprintf "Fixture compilation failed: %A" diagnostics)
        compile contract "namespace SurveyContract\ntype Gauge = { Depth: int }"
        compile extension "namespace SurveyExtension\ntype Marker = class end"
        let first, second = Path.Combine(scratch, "East.fs"), Path.Combine(scratch, "West.fs")
        let writeFiles depth =
            File.WriteAllText(first, "module East\nlet gauge: SurveyContract.Gauge = { Depth = " + depth + " }\nlet measured = gauge.Depth")
            File.WriteAllText(second, "module West\nlet gauge: SurveyContract.Gauge = { Depth = " + depth + " }\nlet measured = gauge.Depth\nlet extra: SurveyExtension.Marker option = None")
        writeFiles "17"
        let checkers = makeCheckers ()
        let loadedAt = DateTime.UtcNow
        let check name checker (file: string) extra =
            let refs = Array.concat [references; [|contract|]; extra]
            let result = checkProject checker loadedAt (Path.Combine(scratch, name + (Path.GetFileName(file) |> string) + ".fsproj")) refs file
            let errors = result.Diagnostics |> Array.filter (fun d -> d.Severity = FSharpDiagnosticSeverity.Error)
            let diagnostics = result.Diagnostics |> Array.map (fun d -> d.Severity, d.ErrorNumber, d.Message) |> Array.sort
            let signature = result.AssemblySignature.Entities |> Seq.collect (fun e -> e.MembersFunctionsAndValues) |> Seq.map (fun m -> m.DisplayName, m.FullType.Format(FSharp.Compiler.Symbols.FSharpDisplayContext.Empty)) |> Seq.sort |> Seq.toArray
            errors, signature, diagnostics
        let initial = ResizeArray<_>()
        for name, checker in checkers do
            for file, extra in [first, [||]; second, [|extension|]] do
                let errors, signature, diagnostics = check name checker file extra
                require (errors.Length = 0 && signature |> Array.exists (fun (n,t) -> n = "measured" && t.Contains("int", StringComparison.Ordinal))) (sprintf "%s initial check failed: %A / %A" name errors signature)
                initial.Add(file, signature, diagnostics)
            // Same binary and same reference closure, distinct project identity.
            let errors, _, _ = check (name + "Shared") checker first [||]
            require (errors.Length = 0) "Shared closure changed diagnostics"
        for file in [first; second] do
            let results = initial |> Seq.filter (fun (f, _, _) -> f = file) |> Seq.map (fun (_, signature, diagnostics) -> signature, diagnostics) |> Seq.toArray
            require (results |> Array.forall ((=) results[0])) "Sharing changed intended symbols or diagnostics"
        // Replace the actual binary at the same path; FCS imports metadata, not a loaded fixture type.
        compile contract "namespace SurveyContract\ntype Gauge = { Depth: string }"
        File.SetLastWriteTimeUtc(contract, DateTime.UtcNow.AddSeconds(2.0))
        writeFiles "\"seventeen\""
        let updated = ResizeArray<_>()
        let isolated = ResizeArray<_>()
        for name, checker in checkers do
            for file, extra in [first, [||]; second, [|extension|]] do
                let errors, signature, diagnostics = check name checker file extra
                require (errors.Length = 0 && signature |> Array.exists (fun (n,t) -> n = "measured" && t.Contains("string", StringComparison.Ordinal))) (sprintf "%s retained stale imports: %A / %A" name errors signature)
                updated.Add(file, signature, diagnostics)
            // A project lacking the extension must still reject its symbols.
            let errors, _, diagnostics = check (name + "Isolated") checker second [||]
            require (errors.Length > 0) "Reference closure leaked symbols"
            isolated.Add diagnostics
        for file in [first; second] do
            let results = updated |> Seq.filter (fun (f, _, _) -> f = file) |> Seq.map (fun (_, signature, diagnostics) -> signature, diagnostics) |> Seq.toArray
            require (results |> Array.forall ((=) results[0])) "Updated imports changed symbols or diagnostics between sharing policies"
        require (isolated |> Seq.forall ((=) isolated[0])) "Sharing changed missing-reference diagnostics"
        printfn "PASS: %A; shared binary/closure and differing closures; int -> string reference replacement; isolated diagnostics; runtime %O; FCS %O" (checkers |> Array.map fst) Environment.Version (typeof<FSharpChecker>.Assembly.GetName().Version)
    finally
        Directory.Delete(scratch, true)
