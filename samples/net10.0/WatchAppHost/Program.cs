// snippet:host
using Aspire.Hosting;
using System.Reflection;

Console.WriteLine($"watch-configuration={Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration}");
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
string dotnet = Environment.GetEnvironmentVariable("DEPOT_DOTNET")
    ?? throw new InvalidOperationException("Set DEPOT_DOTNET to the absolute dotnet executable.");
builder.AddExecutable("rack-audit", dotnet, AppContext.BaseDirectory, "--info");
builder.Build().Run();
// endsnippet:host
