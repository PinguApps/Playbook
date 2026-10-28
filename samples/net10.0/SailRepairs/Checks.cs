using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Playbook.Samples;

public static class Checks
{
    public static void Run(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var catalogue = scope.ServiceProvider.GetRequiredService<MaterialCatalogue>();
        using var other = services.CreateScope();
        Require(catalogue.ScopeId != other.ServiceProvider.GetRequiredService<MaterialCatalogue>().ScopeId, "Real scoped catalogue");
        var editor = new RepairEditor(scope.ServiceProvider);
        for (var generation = 0; generation < 3; generation++)
        {
            editor.Model.Material = "plastic";
            var before = catalogue.Checks;
            Require(!editor.Context.Validate(), "Unsupported material rejected");
            Require(catalogue.Checks == before + 1, "Exactly one current validation handler");
            Require(editor.Context.GetValidationMessages().Single() == "Choose canvas or sailcloth.", "One current message");
            editor.Model.Material = "canvas";
            Require(editor.Context.Validate(), "Permitted material accepted");
            var old = editor.Context;
            editor.Replace();
            before = catalogue.Checks;
            old.NotifyFieldChanged(new FieldIdentifier(old.Model, nameof(RepairJob.Material)));
            old.Validate();
            Require(catalogue.Checks == before && !old.GetValidationMessages().Any(), "Old subscription disposed");
        }
        var final = editor.Context;
        editor.Dispose();
        var finalCount = catalogue.Checks;
        final.Validate();
        Require(catalogue.Checks == finalCount, "Final subscription disposed");
#if NET10_0
        LegacyChecks.Run();
#endif
        Console.WriteLine($"PASS: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; scoped DI, rejection/acceptance, three replacements, one handler and final disposal.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
