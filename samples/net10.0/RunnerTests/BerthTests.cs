// snippet:tests
using Xunit;

namespace Pier.Tests;

public sealed class BerthTests
{
    [Theory]
    [InlineData(18, 5, 13)]
    [InlineData(5, 5, 0)]
    public void ReservationLeavesExpectedBerths(int capacity, int reserved, int expected)
        => Assert.Equal(expected, capacity - reserved);

    [Fact]
    public void DeliberateFailureIsOptIn()
        => Assert.False(Environment.GetEnvironmentVariable("PIER_FAIL") == "1");

    [Fact]
    public async Task DelayIsOptIn()
    {
        if (Environment.GetEnvironmentVariable("PIER_DELAY") == "1")
        {
            await Task.Delay(TimeSpan.FromSeconds(8), TestContext.Current.CancellationToken);
        }
    }
}
// endsnippet:tests
