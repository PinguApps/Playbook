namespace Playbook.Samples;

public static class JumpMigration
{
    // snippet:goto-before
    public static string WithGoto()
    {
        string trace = "";
        for (int batch = 0; batch < 3; batch++)
        {
            for (int slot = 0; slot < 3; slot++)
            {
                if (batch == 0 && slot == 1) { goto nextBatch; }
                if (batch == 2) { goto finished; }
                trace += $"{batch}:{slot};";
            }
            trace += "complete;";
        nextBatch: ;
        }
    finished:
        return trace + "finished";
    }
    // endsnippet:goto-before

    // snippet:goto-after
    public static string WithLabels()
    {
        string trace = "";
    batches: for (int batch = 0; batch < 3; batch++)
        {
            for (int slot = 0; slot < 3; slot++)
            {
                if (batch == 0 && slot == 1) { continue batches; }
                if (batch == 2) { break batches; }
                trace += $"{batch}:{slot};";
            }
            trace += "complete;";
        }
        return trace + "finished";
    }
    // endsnippet:goto-after

    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Check()
    {
        const string expected = "0:0;1:0;1:1;1:2;complete;finished";
        if (WithGoto() != expected || WithLabels() != expected)
        {
            throw new InvalidOperationException("Jump migration changed continuation or post-loop work.");
        }
        Console.WriteLine("PASS: goto migration preserves continuation, completion and post-loop work");
    }
}
