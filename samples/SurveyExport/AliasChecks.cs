using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Measured Windows NTFS route only");
Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
string parent = Path.GetFullPath(Path.GetTempPath());
string tree = Path.Combine(parent, "playbook-survey-" + Guid.NewGuid().ToString("N"));
var drive = new DriveInfo(Path.GetPathRoot(tree)!);
if (drive.DriveFormat != "NTFS") throw new PlatformNotSupportedException("NTFS proof requires NTFS");
Console.WriteLine($"filesystem={drive.DriveFormat}; driveType={drive.DriveType}; root={drive.RootDirectory}; fixture={tree}; kernel32={Environment.SystemDirectory}/kernel32.dll");
Directory.CreateDirectory(tree);
try
{
    Exercise(SurveyAlias.Create, "canonical");
#if NET11_0
    Exercise(SurveyAlias.CreateWithInfo, "FileInfo receiver=new-alias");
#endif
}
finally
{
    if (Path.GetDirectoryName(Path.GetFullPath(tree)) != parent.TrimEnd(Path.DirectorySeparatorChar)) throw new InvalidOperationException("Cleanup outside temporary parent");
    Directory.Delete(tree, recursive: true);
    Console.WriteLine($"CLEANUP temporary tree removed={ !Directory.Exists(tree) }");
}
Console.WriteLine("PASS hard-link lifecycle; other filesystems/OS/cross-volume outcomes not measured");

void Exercise(Action<string, string> create, string name)
{
    string existing = Path.Combine(tree, Guid.NewGuid().ToString("N") + ".survey");
    string alias = existing + ".approved";
    const string original = "Survey transect M4: depth=18.25";
    const string updated = "Survey transect M4: depth=19.75; tide=falling";
    File.WriteAllText(existing, original);
    create(alias, existing);
    Check(File.ReadAllText(alias) == original, "alias reads original");
    File.WriteAllText(alias, updated);
    Check(File.ReadAllText(existing) == updated, "alias mutation reaches original");
    File.AppendAllText(existing, "; checked");
    Check(File.ReadAllText(alias) == updated + "; checked", "original mutation reaches alias");
    Reject(() => create(alias, existing), "destination-exists");
    Check(File.ReadAllText(alias) == updated + "; checked", "failed destination preserves content");
    Reject(() => create(existing + ".missing-alias", existing + ".absent"), "absent-target");
    Check(!File.Exists(existing + ".missing-alias"), "absent target creates no alias");
    Reject(() => create(existing + ".directory-alias", tree), "directory-target");
    File.Delete(existing);
    Check(File.ReadAllText(alias) == updated + "; checked", "deleting original preserves alias data");
    Console.WriteLine($"PASS {name}; bidirectional mutation=True; remainingSha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(alias)))}");
    File.Delete(alias);
    // Also remove the alias first, leaving the original intact.
    File.WriteAllText(existing, original);
    create(alias, existing);
    File.Delete(alias);
    Check(File.ReadAllText(existing) == original, "deleting alias preserves original");
    File.Delete(existing);
}
static void Reject(Action action, string label)
{
    try { action(); throw new InvalidOperationException("Expected rejection: " + label); }
    catch (Exception ex) when (ex is IOException or Win32Exception or UnauthorizedAccessException)
    {
        Console.WriteLine($"PASS {label}; exception={ex.GetType().Name}; hresult=0x{ex.HResult:X8}; win32={(ex as Win32Exception)?.NativeErrorCode}");
    }
}
static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
