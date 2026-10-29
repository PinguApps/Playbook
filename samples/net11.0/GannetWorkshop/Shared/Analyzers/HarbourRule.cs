// snippet:rule
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gannet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HarbourRule : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new("GANNET001", "Name the harbour deliberately",
        "Replace harbour type name 'UnsafeHarbour'", "Gannet", DiagnosticSeverity.Warning, isEnabledByDefault: true);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(c =>
        {
            var declaration = (ClassDeclarationSyntax)c.Node;
            if (declaration.Identifier.ValueText == "UnsafeHarbour")
                c.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation()));
        }, SyntaxKind.ClassDeclaration);
    }
}
// endsnippet:rule
