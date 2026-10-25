// snippet:complete
using MooringClock;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

ManualClock clock = new(new DateTimeOffset(2032, 4, 8, 9, 0, 0, TimeSpan.Zero));
ServiceCollection services = new();
services.AddLogging();
services.AddSingleton<TimeProvider>(clock);
services.AddIdentityCore<IdentityUser>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 2;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(17);
}).AddUserStore<MemoryLockoutStore>();
using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
UserManager<IdentityUser> manager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
IdentityUser keeper = new("MooringKeeper") { LockoutEnabled = true };
Check((await manager.CreateAsync(keeper)).Succeeded, "Create keeper");
Check((await manager.AccessFailedAsync(keeper)).Succeeded, "First failure");
Check(!await manager.IsLockedOutAsync(keeper), "One failure must not lock");
Check((await manager.AccessFailedAsync(keeper)).Succeeded, "Second failure");
Check(await manager.IsLockedOutAsync(keeper), "Two failures must lock");
DateTimeOffset? end = await manager.GetLockoutEndDateAsync(keeper);
// Registering a TimeProvider does not redirect UserManager's .NET 10 wall clock.
Check(end > DateTimeOffset.UtcNow.AddMinutes(16) && end < DateTimeOffset.UtcNow.AddMinutes(18), "Wall-clock lockout end");
clock.Advance(TimeSpan.FromDays(1));
Check(await manager.IsLockedOutAsync(keeper), "Advancing the injected clock does not expire this .NET 10 lockout");
// Valid older fixture technique: arrange an already-expired stored timestamp.
// This tests the expiry decision, not automatic passage through its boundary.
Check((await manager.SetLockoutEndDateAsync(keeper, DateTimeOffset.UtcNow.AddMinutes(-1))).Succeeded, "Arrange expired state");
Check(!await manager.IsLockedOutAsync(keeper), "Expired stored state must unlock");
Console.WriteLine($"PASS: real .NET 10 UserManager ignores injected clock; future/past stored-state checks without sleeping. Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

static void Check(bool success, string message)
{
    if (!success) throw new InvalidOperationException(message);
}
// endsnippet:complete
