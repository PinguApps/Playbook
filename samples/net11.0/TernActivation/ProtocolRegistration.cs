// snippet:complete
using Microsoft.Windows.AppLifecycle;

namespace TernActivation;

internal static class ProtocolRegistration
{
    public static void RunRequestedAction()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        if (arguments.Contains("--register", StringComparer.Ordinal))
        {
            ActivationRegistrationManager.RegisterForProtocolActivation(
                "playbook-tern-pin919", "", "Playbook Tern sample", Environment.ProcessPath!);
            Environment.Exit(0);
        }
        if (arguments.Contains("--unregister", StringComparer.Ordinal))
        {
            ActivationRegistrationManager.UnregisterForProtocolActivation(
                "playbook-tern-pin919", Environment.ProcessPath!);
            Environment.Exit(0);
        }
    }
}

// endsnippet:complete
