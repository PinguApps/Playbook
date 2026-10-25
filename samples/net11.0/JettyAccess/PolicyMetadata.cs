// snippet:complete
using Microsoft.AspNetCore.Authorization;

namespace JettyAccess;

public static class PolicyMetadata
{
    public static Task<AuthorizationPolicy?> CombineAsync(IAuthorizationPolicyProvider provider)
    {
        // object[] deliberately selects the new metadata overload, rather than
        // the older IEnumerable<IAuthorizeData> overload that drops requirements.
        object[] metadata = [new CrewPermitAttribute(), new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()];
        return AuthorizationPolicy.CombineAsync(provider, metadata);
    }
}
// endsnippet:complete
