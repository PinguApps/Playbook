// snippet:complete
using Microsoft.AspNetCore.Authorization;

namespace JettyAccess;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class CrewPermitAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements() => [new CrewPermit()];
}

public sealed record CrewPermit : IAuthorizationRequirement;

public sealed class CrewPermitHandler : AuthorizationHandler<CrewPermit>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CrewPermit requirement)
    {
        if (context.User.HasClaim("jetty", "launch"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
// endsnippet:complete
