// snippet:complete
using ArchiveContracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddControllers();
builder.Services.AddOpenApi("v32", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2);
builder.Services.AddOpenApi("v31", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
builder.Services.AddOpenApi("v30", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0);
WebApplication app = builder.Build();
app.MapOpenApi();
app.MapControllers();

// snippet:unauthorized
app.MapGet("/archive/restricted", () => TypedResults.Unauthorized());
// endsnippet:unauthorized

// snippet:download
app.MapGet("/archive/file", () => TypedResults.File(Archive.Bytes, "application/zip", "soundings.zip"))
    .Produces<FileContentHttpResult>(contentType: "application/zip");
app.MapGet("/archive/stream", () => TypedResults.File(new MemoryStream(Archive.Bytes), "application/zip", "soundings.zip"))
    .Produces<FileStreamHttpResult>(contentType: "application/zip");
// endsnippet:download

// snippet:query
app.MapMethods("/archive/search", ["QUERY"], ([FromBody] SearchFilter filter) => Archive.Search(filter));
// endsnippet:query

// snippet:variants
app.MapGet("/archive/summary", IResult (string? kind) => kind switch
{
    "text" => Results.Text("Soundings: 2", "text/plain"),
    "empty" => Results.Json(new EmptySummary("No readings")),
    _ => Results.Json(new ReadingSummary("Soundings", 2))
})
    .Produces<ReadingSummary>(200, "application/json")
    .Produces<EmptySummary>(200, "application/json")
    .Produces<string>(200, "text/plain")
    .WithName("ReadSummary");
// endsnippet:variants

await ContractChecks.RunAsync(app, newer: true);

namespace ArchiveContracts
{
    // snippet:models
    public sealed record SearchFilter(string Prefix, int MinimumCount);
    public sealed record ReadingSummary(string Name, int Count);
    public sealed record EmptySummary(string Reason);
    public static class Archive
    {
        // An empty ZIP archive, with an end-of-central-directory record.
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
        [ProducesResponseType<FileContentResult>(200, "application/zip")]
        public IActionResult Download() => File(Archive.Bytes, "application/zip", "soundings.zip");

        [HttpGet("stream")]
        [ProducesResponseType<FileStreamResult>(200, "application/zip")]
        public IActionResult StreamDownload() => File(new MemoryStream(Archive.Bytes), "application/zip", "soundings.zip");
        // endsnippet:mvc-download

        // snippet:mvc-variants
        [HttpGet("summary")]
        [ProducesResponseType<ReadingSummary>(200, "application/json")]
        [ProducesResponseType<EmptySummary>(200, "application/json")]
        [ProducesResponseType<string>(200, "text/plain")]
        public IActionResult Summary(string? kind) => kind switch
        {
            "text" => Content("Soundings: 2", "text/plain"),
            "empty" => new JsonResult(new EmptySummary("No readings")),
            _ => new JsonResult(new ReadingSummary("Soundings", 2))
        };
        // endsnippet:mvc-variants
    }
}
// endsnippet:complete
