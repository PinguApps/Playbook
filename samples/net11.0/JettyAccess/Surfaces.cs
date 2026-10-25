// snippet:complete
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace JettyAccess;

[Microsoft.AspNetCore.Mvc.Route("manifest")]
public sealed class ManifestController : ControllerBase
{
    [HttpGet, CrewPermit]
    public string Read() => "manifest granted";

    // Retain a named policy on the legacy non-endpoint MVC path.
    [HttpGet("legacy-safe"), Authorize(Policy = "crew-permit")]
    public string ReadLegacy() => "manifest granted";
}

public sealed class ManifestHub : Hub
{
    [CrewPermit]
    public string Read() => "manifest granted";
}

// AuthorizeView's normal Policy parameter remains a policy NAME.
// Its protected IAuthorizeData hook permits this reusable metadata adapter.
public sealed class PermitView : AuthorizeView
{
    protected override IAuthorizeData[] GetAuthorizeData() => [new CrewPermitAttribute()];
}

[CrewPermit]
public sealed class ManifestPage : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "manifest granted");
}

public sealed class AccessViews : ComponentBase
{
    [Parameter] public bool Route { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Route)
        {
            builder.OpenComponent<AuthorizeRouteView>(0);
            builder.AddComponentParameter(1, nameof(AuthorizeRouteView.RouteData), new Microsoft.AspNetCore.Components.RouteData(typeof(ManifestPage), new Dictionary<string, object?>()));
            builder.CloseComponent();
        }
        else
        {
            builder.OpenComponent<PermitView>(2);
            builder.AddComponentParameter(3, nameof(PermitView.Authorized), (RenderFragment<AuthenticationState>)(_ => b => b.AddContent(0, "manifest granted")));
            builder.AddComponentParameter(4, nameof(PermitView.NotAuthorized), (RenderFragment<AuthenticationState>)(_ => b => b.AddContent(0, "Not authorized")));
            builder.CloseComponent();
        }
    }
}
// endsnippet:complete
