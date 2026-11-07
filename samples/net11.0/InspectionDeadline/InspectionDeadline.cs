namespace InspectionDeadline;

public sealed class BusinessExpiry(TimeProvider clock, DateTimeOffset expiresAt)
{
    private bool expired;

    // snippet:expiry
    public bool IsExpired()
    {
        // Clock policy: forward corrections can expire early; backward corrections
        // delay expiry until UTC catches up. Once observed expired, never reopen.
        expired |= clock.GetUtcNow() >= expiresAt;
        return expired;
    }
    // endsnippet:expiry
}

public static class RetryBudget
{
    // snippet:budget
    public static bool HasElapsed(long startedAt, TimeSpan budget) =>
        Environment.TickCount64 - startedAt >= budget.TotalMilliseconds;
    // endsnippet:budget
}
