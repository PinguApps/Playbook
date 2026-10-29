// snippet:legacy-session
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
namespace TernTesting;
public static class LegacySession
{
    // The old pooled-session API was explicitly marked unfinished by its publisher.
#pragma warning disable CS0618
    public static ITestSession? Start(IVsTestConsoleWrapper wrapper, IList<string> sources, ITestSessionEventsHandler events)
        => wrapper.StartTestSession(sources, "<RunSettings />", events);
    public static bool Stop(ITestSession session) => session.StopTestSession();
#pragma warning restore CS0618
}
// endsnippet:legacy-session
