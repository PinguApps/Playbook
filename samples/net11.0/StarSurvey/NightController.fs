// snippet:complete
namespace StarSurvey

open Microsoft.AspNetCore.Mvc

[<Route("/")>]
type NightController() =
    inherit Controller()

    [<HttpGet>]
    member this.Index() = this.View("Index", box "Lyra <clear>")
// endsnippet:complete
