// snippet:complete
using Microsoft.AspNetCore.Mvc;

namespace FernPosters;

[Route("posters")]
public sealed class PostersController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
// endsnippet:complete
