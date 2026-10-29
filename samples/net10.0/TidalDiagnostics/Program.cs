// snippet:report
using System.Runtime.InteropServices;

Console.WriteLine($"station=Gullhaven;mode={AppContext.GetData("TidalStation.DiagnosticMode")};" +
    $"operator={AppContext.GetData("TidalStation.Operator") ?? "none"};runtime={RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"corelib={typeof(object).Assembly.Location}");
// endsnippet:report
