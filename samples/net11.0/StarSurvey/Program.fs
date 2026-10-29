// snippet:complete
module StarSurvey.Program

open System
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Hosting.Server
open Microsoft.AspNetCore.Hosting.Server.Features
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main args =
    let run () = task {
        let builder = WebApplication.CreateBuilder(args)
        builder.Logging.ClearProviders() |> ignore
        builder.WebHost.ConfigureKestrel(fun options -> options.Listen(IPAddress.Loopback, 0)) |> ignore
        let mvc = builder.Services.AddControllersWithViews()
        if not (Array.contains "--expect-missing" args) then
            StarSurvey.SurveyViews.register mvc |> ignore
        let app = builder.Build()
        // The negative fixture must observe the missing-view exception, not an unrelated HTTP 500.
        app.Use(Func<HttpContext, Func<Threading.Tasks.Task>, Threading.Tasks.Task>(fun context next -> task {
            try do! next.Invoke()
            with :? InvalidOperationException as error when error.Message.Contains("The view 'Index' was not found") ->
                context.Response.StatusCode <- 500
                do! context.Response.WriteAsync("Missing compiled view: Index")
        })) |> ignore
        app.MapControllers() |> ignore
        try
            do! app.StartAsync()
            let addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses
            use client = new HttpClient(Timeout = TimeSpan.FromSeconds(15.))
            use! response = client.GetAsync(Seq.exactlyOne addresses)
            let! body = response.Content.ReadAsStringAsync()
            let missing = Array.contains "--expect-missing" args
            if missing then
                if response.StatusCode <> HttpStatusCode.InternalServerError || body <> "Missing compiled view: Index" then
                    failwithf "Expected a missing-view 500; received %A: %s" response.StatusCode body
                printfn "PASS build succeeded but actual MVC request failed: missing compiled view (500)."
            else
                if response.StatusCode <> HttpStatusCode.OK || not (body.Contains("Lyra &lt;clear&gt;")) then
                    failwithf "View failed: %A %s" response.StatusCode body
                printfn "PASS real MVC render and model HTML encoding: %s" (body.Trim())
            printfn "PASS runtime=%O; FSharp.Core=%O; MVC=%O" Environment.Version (typeof<int option>.Assembly.GetName().Version) (typeof<Microsoft.AspNetCore.Mvc.Controller>.Assembly.GetName().Version)
        finally
            app.DisposeAsync().AsTask().GetAwaiter().GetResult()
    }
    run().GetAwaiter().GetResult()
    0
// endsnippet:complete
