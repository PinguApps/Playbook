// snippet:complete
using Microsoft.AspNetCore.Authorization;

namespace JettyAccess;

public static class PolicyMetadata
{
    public static Task<AuthorizationPolicy?> CombineAsync(IAuthorizationPolicyProvider provider)
    {
        // Named policy lookup is a supported way to share the requirement on .NET 10.
        IAuthorizeData[] metadata = [new CrewPermitAttribute()];
        return AuthorizationPolicy.CombineAsync(provider, metadata);
    }
}
// endsnippet:complete
