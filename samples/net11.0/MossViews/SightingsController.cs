// snippet:complete
using Microsoft.AspNetCore.Mvc;

namespace MossObservatory;

public sealed class SightingsController : Controller
{
    [HttpGet("/sightings")]
    public IActionResult Index() => View("Sightings", "<meteor>");
}
// endsnippet:complete
