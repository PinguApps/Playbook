// snippet:complete
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
namespace ArchiveAccess;

// A local circuit-scoped source makes notification/UI behaviour reproducible.
// An application's trusted authentication integration owns real identity refresh.
public sealed class ArchiveIdentity : AuthenticationStateProvider
{
    private AuthenticationState state = Create("public");
    public int Notifications { get; private set; }
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(state);
    public void ChangeClearance(string clearance)
    {
        state = Create(clearance);
        Notifications++;
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }
    private static AuthenticationState Create(string clearance) => new(new ClaimsPrincipal(new ClaimsIdentity(
        new[] { new Claim(ClaimTypes.NameIdentifier, "mara"), new Claim("clearance", clearance) }, "LocalArchive")));
}
// endsnippet:complete
