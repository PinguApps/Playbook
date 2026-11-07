using InspectionDeadline;
using System.Runtime.InteropServices;

DateTimeOffset initial = new(2033, 5, 12, 8, 0, 0, TimeSpan.Zero);
ManualClock clock = new(initial);
BusinessExpiry expiry = new(clock, initial.AddMinutes(10));
Check(!expiry.IsExpired(), "Before expiry");
clock.Now = initial.AddMinutes(10).AddTicks(-1);
Check(!expiry.IsExpired(), "One tick before expiry");
clock.Now = initial.AddMinutes(10);
Check(expiry.IsExpired(), "Exact expiry");
clock.Now = initial.AddHours(-1);
Check(expiry.IsExpired(), "Observed expiry cannot reopen after backward correction");
BusinessExpiry delayed = new(clock, initial.AddMinutes(10));
Check(!delayed.IsExpired(), "Backward correction delays unobserved expiry");
clock.Now = initial.AddMinutes(9);
Check(!delayed.IsExpired(), "Catch-up before boundary");
clock.Now = initial.AddDays(1);
Check(delayed.IsExpired(), "Forward correction expires immediately on next check");
Check(new BusinessExpiry(TimeProvider.System, DateTimeOffset.UtcNow.AddMinutes(-1)).IsExpired(), "Real UTC past expiry");
Check(!new BusinessExpiry(TimeProvider.System, DateTimeOffset.UtcNow.AddMinutes(1)).IsExpired(), "Real UTC future expiry");
Console.WriteLine("PASS: deterministic TimeProvider clock-jump policy; these checks simulate civil-clock inputs, not physical suspend.");

long started = Environment.TickCount64;
int tick32 = Environment.TickCount;
DateTimeOffset utc = DateTimeOffset.UtcNow;
using ManualResetEvent wait = new(false);
bool signaled = wait.WaitOne(120);
long elapsed = Environment.TickCount64 - started;
uint elapsed32 = unchecked((uint)(Environment.TickCount - tick32));
double civil = (DateTimeOffset.UtcNow - utc).TotalMilliseconds;
Check(!signaled, "Unsignaled OS wait times out");
Check(elapsed >= 80 && elapsed < 30000, "Ordinary wait within broad scheduler tolerance");
Check(elapsed32 < 30000, "Bounded unsigned TickCount difference");
Check(RetryBudget.HasElapsed(started, TimeSpan.Zero), "Zero retry budget");
Check(!RetryBudget.HasElapsed(Environment.TickCount64, TimeSpan.FromDays(1)), "Fresh retry budget");
Console.WriteLine($"AWAKE-ONLY observation: TickCount64={elapsed}ms TickCount(unsigned delta)={elapsed32}ms UTC={civil:F3}ms OS-wait-signaled={signaled}; requested=120ms tolerance=[80,30000)ms; no sleep/resume or hibernation executed.");
Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}; OS: {RuntimeInformation.OSDescription}; architecture: {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"CoreLib: {typeof(Environment).Assembly.Location}; version: {typeof(Environment).Assembly.GetName().Version}");

static void Check(bool value, string message)
{
    if (!value) throw new InvalidOperationException(message);
}

sealed class ManualClock(DateTimeOffset initial) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = initial;
    public override DateTimeOffset GetUtcNow() => Now;
}
