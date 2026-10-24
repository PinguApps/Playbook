// snippet:complete
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
namespace QuayPermits;

public sealed class PermitController(IAntiforgery antiforgery, ProtectionMode mode) : Controller
{
    [HttpGet("/mvc")]
    public ContentResult Form() => Content(FormDocument.Create("/mvc", HttpContext, antiforgery, mode.KeepTokens), "text/html");

    [HttpPost("/mvc")]
    [ValidateAntiForgeryToken]
    public ContentResult Submit([FromForm] string cargo) => Content("MVC accepted: " + cargo);
}
// endsnippet:complete
