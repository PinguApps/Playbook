// snippet:alias
using System.ComponentModel;
using System.Runtime.InteropServices;

internal static class SurveyAlias
{
    internal static void Create(string aliasPath, string existingPath)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows native wrapper");
        if (!CreateHardLinkW(Path.GetFullPath(aliasPath), Path.GetFullPath(existingPath), IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLinkW(string newName, string existingName, IntPtr reserved);
}
// endsnippet:alias
