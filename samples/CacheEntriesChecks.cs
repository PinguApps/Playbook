using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Playbook.Samples;

await CacheExamples.RunAsync();
Console.WriteLine($"PASS cache statistics, metrics and value-derived expiration; runtime={Environment.Version}");
