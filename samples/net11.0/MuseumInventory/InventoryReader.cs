// snippet:partial
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

internal static class InventoryReader
{
    public static void Read(string projectPath)
    {
        using var projects = new ProjectCollection();
        var properties = ProjectInstance.FromFile(projectPath, new ProjectOptions
        {
            ProjectCollection = projects, EvaluationStage = ProjectEvaluationStage.Properties
        });
        Console.WriteLine($"{properties.EvaluationStage}: {properties.GetPropertyValue("CollectionCode")}");
        // This is a second evaluation, not an upgrade of the first instance.
        var items = ProjectInstance.FromFile(projectPath, new ProjectOptions
        {
            ProjectCollection = projects, EvaluationStage = ProjectEvaluationStage.Items
        });
        foreach (var item in items.GetItems("MuseumObject"))
            Console.WriteLine($"{item.EvaluatedInclude}: {item.GetMetadataValue("Gallery")}");
    }
}
// endsnippet:partial
