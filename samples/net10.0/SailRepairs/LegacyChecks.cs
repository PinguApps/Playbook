using Microsoft.AspNetCore.Components.Forms;

namespace Playbook.Samples;

// snippet:legacy
public static class LegacyChecks
{
    public static void Run()
    {
#pragma warning disable CS0618 // Historical migration fixtures only; neither supplies the catalogue.
        var first = new EditContext(new RepairJob { Material = "canvas" });
        var returnedContext = first.AddDataAnnotationsValidation();
        CheckMissingService(returnedContext);
        var second = new EditContext(new RepairJob { Material = "canvas" });
        using var subscription = second.EnableDataAnnotationsValidation();
        CheckMissingService(second);
#pragma warning restore CS0618
    }

    private static void CheckMissingService(EditContext context)
    {
        if (context.Validate() || !context.GetValidationMessages().Contains("Material catalogue unavailable."))
            throw new InvalidOperationException("Expected the providerless service-resolution limitation.");
        Console.WriteLine("PASS: providerless fixture cannot resolve MaterialCatalogue.");
    }
}
// endsnippet:legacy
