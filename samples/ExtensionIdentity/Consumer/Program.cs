using Microsoft.Extensions.Options;
using Playbook.Samples;
if (Schedule.Label(Options.Create("dusk")) != "show:dusk") throw new Exception("Library mismatch");
Console.WriteLine("PASS Consumer " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + " " + typeof(IOptions<>).Assembly.Location);
