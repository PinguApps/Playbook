// snippet:complete
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
                _ = TransferAndExitAsync(owner, args);
                return true;
            }

            Receipt.Record(args);
            return false;
        })));

    private static async Task TransferAndExitAsync(AppInstance owner, AppActivationArguments args)
    {
        try
        {
            await owner.RedirectActivationToAsync(args).AsTask();
            Environment.Exit(0);
        }
        catch
        {
            Environment.Exit(1);
        }
    }
}
// endsnippet:activation

internal static class Receipt
{
    public static void Record(AppActivationArguments args)
    {
        // Only the fixed, credential-free test URI may enter the receipt.
        string value = args.Data is IProtocolActivatedEventArgs protocol
            ? protocol.Uri.AbsoluteUri == "playbook-tern-pin919://callback/?ticket=919"
                ? protocol.Uri.AbsoluteUri : "Protocol (redacted)"
            : args.Kind.ToString();
        File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "activation.txt"),
            $"{Environment.ProcessId}|{value}{Environment.NewLine}");
    }
}
// endsnippet:complete
