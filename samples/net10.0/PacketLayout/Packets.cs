// snippet:program
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

static class PacketInspection
{
    static void Main()
    {
        const string declarations = """
            using System.Runtime.InteropServices;
            [StructLayout(LayoutKind.Sequential, Pack = 2, Size = 24)]
            public struct SeedPacket { public byte Kind; public long Batch; }
            [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 32)]
            public struct SensorPacket { [FieldOffset(0)] public int Reading; [FieldOffset(8)] public long Stamp; }
            [StructLayout(LayoutKind.Auto)] public class WorkPacket { public int Count; }
            """;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var tree = CSharpSyntaxTree.ParseText(declarations);
        var compilation = CSharpCompilation.Create("Packets", [tree], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException(string.Join("\n", errors.Select(d => d.ToString())));
        foreach (var expected in new[] { ("SeedPacket", LayoutKind.Sequential, 2, 24), ("SensorPacket", LayoutKind.Explicit, 1, 32), ("WorkPacket", LayoutKind.Auto, 0, 0) })
        {
            var symbol = compilation.GetTypeByMetadataName(expected.Item1)!;
#if CURRENT
            var layout = symbol.TypeLayout;
            var actual = (layout.Kind, (int)layout.PackingSize, layout.Size);
#else
            // Bounded older fallback for these explicit attributes, not a general metadata-layout decoder.
            var attribute = symbol.GetAttributes().Single(a => a.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.StructLayoutAttribute");
            int Named(string name) => attribute.NamedArguments.Where(p => p.Key == name).Select(p => (int)p.Value.Value!).SingleOrDefault();
            var actual = ((LayoutKind)(int)attribute.ConstructorArguments[0].Value!, Named("Pack"), Named("Size"));
#endif
            if (actual != (expected.Item2, expected.Item3, expected.Item4)) throw new InvalidOperationException($"Layout mismatch: {actual}");
            Console.WriteLine($"{expected.Item1}: {actual} PASS");
        }
        Console.WriteLine($"Runtime={Environment.Version}; Roslyn={typeof(Compilation).Assembly.FullName}");
    }
}
// endsnippet:program
