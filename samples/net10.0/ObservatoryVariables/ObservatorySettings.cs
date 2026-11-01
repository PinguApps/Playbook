// snippet:observatory-settings
using System.Collections;
using Microsoft.Extensions.Configuration;

namespace Playbook.Samples;

internal static class ObservatorySettings
{
    public static IConfigurationRoot Load(string prefix) =>
        new ConfigurationBuilder().Add(new MappedVariables(prefix)).Build();

    private sealed class MappedVariables(string prefix) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(prefix);
    }

    private sealed class Provider(string prefix) : ConfigurationProvider
    {
        public override void Load()
        {
            // This application convention treats triples as dots before pairs as sections.
            static string Map(string name) => string.Join(".", name.Split("___")
                .Select(part => part.Replace("__", ":", StringComparison.Ordinal)));
            string mappedPrefix = Map(prefix);
            Dictionary<string, string?> values = new(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                string key = Map((string)item.Key);
                if (key.StartsWith(mappedPrefix, StringComparison.OrdinalIgnoreCase))
                    values[key[mappedPrefix.Length..]] = (string?)item.Value;
            }
            Data = values;
        }
    }
}
// endsnippet:observatory-settings
