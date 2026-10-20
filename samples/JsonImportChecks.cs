using System.Text.Json;

namespace Playbook.Samples;

internal static class Program
{
    private static void Main()
    {
        Check(BerthReader.Read("{\"Vessel\":\"Wren\",\"Crates\":6}") == new BerthImport("Wren", 6), "valid import");
        foreach (string malformed in new[]
        {
            "{\"Vessel\":\"Wren\"}",
            "{\"Vessel\":\"Wren\",\"Crates\":1,\"Crates\":2}",
            "{\"Vessel\":\"Wren\",\"Crates\":1,\"Colour\":\"red\"}",
            "{\"Vessel\":null,\"Crates\":1}",
            "{\"Vessel\":\"Wren\",\"Crates\":\"6\"}",
            "null"
        })
        {
            try { BerthReader.Read(malformed); throw new Exception("Malformed input accepted: " + malformed); }
            catch (JsonException) { }
        }
        BerthImport structural = BerthReader.Read("{\"Vessel\":\"\",\"Crates\":-1}");
        Check(structural.Crates == -1, "structural checks are not business validation");
        Console.WriteLine($"PASS missing/duplicate/additional/null/numeric rejection and separate business validation; runtime {Environment.Version}");
    }
    private static void Check(bool condition, string claim) { if (!condition) throw new Exception(claim); }
}
