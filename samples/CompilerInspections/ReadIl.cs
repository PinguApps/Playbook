using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

public static class SeedIl
{
    public static string[] Inspect(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
            .ToDictionary(c => unchecked((ushort)c.Value));
        string Name(EntityHandle handle) => handle.Kind switch
        {
            HandleKind.MethodSpecification => Name(metadata.GetMethodSpecification((MethodSpecificationHandle)handle).Method),
            HandleKind.MemberReference => metadata.GetString(metadata.GetMemberReference((MemberReferenceHandle)handle).Name),
            HandleKind.MethodDefinition => metadata.GetString(metadata.GetMethodDefinition((MethodDefinitionHandle)handle).Name),
            _ => handle.Kind.ToString()
        };
        var results = new List<string>();
        foreach (var handle in metadata.MethodDefinitions)
        {
            var method = metadata.GetMethodDefinition(handle);
            var name = metadata.GetString(method.Name);
            if (name is not ("opaque" or "control" or "literal")) continue;
            var bytes = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
            var calls = new List<(int Offset, string Name)>();
            var loops = new List<int>();
            for (int pos = 0; pos < bytes.Length;)
            {
                int offset = pos;
                ushort value = bytes[pos++];
                if (value == 0xfe) value = (ushort)(0xfe00 | bytes[pos++]);
                var code = codes[value];
                int size = code.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(bytes, pos),
                    _ => 4
                };
                if (code.OperandType == OperandType.InlineMethod)
                    calls.Add((offset, Name(MetadataTokens.EntityHandle(BitConverter.ToInt32(bytes, pos)))));
                int target = code.OperandType switch
                {
                    OperandType.ShortInlineBrTarget => pos + size + (sbyte)bytes[pos],
                    OperandType.InlineBrTarget => pos + size + BitConverter.ToInt32(bytes, pos),
                    _ => int.MaxValue
                };
                if (target <= offset) loops.Add(target);
                pos += size;
            }
            if (loops.Count == 0) throw new InvalidOperationException($"No loop found: {name}");
            int loopStart = loops.Min();
            int adapts = calls.Count(c => c.Name == "Adapt");
            int fast = calls.Count(c => c.Name == "InvokeFast");
            results.Add($"{name}: loopStart={loopStart}; " + string.Join(", ", calls.Select(c => $"{c.Offset}:{c.Name}")));
            if (name == "opaque" && (adapts != 1 || calls.Single(c => c.Name == "Adapt").Offset >= loopStart || fast != 0 || !calls.Any(c => c.Name == "Invoke" && c.Offset >= loopStart)))
                throw new InvalidOperationException("Opaque callback was not adapted once before the loop");
            if (name == "control" && (adapts != 0 || !calls.Any(c => c.Name == "InvokeFast" && c.Offset >= loopStart)))
                throw new InvalidOperationException("Control no longer demonstrates repeated InvokeFast: " + results.Last());
            if (name == "literal" && adapts != 0) throw new InvalidOperationException("Literal unexpectedly adapted");
        }
        if (results.Count != 3) throw new InvalidOperationException("Missing observed methods");
        return results.ToArray();
    }
}
