using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Playbook.Samples;

await StartupExamples.RunAsync();
Console.WriteLine($"PASS DI validator; asynchronous startup success and rejection; runtime={Environment.Version}");
