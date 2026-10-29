// snippet:complete
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
namespace CatalogueCompiler;

public static class CatalogueTool
{
    public static RazorCSharpDocument Compile(string input)
    {
        var engine = RazorProjectEngine.Create(RazorConfiguration.Default, RazorProjectFileSystem.Create("."));
        var source = RazorSourceDocument.Create(input, "Catalogue.cshtml");
        return engine.Process(source, RazorFileKind.Legacy, [], TagHelperCollection.Empty, default).GetCSharpDocument();
    }

    public static RazorCSharpDocument Copy(RazorCodeDocument document, RazorCSharpDocument generated,
        bool declaration = false, bool stub = false) =>
        new(document, generated.Text, generated.Diagnostics, generated.SourceMappingsSortedByGenerated, generated.LinePragmas,
            isDeclarationDocument: declaration, isStubDocument: stub);

    public static string Read(RazorCSharpDocument document) => document.GeneratedCode;

    public static (int Imported, int AfterAdd) DiagnosticCounts(RazorSourceDocument source, RazorDiagnostic diagnostic, bool report)
    {
        var node = new DocumentIntermediateNode();
        node.AddDiagnostic(diagnostic);
        using var context = new CodeRenderingContext(new DiagnosticWriter(), source,
            node, RazorCodeGenerationOptions.Default, reportDiagnostics: report);
        var imported = context.GetDiagnostics().Length;
        context.AddDiagnostic(diagnostic);
        return (imported, context.GetDiagnostics().Length);
    }
    // Only the diagnostic accumulator is exercised here; no custom node rendering is supplied.
    private sealed class DiagnosticWriter : IntermediateNodeWriter;
}
// endsnippet:complete
