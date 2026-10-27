// snippet:complete
using ArchiveContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddControllers();
builder.Services.AddOpenApi("v31", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
WebApplication app = builder.Build();
app.MapOpenApi();
app.MapControllers();

// snippet:unauthorized
app.MapGet("/archive/restricted", () => TypedResults.Unauthorized())
    .Produces(StatusCodes.Status401Unauthorized);
// endsnippet:unauthorized

// snippet:download
app.MapGet("/archive/file", () => TypedResults.File(Archive.Bytes, "application/zip", "soundings.zip"))
    .Produces<Stream>(contentType: "application/zip");
app.MapGet("/archive/stream", () => TypedResults.File(new MemoryStream(Archive.Bytes), "application/zip", "soundings.zip"))
    .Produces<Stream>(contentType: "application/zip");
// endsnippet:download

// snippet:query
// QUERY routes work, but the .NET 10 generator omits this operation.
app.MapMethods("/archive/search", ["QUERY"], ([FromBody] SearchFilter filter) => Archive.Search(filter));
// POST is a widely supported body-search contract, without QUERY's safe-method semantics.
app.MapPost("/archive/search", (SearchFilter filter) => Archive.Search(filter));
// endsnippet:query

// snippet:variants
// Separate routes keep every response visible to the .NET 10 document generator.
app.MapGet("/archive/summary/json", () => TypedResults.Ok(new ReadingSummary("Soundings", 2)));
app.MapGet("/archive/summary/empty", () => TypedResults.Ok(new EmptySummary("No readings")));
app.MapGet("/archive/summary/text", () => TypedResults.Text("Soundings: 2", "text/plain"))
    .Produces<string>(200, "text/plain");
// endsnippet:variants

await ContractChecks.RunAsync(app, newer: false);

namespace ArchiveContracts
{
    // snippet:models
    public sealed record SearchFilter(string Prefix, int MinimumCount);
    public sealed record ReadingSummary(string Name, int Count);
    public sealed record EmptySummary(string Reason);
    public static class Archive
    {
        public static readonly byte[] Bytes = [0x50, 0x4b, 0x05, 0x06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        public static ReadingSummary[] Search(SearchFilter filter) =>
            new[] { new ReadingSummary("Soundings", 2), new ReadingSummary("Tides", 1) }
                .Where(item => item.Name.StartsWith(filter.Prefix, StringComparison.Ordinal) && item.Count >= filter.MinimumCount).ToArray();
    }
    // endsnippet:models

    [ApiController]
    [Route("mvc/archive")]
    public sealed class ArchiveController : ControllerBase
    {
        // snippet:mvc-download
        [HttpGet("file")]
        [ProducesResponseType<Stream>(200, "application/zip")]
        public IActionResult Download() => File(Archive.Bytes, "application/zip", "soundings.zip");

        [HttpGet("stream")]
        [ProducesResponseType<Stream>(200, "application/zip")]
        public IActionResult StreamDownload() => File(new MemoryStream(Archive.Bytes), "application/zip", "soundings.zip");
        // endsnippet:mvc-download

        // snippet:mvc-variants
        [HttpGet("summary/json")]
        [ProducesResponseType<ReadingSummary>(200, "application/json")]
        public IActionResult Summary() => new JsonResult(new ReadingSummary("Soundings", 2));

        [HttpGet("summary/empty")]
        [ProducesResponseType<EmptySummary>(200, "application/json")]
        public IActionResult EmptySummary() => new JsonResult(new EmptySummary("No readings"));

        [HttpGet("summary/text")]
        [ProducesResponseType<string>(200, "text/plain")]
        public IActionResult TextSummary() => Content("Soundings: 2", "text/plain");
        // endsnippet:mvc-variants
    }
}
// endsnippet:complete
