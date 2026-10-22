using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Playbook.Samples;

// snippet:binding
public sealed class HarbourSettings
{
    public int BerthLimit { get; set; } = 4;
    public string SessionCredential { get; set; } = "issued-at-runtime";
}

public static class ConfigurationRootChecks
{
    public static void Run()
    {
        using ConfigurationRoot configuration = (ConfigurationRoot)new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["BerthLimit"] = "12", ["SessionCredential"] = "untrusted" }).Build();
        HarbourSettings settings = new();
        // An allowlist controls which values cross into the runtime model.
        settings.BerthLimit = configuration.GetValue<int>("BerthLimit");
        if (settings.BerthLimit != 12 || settings.SessionCredential != "issued-at-runtime")
        {
            throw new Exception("The selective binding boundary changed.");
        }
    }
}
// endsnippet:binding

// snippet:reload
public sealed class ReloadSubscription : IAsyncDisposable
{
    private readonly Channel<bool> _pending = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        SingleReader = true, FullMode = BoundedChannelFullMode.DropWrite,
    });
    private readonly IDisposable _registration;
    private readonly Task _worker;

    public ReloadSubscription(Func<IChangeToken?> token, Func<Task> refresh, Action<Exception> report)
    {
        _worker = ProcessAsync(refresh, report);
        _registration = ChangeToken.OnChange(token, () => { _pending.Writer.TryWrite(true); });
    }

    private async Task ProcessAsync(Func<Task> refresh, Action<Exception> report)
    {
        await foreach (bool _ in _pending.Reader.ReadAllAsync())
        {
            try { await refresh(); }
            catch (Exception error) { report(error); }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _registration.Dispose();
        _pending.Writer.TryComplete();
        await _worker; // This application owns and joins its worker.
    }
}
// endsnippet:reload
