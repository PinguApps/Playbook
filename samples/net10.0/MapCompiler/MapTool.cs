// snippet:complete
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Text;

namespace MapCompiler;

// Application annotations, not a compiler Annotations API. Values are immutable.
public sealed record MapNote(string Survey);

public sealed class MapNode : ExtensionIntermediateNode
{
    public string Layer { get; set; } = "coast";
    public List<string> Colours { get; } = ["blue"];
    public Dictionary<string, MapNote> Notes { get; } = new(StringComparer.Ordinal);
    public override IntermediateNodeCollection Children { get; } = new();

    public MapNode CopyForTransform()
    {
        var copy = new MapNode { Layer = Layer };
        copy.Colours.Clear();
        copy.Colours.AddRange(Colours);
        foreach (var entry in Notes) copy.Notes.Add(entry.Key, entry.Value);
        CopyCommon(this, copy);
        foreach (var child in Children) copy.Children.Add(CopyMarkup(child));
        return copy;
    }

    // A closed vocabulary: reject unknown nodes rather than dropping their state.
    private static IntermediateNode CopyMarkup(IntermediateNode node)
    {
        IntermediateNode copy = node switch
        {
            HtmlContentIntermediateNode html => new HtmlContentIntermediateNode { HasEncodedContent = html.HasEncodedContent },
            HtmlIntermediateToken token => new HtmlIntermediateToken(token.Content, token.Source),
            _ => throw new NotSupportedException($"Map copy cannot handle {node.GetType().Name}.")
        };
        CopyCommon(node, copy);
        foreach (var child in node.Children) copy.Children.Add(CopyMarkup(child));
        return copy;
    }

    private static void CopyCommon(IntermediateNode source, IntermediateNode copy)
    {
        copy.Source = source.Source;
        copy.IsImported = source.IsImported;
        copy.AddDiagnosticsFromNode(source);
    }

    public override void Accept(IntermediateNodeVisitor visitor) => AcceptExtensionNode(this, visitor);

    public override void WriteNode(CodeTarget target, CodeRenderingContext context)
    {
        // Compiler escaping is required even when a layer name contains quotes.
        context.CodeWriter.Write("WriteLiteral(").Write(Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral($"[{Layer}:{string.Join(",", Colours)}]", quote: true)).WriteLine(");");
        context.RenderChildren(this);
    }
}

public sealed class MapPass : RazorEngineFeatureBase, IRazorOptimizationPass
{
    public int Order => 1000;
    public bool Transform { get; init; } = true;
    public MapNode? Original { get; private set; }
    public MapNode? Working { get; private set; }

    public void Execute(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode, CancellationToken cancellationToken)
    {
        if (!Replace(documentNode)) throw new InvalidOperationException("The map needs one literal markup node.");
    }

    private bool Replace(IntermediateNode parent)
    {
        for (var index = 0; index < parent.Children.Count; index++)
        {
            var child = parent.Children[index];
            if (child is HtmlContentIntermediateNode)
            {
                var original = new MapNode { Source = child.Source, IsImported = child.IsImported };
                original.Notes.Add("survey", new MapNote("harbour-7"));
                original.Children.Add(child);
                Original = original;
                var working = original.CopyForTransform();
                if (Transform)
                {
                    working.Layer = "ridge";
                    working.Colours[0] = "amber";
                    Rewrite(working);
                }
                Working = working;
                parent.Children[index] = working;
                return true;
            }
            if (Replace(child)) return true;
        }
        return false;
    }

    private static void Rewrite(IntermediateNode node)
    {
        if (node is HtmlIntermediateToken token) token.UpdateContent(token.Content.Replace("Estuary", "Ridge", StringComparison.Ordinal));
        foreach (var child in node.Children) Rewrite(child);
    }
}

public static class MapTool
{
    public const string Input = "@inherits MapCompiler.MapTemplate\n<p>Estuary route</p>";

    public static RazorCodeDocument Compile(MapPass pass)
    {
        var engine = RazorProjectEngine.Create(RazorConfiguration.Default, RazorProjectFileSystem.Create("."),
            builder => builder.Features.Add(pass));
        return engine.Process(RazorSourceDocument.Create(Input, "Map.cshtml"), RazorFileKind.Legacy, [], TagHelperCollection.Empty, default);
    }
}

// A tiny execution host for this literal-only generated map, not MVC rendering.
public abstract class MapTemplate
{
    private readonly StringBuilder output = new();
    public void WriteLiteral(string value) => output.Append(value);
    public abstract Task ExecuteAsync();
    public override string ToString() => output.ToString();
}
// endsnippet:complete
