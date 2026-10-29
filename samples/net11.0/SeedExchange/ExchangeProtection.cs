// snippet:complete
using Microsoft.AspNetCore.Antiforgery;

namespace SeedExchange;

public sealed class ExchangeProtection(ExchangeOrigins origins, Observations observations) : ICsrfProtection
{
    public ValueTask<CsrfProtectionResult> ValidateAsync(HttpContext context)
    {
        bool allowed = HttpMethods.IsGet(context.Request.Method) ||
            HttpMethods.IsHead(context.Request.Method) || HttpMethods.IsOptions(context.Request.Method) ||
            origins.Allows(context.Request);
        observations.ProviderCalled(context);
        return ValueTask.FromResult(allowed ? CsrfProtectionResult.Allowed() : CsrfProtectionResult.Denied());
    }
}
// endsnippet:complete
