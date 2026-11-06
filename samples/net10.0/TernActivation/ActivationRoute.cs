// snippet:complete
// snippet:activation
using Microsoft.Maui.Hosting;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace TernActivation;

public static class ActivationRoute
{
    public static void Configure(MauiAppBuilder builder) { }

    public static bool Redirect()
    {
        AppInstance owner = AppInstance.FindOrRegisterForKey("playbook-tern-dispatch");
        AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
        if (!owner.IsCurrent)
        {
            owner.RedirectActivationToAsync(args).AsTask().GetAwaiter().GetResult();
            return true;
        }

        AppInstance.GetCurrent().Activated += (_, activation) => Receipt.Record(activation);
        Receipt.Record(args);
        return false;
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
