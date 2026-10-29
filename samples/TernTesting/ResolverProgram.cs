// snippet:resolver-program
using TernTesting;
if (args.Length != 1) throw new ArgumentException("controlled plugin directory required");
Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
ResolverChecks.Run(args[0]);
// endsnippet:resolver-program
