// snippet:legacy
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

internal static class InventoryReader
{
    public static void Read(string projectPath)
    {
        using var projects = new ProjectCollection();
        var full = ProjectInstance.FromFile(projectPath, new ProjectOptions { ProjectCollection = projects });
        Console.WriteLine(full.GetPropertyValue("CollectionCode"));
        foreach (var item in full.GetItems("MuseumObject"))
            Console.WriteLine($"{item.EvaluatedInclude}: {item.GetMetadataValue("Gallery")}");
        Console.WriteLine($"Registered targets: {full.Targets.Count}");
    }
}
// endsnippet:legacy
