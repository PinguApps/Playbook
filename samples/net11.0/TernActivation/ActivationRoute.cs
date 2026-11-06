// snippet:activation
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace TernActivation;

public static class ActivationRoute
{
    public static void Configure(MauiAppBuilder builder) => builder.ConfigureLifecycleEvents(events =>
        events.AddWindows(windows => windows.OnAppInstanceActivated((_, args) =>
        {
            AppInstance owner = AppInstance.FindOrRegisterForKey("playbook-tern-dispatch");
            if (!owner.IsCurrent)
            {
                owner.RedirectActivationToAsync(args).AsTask().GetAwaiter().GetResult();
                Environment.Exit(0);
                return true;
            }

            Receipt.Record(args);
            return false;
        })));
}
// endsnippet:activation

internal static class Receipt
{
    public static void Record(AppActivationArguments args)
    {
        string value = args.Data is IProtocolActivatedEventArgs protocol
            ? protocol.Uri.AbsoluteUri : args.Kind.ToString();
        File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "activation.txt"),
            $"{Environment.ProcessId}|{value}{Environment.NewLine}");
    }
}
