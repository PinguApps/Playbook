// snippet:launch
using System.Reflection;
using System.Text.Json;

Assembly assembly = Assembly.GetExecutingAssembly();
Console.WriteLine(JsonSerializer.Serialize(new
{
    Configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration,
    Environment = System.Environment.GetEnvironmentVariable("DEPOT_ZONE"),
    Arguments = args,
}));
// endsnippet:launch
