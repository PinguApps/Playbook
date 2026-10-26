// snippet:complete
using System.Text.Json;
using System.Text.Json.Serialization;
using HarbourBodies;
using Microsoft.AspNetCore.SignalR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddControllers();
builder.Services.AddSignalR();
WebApplication app = builder.Build();
app.MapPost("/minimal", (Delivery delivery) => delivery is null ? Results.BadRequest() : Results.Ok(delivery));
app.MapControllers();
app.MapHub<DeliveryHub>("/deliveries");
await BodyChecks.RunAsync(app);

namespace HarbourBodies
{
    // .NET 10 retains the same untagged wire contract with an explicit converter.
    [JsonConverter(typeof(DeliveryConverter))]
    public sealed record Delivery(string? LockerCode, string? StreetAddress);

    public sealed class DeliveryConverter : JsonConverter<Delivery>
    {
        public override Delivery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) { throw new JsonException("Expected one delivery object."); }
            JsonProperty[] properties = root.EnumerateObject().ToArray();
            if (properties.Length != 1 || properties[0].Value.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("Choose exactly one delivery shape.");
            }
            return properties[0].Name switch
            {
                "lockerCode" => new Delivery(properties[0].Value.GetString(), null),
                "streetAddress" => new Delivery(null, properties[0].Value.GetString()),
                _ => throw new JsonException("Unknown delivery shape.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Delivery value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value.LockerCode is string locker && value.StreetAddress is null) { writer.WriteString("lockerCode", locker); }
            else if (value.StreetAddress is string address && value.LockerCode is null) { writer.WriteString("streetAddress", address); }
            else { throw new JsonException("Choose exactly one delivery shape."); }
            writer.WriteEndObject();
        }
    }

    [Microsoft.AspNetCore.Mvc.ApiController]
    [Microsoft.AspNetCore.Mvc.Route("mvc")]
    public sealed class DeliveryController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public Microsoft.AspNetCore.Mvc.ActionResult<Delivery> Echo([Microsoft.AspNetCore.Mvc.FromBody] Delivery delivery) =>
            delivery is null ? BadRequest() : Ok(delivery);
    }

    public sealed class DeliveryHub : Hub
    {
        public Delivery Echo(Delivery delivery) =>
            delivery is null ? throw new HubException("Choose one delivery shape.") : delivery;
    }
}
// endsnippet:complete
