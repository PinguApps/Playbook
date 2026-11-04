// snippet:complete
using Microsoft.Maui.Controls;
using System.Text.Json.Serialization;
namespace MarshDispatch;
internal static class Program { private static async Task Main() {
var web = new HybridWebView();
web.SetInvokeJavaScriptTarget(new SurveyActions(), SurveyJsonContext.Default);
var result = await web.Invoker.InvokeMethodAsync("Record", ["{\"Station\":\"Reedbank\",\"Birds\":4}"]); Console.WriteLine(result); if (result != "{\"Station\":\"Reedbank\",\"Birds\":5}") throw new InvalidOperationException("Wrong generated JSON dispatch");
} }
public sealed class SurveyActions { public SurveyReceipt Record(SurveyRequest request) => new(request.Station, request.Birds + 1); }
public sealed record SurveyRequest(string Station, int Birds);
public sealed record SurveyReceipt(string Station, int Birds);
[JsonSerializable(typeof(SurveyRequest))][JsonSerializable(typeof(SurveyReceipt))]
internal partial class SurveyJsonContext : JsonSerializerContext { }

// endsnippet:complete
