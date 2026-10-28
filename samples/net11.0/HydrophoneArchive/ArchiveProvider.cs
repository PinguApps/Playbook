// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace HydrophoneArchive;

// Only the IAccessTokenProvider boundary is controlled. Navigation is the real WASM service.
public sealed class ArchiveProvider(NavigationManager navigation) : IAccessTokenProvider
{
    public string Mode { get; set; } = "success";
    public string LastRequest { get; private set; } = "No request";

    public ValueTask<AccessTokenResult> RequestAccessToken() => RequestAccessToken(new()
    {
        Scopes = ["recordings.read"], ReturnUrl = navigation.ToAbsoluteUri("recordings/reef-17").AbsoluteUri
    });

    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        var applicationBase = new Uri(navigation.BaseUri);
        if (!Uri.TryCreate(options.ReturnUrl, UriKind.Absolute, out var target) || !applicationBase.IsBaseOf(target))
            throw new ArgumentException("Return location must be inside this application.");
        LastRequest = string.Join(",", (options.Scopes ?? [])) + " → " + target.AbsoluteUri;
        var token = new AccessToken
        {
            Value = "controlled-boundary-value-not-issued", Expires = DateTimeOffset.UtcNow.AddMinutes(5),
            GrantedScopes = (options.Scopes ?? []).ToArray()
        };
        if (Mode == "success")
            return ValueTask.FromResult(new AccessTokenResult(AccessTokenResultStatus.Success, token, null, null));
        var interaction = Mode is "fallback" or "modern-fallback" ? null : new InteractiveRequestOptions
        {
            Interaction = InteractionType.GetToken, ReturnUrl = target.AbsoluteUri,
            Scopes = (options.Scopes ?? []).ToArray()
        };
        return ValueTask.FromResult(new AccessTokenResult(
            AccessTokenResultStatus.RequiresRedirect, token, "authentication/login", interaction));
    }
}
// endsnippet:complete
