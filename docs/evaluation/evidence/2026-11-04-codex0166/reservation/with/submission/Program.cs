using ReedReservations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Availability>();
builder.Services.AddValidation();
var app = builder.Build();
app.MapPost("/reservations", (Reservation request, Availability availability) =>
{
    availability.Accept(request.Code);
    return Results.NoContent();
});
// Local evaluation instrumentation. Keep these routes and the service unchanged.
app.MapPost("/control/arm/{code}", (string code, Availability service) => service.Arm(code));
app.MapPost("/control/release/{code}", (string code, Availability service) => service.Release(code));
app.MapGet("/control/{code}", (string code, Availability service) => service.Snapshot(code));
app.Run();
