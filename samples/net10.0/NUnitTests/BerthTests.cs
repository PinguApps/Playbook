// snippet:tests
using NUnit.Framework;

namespace Pier.Tests;

public sealed class BerthTests
{
    [TestCase(18, 5, 13)]
    [TestCase(5, 5, 0)]
    public void ReservationLeavesExpectedBerths(int capacity, int reserved, int expected)
        => Assert.That(capacity - reserved, Is.EqualTo(expected));
}
// endsnippet:tests
