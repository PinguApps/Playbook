// snippet:complete
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Antiforgery;

namespace SeedExchange;

// Local demonstration only: do not expose request diagnostics publicly.
public sealed class Observations
{
    private readonly ConcurrentDictionary<string, Observation> entries = new();
    private Observation Entry(HttpContext context) => entries.GetOrAdd(context.Request.Query["case"].ToString(), _ => new());
    public void ProviderCalled(HttpContext context) => Entry(context).CountProvider();
    public void Handled(HttpContext context) => Entry(context).CountHandler();
    public void Completed(HttpContext context)
    {
        if (!context.Request.Query.ContainsKey("case")) { return; }
        Observation entry = Entry(context);
        entry.Status = context.Response.StatusCode;
        entry.Origin = context.Request.Headers.Origin.ToString();
        entry.FetchSite = context.Request.Headers["Sec-Fetch-Site"].ToString();
        entry.CookiePresent = context.Request.Cookies.ContainsKey("seed-session");
        entry.Valid = context.Features.Get<IAntiforgeryValidationFeature>()?.IsValid;
    }
    public object Snapshot() => entries.ToDictionary(pair => pair.Key, pair => new
    {
        pair.Value.ProviderCalls, pair.Value.HandlerCalls, pair.Value.Status,
        pair.Value.Origin, pair.Value.FetchSite, pair.Value.Valid, pair.Value.CookiePresent
    });
    private sealed class Observation
    {
        public int ProviderCalls { get; private set; }
        public int HandlerCalls { get; private set; }
        public int Status { get; set; }
        public string Origin { get; set; } = "";
        public string FetchSite { get; set; } = "";
        public bool CookiePresent { get; set; }
        public bool? Valid { get; set; }
        public void CountProvider() { lock (this) { ProviderCalls++; } }
        public void CountHandler() { lock (this) { HandlerCalls++; } }
    }
}
// endsnippet:complete
