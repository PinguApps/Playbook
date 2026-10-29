// snippet:checker
using System.Runtime.InteropServices;
using System.Text.Json;

if (args.Length == 0)
{
    Console.WriteLine("museum=Gullhaven;catalogue=not-supplied");
    return;
}

var cards = File.ReadAllLines(args[0]);
var unique = cards.Distinct(StringComparer.Ordinal).Count();
Console.WriteLine($"museum=Gullhaven;cards={cards.Length};unique={unique}");
Console.WriteLine($"arguments={JsonSerializer.Serialize(args)}");
Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription};corelib={typeof(object).Assembly.Location}");
Environment.ExitCode = unique == cards.Length ? 0 : 7;
// endsnippet:checker
