// snippet:construction
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Quay.Collection;

// Unit fixture only: public constructors do not replace production runner events.
var test = new TestCase("Quay.UnitFixture", new Uri("executor://quay/berths/v1"), "fixture.dll");
var start = new TestCaseStartEventArgs(test);
var end = new TestCaseEndEventArgs(test, TestOutcome.Failed);
var before = ObserveCase.End(end);
if (ObserveCase.Start(start).Id != test.Id || before.Id != test.Id
    || before.Name != test.DisplayName || before.IsChild || before.Outcome != "Failed")
    throw new InvalidOperationException("Constructor/snapshot fixture failed.");
if (ObserveCase.End(end) != before || end.TestOutcome != TestOutcome.Failed || end.TestCaseId != test.Id)
    throw new InvalidOperationException("Observation changed the input.");
var expectedPublic = bool.Parse(args[0]);
foreach (var name in new[] { "TestCaseId", "TestCaseName", "IsChildTestCase", "TestElement" })
    CheckSetter(typeof(TestCaseEventArgs), name, expectedPublic);
CheckSetter(typeof(TestCaseEndEventArgs), "TestOutcome", expectedPublic);
Console.WriteLine("QUAY_UNIT:constructor-and-immutable-observation;public-setters=" + expectedPublic);
Console.WriteLine("OBJECTMODEL:" + typeof(TestCaseEventArgs).Assembly.Location);

static void CheckSetter(Type type, string name, bool expected)
{
    var property = type.GetProperty(name) ?? throw new InvalidOperationException("Missing event property.");
    if ((property.GetSetMethod() is not null) != expected)
        throw new InvalidOperationException("Unexpected public setter: " + name);
}
// endsnippet:construction
