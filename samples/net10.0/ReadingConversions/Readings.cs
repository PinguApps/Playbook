// snippet:program
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.VisualBasic;

static class ReadingInspection
{
    static readonly MetadataReference[] References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)).ToArray();

    static ICoalesceOperation Find(string text, bool visualBasic)
    {
        if (visualBasic) text = text.Replace(" : ", "\n", StringComparison.Ordinal);
        SyntaxTree tree = visualBasic ? VisualBasicSyntaxTree.ParseText(text) : CSharpSyntaxTree.ParseText(text);
        Compilation compilation = visualBasic
            ? VisualBasicCompilation.Create("Readings", [tree], References, new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            : CSharpCompilation.Create("Readings", [tree], References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException(string.Join("\n", errors.Select(d => d.ToString())));
        var model = compilation.GetSemanticModel(tree);
        return tree.GetRoot().DescendantNodes().Select(node => model.GetOperation(node)).OfType<ICoalesceOperation>().Single();
    }

    static void Require(bool condition) { if (!condition) throw new InvalidOperationException("Conversion mismatch"); }
    static void Reject<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }

    static void Main()
    {
        foreach (bool vb in new[] { false, true })
        {
            var numeric = Find(vb
                ? "Public Module Station : Public Function Read(value As Integer?) As Long : Return If(value, 7L) : End Function : End Module"
                : "public static class Station { public static long Read(int? value) => value ?? 7L; }", vb);
            var reference = Find(vb
                ? "Public Module Station : Public Function Read(value As String, fallback As Object) As Object : Return If(value, fallback) : End Function : End Module"
                : "public static class Station { public static object Read(string value, object fallback) => value ?? fallback; }", vb);
            var identity = Find(vb
                ? "Public Module Station : Public Function Read(value As Integer?) As Integer : Return If(value, 7) : End Function : End Module"
                : "public static class Station { public static int Read(int? value) => value ?? 7; }", vb);
            Require(numeric.ValueConversion.IsNumeric && reference.ValueConversion.IsReference && identity.ValueConversion.IsIdentity);
#if CURRENT
            if (vb)
            {
                var conversion = Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetValueConversion(numeric);
                Require(conversion.IsNumeric && conversion.IsWidening);
                Require(Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetValueConversion(reference).IsReference);
                Reject<ArgumentException>(() => Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetValueConversion(numeric));
            }
            else
            {
                var conversion = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetValueConversion(numeric);
                Require(conversion.IsNumeric && conversion.IsImplicit);
                Require(Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetValueConversion(reference).IsReference);
                Reject<ArgumentException>(() => Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetValueConversion(numeric));
            }
#endif
            Console.WriteLine($"{(vb ? "VB If" : "C# ??")}: nullable numeric, reference, identity PASS");
        }
#if CURRENT
        Reject<ArgumentNullException>(() => Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetValueConversion(null!));
        Reject<ArgumentNullException>(() => Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetValueConversion(null!));
        Console.WriteLine("Both language extensions: wrong-language and null rejection PASS");
#endif
        Console.WriteLine($"Runtime={Environment.Version}; Roslyn={typeof(Compilation).Assembly.FullName}");
    }
}
// endsnippet:program
