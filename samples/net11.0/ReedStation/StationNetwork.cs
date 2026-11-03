// snippet:network-path
using System.Net.NetworkInformation;
namespace Playbook.Samples;
public static class StationNetwork
{
    public static IDisposable WatchReedStation(Action invalidate, Action requestManualRefresh)
    {
        if (OperatingSystem.IsBrowser() || OperatingSystem.IsWasi() || !NetworkChange.IsSupported)

        {
            requestManualRefresh();
            return new Subscription(() => { });
        }
        NetworkAddressChangedEventHandler handler = (_, _) => invalidate();
        NetworkChange.NetworkAddressChanged += handler;
        return new Subscription(() => NetworkChange.NetworkAddressChanged -= handler);
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action? release = unsubscribe;
        public void Dispose() => Interlocked.Exchange(ref release, null)?.Invoke();
    }
    }
// endsnippet:network-path
