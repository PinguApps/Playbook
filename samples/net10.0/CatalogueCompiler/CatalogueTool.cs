// snippet:complete
using Microsoft.AspNetCore.Razor.Language;
namespace CatalogueCompiler;

public static class CatalogueTool
{
    // The existing host supplies its compiler output. This SDK has no public output reader.
    public static RazorCSharpDocument Copy(RazorCodeDocument document, RazorCSharpDocument generated) =>
        new(document, generated.Text, generated.Diagnostics, generated.SourceMappingsSortedByGenerated, generated.LinePragmas);

    public static string Read(RazorCSharpDocument document) => document.Text.ToString();
}
// endsnippet:complete
