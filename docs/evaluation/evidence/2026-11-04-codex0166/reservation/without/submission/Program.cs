using ReedReservations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Availability>();
var app = builder.Build();
app.MapPost("/reservations", (Reservation request, Availability availability) =>
{
    availability.Accept(request.Code);
    return Results.NoContent();
}).AddEndpointFilter(async (context, next) =>
{
    var cancellationToken = context.HttpContext.RequestAborted;
    var availability = context.HttpContext.RequestServices.GetRequiredService<Availability>();
    var errors = await context.GetArgument<Reservation>(0).ValidateAsync(availability, cancellationToken);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    cancellationToken.ThrowIfCancellationRequested();
    return await next(context);
});
// Local evaluation instrumentation. Keep these routes and the service unchanged.
app.MapPost("/control/arm/{code}", (string code, Availability service) => service.Arm(code));
app.MapPost("/control/release/{code}", (string code, Availability service) => service.Release(code));
app.MapGet("/control/{code}", (string code, Availability service) => service.Snapshot(code));
app.Run();
