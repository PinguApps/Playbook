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
Check(end == clock.GetUtcNow().AddMinutes(17), "Identity must calculate the end from the injected clock");
clock.Advance(TimeSpan.FromMinutes(17) - TimeSpan.FromTicks(1));
Check(await manager.IsLockedOutAsync(keeper), "Still locked one tick before expiry");
clock.Advance(TimeSpan.FromTicks(1));
Check(await manager.IsLockedOutAsync(keeper), "RC2 remains locked at the inclusive end timestamp");
clock.Advance(TimeSpan.FromTicks(1));
Check(!await manager.IsLockedOutAsync(keeper), "Unlocked one tick after expiry without editing the stored end");
Check(await manager.GetLockoutEndDateAsync(keeper) == end, "Stored timestamp must remain unchanged");
Console.WriteLine($"PASS: real UserManager, two failures, injected end {end:O}, one tick before, exactly at and one tick after expiry; no sleeping or end-date edits. Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

static void Check(bool success, string message)
{
    if (!success) throw new InvalidOperationException(message);
}
// endsnippet:complete
