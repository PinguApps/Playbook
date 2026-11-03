using System.Runtime.InteropServices.JavaScript; namespace Playbook.Samples; public static partial class BrowserChecks { [JSExport] public static string Run() => SupportPaths.Check(); }
