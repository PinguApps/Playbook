// snippet:demo
using System.Reflection;
using System.Security;
using Microsoft.Build.Execution;

namespace Gannet;

public static class WorkshopDemo
{
    public static async Task Run()
    {
        BuildTwoHarbours();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        string child = await CourierLauncher.Launch(Environment.ProcessPath!,
            [Assembly.GetExecutingAssembly().Location, "--courier-child"], [], deadline.Token);
        if (child.Trim() != "Gannet courier ready") throw new InvalidOperationException("Unexpected child response.");
        Console.WriteLine(child.Trim());
    }

    private static void BuildTwoHarbours()
    {
        string folder = Path.Combine(Path.GetTempPath(), "gannet-demo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string project = Path.Combine(folder, "Gannet.proj"), config = Path.Combine(folder, "harbour.txt"), receipt = Path.Combine(folder, "receipt.txt");
            File.WriteAllText(project, $"""
                <Project>
                  <UsingTask TaskName="Gannet.ReadHarbour" AssemblyFile="{SecurityElement.Escape(Assembly.GetExecutingAssembly().Location)}" />
                  <Target Name="Build"><ReadHarbour Receipt="$(Receipt)" Expected="$(Expected)" /></Target>
                </Project>
                """);
            using var cache = new HarbourCache(() => File.ReadAllText(config));
            using var host = new PackageHost(cache);
            foreach (string harbour in new[] { "east-quay", "west-quay" })
            {
                File.WriteAllText(config, harbour);
                var result = host.Build(project, new() { ["Receipt"] = receipt, ["Expected"] = harbour });
                if (result.OverallResult != BuildResultCode.Success ||
                    File.ReadAllText(receipt) != $"{Environment.ProcessId}|{harbour}|{harbour}")
                    throw new InvalidOperationException("The real reused-host build did not read its own configuration.");
                Console.WriteLine($"Gannet build PID={Environment.ProcessId} harbour={harbour}");
            }
        }
        finally { Directory.Delete(folder, true); }
    }
}
// endsnippet:demo
