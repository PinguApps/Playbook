// snippet:complete
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignalCabin;

await Proof.RunAsync();

namespace SignalCabin
{
    public static class Signals
    {
        public static WebApplication Create()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("fixture", _ => { });
            builder.Services.AddAuthorization(options => options.AddPolicy("Crew", policy => policy.RequireAuthenticatedUser().RequireClaim("crew", "yes")));
            builder.Services.AddCors(options => options.AddPolicy("Chart", policy => policy.WithOrigins("https://chart.example")));
            var app = builder.Build();
            // This trace must run even for standalone responses.
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Cabin-Before"] = "visited";
                await next(context);
            });
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            // This represents processing the standalone signal does not need.
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Cabin-After"] = "visited";
                await next(context);
            });
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            // Public constant response: no identity, CORS policy or downstream work required.
            app.MapGet("/signal/standalone", () => "Cabin open").ShortCircuit();
            // The existing convention remains valid, including its optional status code.
            app.MapGet("/signal/convention", () => "Cabin open").ShortCircuit(202);
            app.MapGet("/signal/ordinary", () => "Cabin open");
            app.MapGet("/crew/manifest", () => "Crew manifest").RequireAuthorization("Crew");
            app.MapGet("/chart", () => "Public chart").RequireCors("Chart");
            return app;
        }
    }
}
// endsnippet:complete
