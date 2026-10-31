using System.Globalization;
using System.Runtime.InteropServices;
using SeedImports;

Console.WriteLine($"RUNTIME {RuntimeInformation.FrameworkDescription} / {RuntimeInformation.OSDescription}");
string parent = Path.GetFullPath(Path.GetTempPath());
string root = Path.Combine(parent, "playbook-seeds-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    string missing = Path.Combine(root, "missing", "seeds.csv");
    foreach (string culture in new[] { "en-GB", "fr-FR" })
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        foreach ((string name, Action operation, string expected) in new (string, Action, string)[]
        {
            ("read-file", () => File.ReadAllText(missing), missing),
            ("enumerate-directory", () => Directory.GetFiles(Path.Combine(root, "missing")), Path.Combine(root, "missing")),
            ("delete-directory", () => Directory.Delete(Path.Combine(root, "missing")), Path.Combine(root, "missing"))
        })
        {
            DirectoryNotFoundException error = Capture(operation);
#if NET11_0_OR_GREATER
            Check(error.DirectoryPath == expected, "actual filesystem property");
            Console.WriteLine($"FILESYSTEM {culture}/{name}: DirectoryPath={error.DirectoryPath}");
#else
            Check(ImportDiagnostic.SelectPath(error, expected) == expected, "retained older attempted path");
            Console.WriteLine($"FILESYSTEM {culture}/{name}: retained attemptedPath={expected}");
#endif
        }
    }
    string withDots = Path.Combine(root, ".", "missing", "..", "missing", "seeds.csv");
    DirectoryNotFoundException wrapped = Capture(() => ImportDiagnostic.Read(withDots));
    Check(ImportDiagnostic.SelectPath(wrapped) == missing, "normalized original operation path");
    Check(wrapped.InnerException is DirectoryNotFoundException, "inner failure retained");
    foreach (var unknown in new[] { new DirectoryNotFoundException(), new DirectoryNotFoundException("Translated detail"), new DirectoryNotFoundException("detail", (Exception?)null) })
    {
        Check(ImportDiagnostic.SelectPath(unknown) == "(path unavailable)", "unknown fallback");
        Check(ImportDiagnostic.SelectPath(unknown, "retained-input") == "retained-input", "attempted fallback");
    }
#if NET11_0_OR_GREATER
    var inner = new IOException("reader failed");
    var explicitPath = new DirectoryNotFoundException(null, "relative/../seeds");
    Check(explicitPath.DirectoryPath == "relative/../seeds", "constructor retains value without normalization");
    var nested = new DirectoryNotFoundException("import", "seed-path", inner);
    Check(nested.DirectoryPath == "seed-path" && ReferenceEquals(nested.InnerException, inner), "explicit path and inner");
    Check(new DirectoryNotFoundException("detail", directoryPath: null).DirectoryPath is null, "explicit unknown path");
    // An empty constructor path is retained verbatim; callers may treat it as unknown.
    Check(new DirectoryNotFoundException("detail", directoryPath: "").DirectoryPath is { Length: 0 }, "empty path retained");
#endif
    string present = Path.Combine(root, "seeds.csv");
    File.WriteAllText(present, "rowan,17");
    Check(ImportDiagnostic.Read(present) == "rowan,17", "successful import");
    Console.WriteLine("PASS directories: real read/enumerate/delete, two UI cultures, normalized attempted path, success, inner and unknown fallback");
}
finally
{
    // Only the newly created direct child of the resolved temp parent is removed.
    if (Directory.GetParent(Path.GetFullPath(root))?.FullName != Path.TrimEndingDirectorySeparator(parent))
        throw new InvalidOperationException("Unsafe cleanup target");
    Directory.Delete(root, recursive: true);
}

static void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); }
static DirectoryNotFoundException Capture(Action action)
{
    try { action(); }
    catch (DirectoryNotFoundException error) { return error; }
    throw new InvalidOperationException("Expected missing directory");
}
