using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Playbook.Samples;

internal static class Program
{
    private static void Main()
    {
        Notice<int> notice = new Dispatch<int> { Value = 23, Dock = "Quay" };
        string json = JsonSerializer.Serialize(notice);
        Check(JsonSerializer.Deserialize<Notice<int>>(json) is Dispatch<int> { Value: 23, Dock: "Quay" }, "derived contract");
        string generated = JsonSerializer.Serialize(notice, NoticeContext.Default.NoticeInt32);
        Check(JsonSerializer.Deserialize(generated, NoticeContext.Default.NoticeInt32) is Dispatch<int> { Value: 23 }, "generated generic derived contract");
        Check(JsonSerializer.Serialize(new Badge<int>(31)) == "31", "generic converter write");
        Check(JsonSerializer.Deserialize<Badge<string>>("\"Cwm\"") == new Badge<string>("Cwm"), "generic converter read");
        JsonSerializerOptions options = new() { TypeInfoResolver = ManifestContext.Default };
        options.MakeReadOnly();
        Check(ManifestMetadata.Required(options).Type == typeof(Manifest), "typed registered contract");
        Check(ManifestMetadata.TryResolve<Manifest>(options, out JsonTypeInfo<Manifest>? info) && info is not null, "typed success");
        Check(!ManifestMetadata.TryResolve<Uri>(options, out _), "unregistered contract miss");
        try { options.GetTypeInfo(typeof(Uri)); throw new Exception("missing contract accepted"); }
        catch (NotSupportedException) { }
        JsonSerializerOptions broken = new() { TypeInfoResolver = new BrokenResolver() };
        try { ManifestMetadata.TryResolve<Manifest>(broken, out _); throw new Exception("resolver exception swallowed"); }
        catch (InvalidOperationException error) when (error.Message == "resolver failed") { }
#if NET11_0_OR_GREATER
        try { options.GetTypeInfo<Uri>(); throw new Exception("generic missing contract accepted"); }
        catch (NotSupportedException) { }
        Notice<string> text = new Dispatch<string> { Value = "Harbour", Dock = "West" };
        Check(JsonSerializer.Deserialize<Notice<string>>(JsonSerializer.Serialize(text)) is Dispatch<string> { Value: "Harbour" }, "second generic closure");
        try { JsonSerializer.Serialize(new InvalidNotice<int>()); throw new Exception("invalid generic accepted"); }
        catch (InvalidOperationException) { }
#endif
        Console.WriteLine($"PASS derived and converter closure, typed metadata success/miss/resolver failure; runtime {Environment.Version}");
    }
    private static void Check(bool condition, string claim) { if (!condition) throw new Exception(claim); }
    private sealed class BrokenResolver : IJsonTypeInfoResolver
    {
        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) => throw new InvalidOperationException("resolver failed");
    }
}
