// snippet:complete
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CwmSeedBank;
namespace CwmSeedBank.Tests;
[TestClass] public sealed class ColdStoreTests {
 [TestMethod] public void MinusEighteenIsSafe() { Probe.Record("ColdStore", true); Assert.IsTrue(ColdStore.InRange(-18)); }
 [TestMethod] public void MinusTenIsUnsafe() { Probe.Record("ColdStore", false); Assert.IsFalse(ColdStore.InRange(-10)); }
}
// endsnippet:complete
