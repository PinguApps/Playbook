// snippet:complete
using System.Text.Json.Serialization;
using HarbourBodies;
using Microsoft.AspNetCore.SignalR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddControllers();
builder.Services.AddSignalR();
WebApplication app = builder.Build();
app.MapPost("/minimal", (Delivery delivery) => delivery.Value is null ? Results.BadRequest() : Results.Ok(delivery));
app.MapControllers();
app.MapHub<DeliveryHub>("/deliveries");
await BodyChecks.RunAsync(app);

namespace HarbourBodies
{
    [JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
    public union Delivery(LockerPickup, Courier);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record LockerPickup(string LockerCode);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record Courier(string StreetAddress);

    [Microsoft.AspNetCore.Mvc.ApiController]
    [Microsoft.AspNetCore.Mvc.Route("mvc")]
    public sealed class DeliveryController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public Microsoft.AspNetCore.Mvc.ActionResult<Delivery> Echo([Microsoft.AspNetCore.Mvc.FromBody] Delivery delivery) =>
            delivery.Value is null ? BadRequest() : Ok(delivery);
    }

    public sealed class DeliveryHub : Hub
    {
        public Delivery Echo(Delivery delivery) =>
            delivery.Value is null ? throw new HubException("Choose one delivery shape.") : delivery;
    }
}
// endsnippet:complete
