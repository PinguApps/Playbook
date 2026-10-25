// snippet:complete
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ParcelIntake;

await Proof.RunAsync();

namespace ParcelIntake
{
    public static class Intake
    {
        public static WebApplication Create(Counters counters, bool throwOnBadRequest = false)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            // Development otherwise throws before the filter gets the failed request.
            builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = throwOnBadRequest);
            var app = builder.Build();

            app.MapGet("/intake/{dock}", (int dock, int slots) =>
            {
                counters.Handlers++;
                return slots < 0 ? Results.BadRequest(new { code = "capacity_closed" }) : Results.Ok(new { dock, slots });
            }).AddEndpointFilter(async (context, next) =>
            {
                counters.Filters++;
                // Inspect status before next; failed arguments aren't safe to read.
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    counters.Shaped++;
                    return Failure();
                }

                return await next(context);
            });

            // Control: malformed JSON is an earlier body-read failure in RC2.
            app.MapPost("/notes", (ParcelNote note) =>
            {
                counters.BodyHandlers++;
                return Results.Ok(note);
            }).AddEndpointFilter(async (context, next) =>
            {
                counters.BodyFilters++;
                return await next(context);
            });
            return app;
        }

        public static IResult Failure() => Results.Problem(
            statusCode: 400,
            title: "Parcel intake could not be read",
            detail: "Supply numeric dock and slots values.",
            extensions: new Dictionary<string, object?> { ["code"] = "intake_binding_failed" });
    }

    public sealed class Counters
    {
        public int Filters { get; set; }
        public int Handlers { get; set; }
        public int Shaped { get; set; }
        public int BodyFilters { get; set; }
        public int BodyHandlers { get; set; }
    }

    public sealed record ParcelNote(string Label);
}
// endsnippet:complete
