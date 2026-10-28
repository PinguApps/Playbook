// snippet:complete
using Microsoft.AspNetCore.Mvc;

namespace QuayArchive;

[ApiController]
[Route("archive/crates")]
public sealed class CratesController : ControllerBase
{
    [HttpGet("{number:int}")]
    public string Get(int number) => $"Crate {number} is ready";
}
// endsnippet:complete
