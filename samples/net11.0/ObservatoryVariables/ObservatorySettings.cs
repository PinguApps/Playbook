// snippet:observatory-settings
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Playbook.Samples;

internal static class ObservatorySettings
{
    public static IConfigurationRoot Load(string prefix) => new ConfigurationBuilder()
        .AddEnvironmentVariables(source =>
        {
            source.Prefix = prefix;
            source.VariableNameTransformation = EnvironmentVariablesConfigurationSource.ColonAndDotTransformation;
        }).Build();
}
// endsnippet:observatory-settings
