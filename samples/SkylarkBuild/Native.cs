// snippet:native
using System.Runtime.InteropServices;

Console.WriteLine($"SKYLARK-NATIVE;crates={7 * 3};architecture={RuntimeInformation.ProcessArchitecture};dynamic={System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported}");
// endsnippet:native
