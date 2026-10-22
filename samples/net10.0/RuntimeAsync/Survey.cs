using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

// snippet:workflow
static class Survey
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<int> ReadDepth(Task<int> reading)
    {
        int depth = await reading;
        Console.WriteLine("LIVE: " + new StackTrace());
        if (depth < 0)
        {
            throw new InvalidOperationException("Survey depth is invalid");
        }

        return depth + 3;
    }
}
// endsnippet:workflow

static class Program
{
    static async Task Main()
    {
        MethodInfo method = typeof(Survey).GetMethod(nameof(Survey.ReadDepth))!;
        bool marked = ((int)method.GetMethodImplementationFlags() & 0x2000) != 0;
        bool stateMachine = method.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;
        bool expected = Environment.GetCommandLineArgs().Contains("--runtime");
        if (marked != expected || stateMachine == expected)
        {
            throw new Exception($"Unexpected lowering: marker={marked}; stateMachine={stateMachine}");
        }

        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<int> result = Survey.ReadDepth(pending.Task);
        if (result.IsCompleted) throw new Exception("Expected actual suspension");
        pending.SetResult(14);
        if (await result != 17) throw new Exception("Depth changed");
        var failed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<int> failure = Survey.ReadDepth(failed.Task);
        failed.SetResult(-1);
        try
        {
            await failure;
            throw new Exception("Expected survey failure");
        }
        catch (InvalidOperationException error)
        {
            Console.WriteLine("EXCEPTION: " + error.StackTrace);
            if (!error.StackTrace!.Contains(nameof(Survey.ReadDepth), StringComparison.Ordinal)) throw new Exception("Origin frame lost");
        }

        Console.WriteLine($"PASS: marker={marked}; stateMachine={stateMachine}; pending result/failure; runtime={Environment.Version}");
    }
}
