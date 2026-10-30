Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic

' snippet:program
Module ReadingInspection
    Private Sub Require(condition As Boolean)
        If Not condition Then Throw New InvalidOperationException("VB conversion mismatch")
    End Sub

    Sub Main()
        Dim references = CStr(AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator).
            Select(Function(path) MetadataReference.CreateFromFile(path)).ToArray()
        For Each sample In {"value As Integer?, fallback As Long", "value As String, fallback As Object", "value As Integer?, fallback As Integer"}
            Dim result = If(sample.Contains("String", StringComparison.Ordinal), "Object", If(sample.Contains("Long", StringComparison.Ordinal), "Long", "Integer"))
            Dim tree = VisualBasicSyntaxTree.ParseText($"Public Module Station : Public Function Read({sample}) As {result} : Return If(value, fallback) : End Function : End Module".Replace(" : ", vbLf, StringComparison.Ordinal))
            Dim compilation = VisualBasicCompilation.Create("Readings", {tree}, references,
                New VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            Dim errors = compilation.GetDiagnostics().Where(Function(d) d.Severity = DiagnosticSeverity.Error).ToArray()
            If errors.Length <> 0 Then Throw New InvalidOperationException(String.Join(vbLf, errors.Select(Function(d) d.ToString())))
            Dim model = compilation.GetSemanticModel(tree)
            Dim operation = tree.GetRoot().DescendantNodes().Select(Function(node) model.GetOperation(node)).OfType(Of ICoalesceOperation)().Single()
#If CURRENT Then
            Dim conversion = Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetValueConversion(operation)
            Require(conversion.IsWidening)
#Else
            Dim conversion = operation.ValueConversion
#End If
            Require(If(sample.Contains("String", StringComparison.Ordinal), conversion.IsReference, If(sample.Contains("Long", StringComparison.Ordinal), conversion.IsNumeric, conversion.IsIdentity)))
            Console.WriteLine($"VB-native caller: {sample} PASS")
        Next
        Console.WriteLine($"Runtime={Environment.Version}; Roslyn={GetType(Compilation).Assembly.FullName}")
    End Sub
End Module
' endsnippet:program
