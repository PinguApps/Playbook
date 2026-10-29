// snippet:dispatch
using System.Reflection;

var direct = Skylark.HotRules.Price(7);
var late = (int)typeof(Skylark.HotRules).GetMethod("LatePrice")!.Invoke(null, [7])!;
var cold = Skylark.ColdRules.Label(7);
if (direct != 21 || late != 35 || cold != "SK-7")
{
    throw new InvalidOperationException("Dispatch result changed.");
}
Console.WriteLine($"PASS Skylark dispatch;direct={direct};late={late};cold={cold};runtime={Environment.Version}");
// endsnippet:dispatch
