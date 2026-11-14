// snippet:complete
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CwmSeedBank;
namespace CwmSeedBank.Tests;
[TestClass] public sealed class GerminationTests {
 [TestMethod] public void EightyPercentIsReady() { Probe.Record("Germination", true); Assert.IsTrue(Germination.Ready(8,10)); }
 [TestMethod] public void BelowThresholdIsNotReady() { Probe.Record("Germination", false); Assert.IsFalse(Germination.Ready(7,10)); }
}
// endsnippet:complete
