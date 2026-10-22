using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Playbook.Samples;

// snippet:binding
public sealed class HarbourSettings
{
    public int BerthLimit { get; set; } = 4;
    [ConfigurationIgnore]
    public string SessionCredential { get; set; } = "issued-at-runtime";
}

public static class ConfigurationRootChecks
{
    public static void Run()
    {
        using ConfigurationRoot configuration = (ConfigurationRoot)new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["BerthLimit"] = "12", ["SessionCredential"] = "untrusted" }).Build();
        HarbourSettings settings = new();
        configuration.Bind(settings);
        if (settings.BerthLimit != 12 || settings.SessionCredential != "issued-at-runtime")
        {
            throw new Exception("The ignored property was bound.");
        }
    }
}
// endsnippet:binding

// snippet:reload
public sealed class ReloadSubscription : IAsyncDisposable
{
    private readonly IDisposable _registration;
    public ReloadSubscription(Func<IChangeToken?> token, Func<Task> refresh, Action<Exception> report)
    {
        _registration = ChangeToken.OnChange(token, async () =>
        {
            try { await refresh(); }
            catch (Exception error) { report(error); }
        });
    }

    public ValueTask DisposeAsync()
    {
        _registration.Dispose();
        // The framework subscription does not await or cancel work already in flight.
        return ValueTask.CompletedTask;
    }
}
// endsnippet:reload
