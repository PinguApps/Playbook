// snippet:network-path
using System.Net.NetworkInformation;
namespace Playbook.Samples;
public static class StationNetwork
{
    public static IDisposable WatchReedStation(Action invalidate, Action requestManualRefresh)
    {
        // Deliberately bounded older route, not a universal support predicate.
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()))

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
