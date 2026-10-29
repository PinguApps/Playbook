// snippet:complete
namespace StarSurvey

open System
open System.IO
open System.Reflection
open System.Runtime.Loader
open Microsoft.AspNetCore.Mvc.ApplicationParts
open Microsoft.Extensions.DependencyInjection

module SurveyViews =
    let register (mvc: IMvcBuilder) =
        let owner = typeof<NightController>.Assembly
        let context = AssemblyLoadContext.GetLoadContext(owner)
        let path = Path.Combine(AppContext.BaseDirectory, "StarSurvey.Views.dll")
        // The legacy Razor compiler emits a separate C# view assembly beside this F# assembly.
        let compiled = context.LoadFromAssemblyPath(path)
        if AssemblyLoadContext.GetLoadContext(compiled) <> context then
            failwith "The view assembly loaded into a different context."
        mvc.PartManager.ApplicationParts.Add(CompiledRazorAssemblyPart(compiled))
        printfn "VIEWS %s context=%s same-owner-context=true" compiled.Location context.Name
        mvc
// endsnippet:complete
