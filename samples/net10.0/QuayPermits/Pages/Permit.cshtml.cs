// snippet:complete
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace QuayPermits.Pages;

public sealed class PermitModel(ProtectionMode mode) : PageModel
{
    public ProtectionMode Mode { get; } = mode;
    public string Result { get; private set; } = "";
    public void OnPost(string cargo) => Result = "Page accepted: " + cargo;
}
// endsnippet:complete
