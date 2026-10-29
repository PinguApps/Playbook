// snippet:task-contract
using Microsoft.TestPlatform.Build.Tasks;
namespace TernTesting;
public static class TaskContract
{
    public static void Configure(VSTestTask task, VSTestTask2 task2)
    {
#if LEGACY_PLATFORM
        task.VSTestCLIRunSettings = ["Tern.Harbour=North Quay", "Tern.Pattern=^Tern\\.Bay/West=13$"];
        task2.VSTestCLIRunSettings = task.VSTestCLIRunSettings;
#else
        task.VSTestCLIRunSettings = "Tern.Harbour=North Quay;Tern.Pattern=^Tern\\.Bay/West=13$";
        task2.VSTestCLIRunSettings = task.VSTestCLIRunSettings;
#endif
    }
}
// endsnippet:task-contract
