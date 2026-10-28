// snippet:complete
using System.ComponentModel.DataAnnotations;
using MossObservatory;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
// snippet:registration
// .NET 9 has no built-in Minimal API AddValidation registration.
// endsnippet:registration
await using var app = builder.Build();
int handlerCalls = 0;
// snippet:endpoint
app.MapPost("/watches", (WatchRequest request) =>
{
    Interlocked.Increment(ref handlerCalls);
    // This is a business decision, separate from input-shape validation.
    if (request.Station == "closed") { return Results.Conflict(new { reason = "Station is closed" }); }
    return Results.Ok(new { accepted = request.Station });
})
    .AddEndpointFilter(async (context, next) =>
    {
        var request = context.GetArgument<WatchRequest>(0);
        var failures = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), failures, validateAllProperties: true))
        {
            var errors = failures.SelectMany(failure => failure.MemberNames.Select(member => (member, failure.ErrorMessage!)))
                .GroupBy(item => item.member).ToDictionary(group => group.Key, group => group.Select(item => item.Item2).ToArray());
            return Results.ValidationProblem(errors);
        }
        return await next(context);
    });
// endsnippet:endpoint
await MossRequestChecks.RunAsync(app, () => Volatile.Read(ref handlerCalls));
// endsnippet:complete
