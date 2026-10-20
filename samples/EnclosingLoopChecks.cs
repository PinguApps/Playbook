using Playbook.Samples;

Check(
    [[21, 0, 22], [31], [-1, 41], [51]],
    ["accept:21", "dispose:reading:21", "dispose:reading:0", "finally:0", "dispose:batch:0",
     "accept:31", "dispose:reading:31", "complete:1", "finally:1", "dispose:batch:1",
     "dispose:reading:-1", "finally:2", "dispose:batch:2", "finished"],
    "continue skips the batch tail; break stops all batches; cleanup unwinds inside out");
Check(
    [[], [8, 9]],
    ["complete:0", "finally:0", "dispose:batch:0", "accept:8", "dispose:reading:8",
     "accept:9", "dispose:reading:9", "complete:1", "finally:1", "dispose:batch:1", "finished"],
    "empty and fully visited batches complete normally");
Check([], ["finished"], "no batches");
Check(
    [[0], [7]],
    ["dispose:reading:0", "finally:0", "dispose:batch:0", "accept:7", "dispose:reading:7",
     "complete:1", "finally:1", "dispose:batch:1", "finished"],
    "continue advances the for iterator");
Check(
    [[-3], [7]],
    ["dispose:reading:-3", "finally:0", "dispose:batch:0", "finished"],
    "break still reaches work after the loop");
Console.WriteLine($"PASS: nested traversal, break/continue targets and finally/using cleanup; runtime {Environment.Version}");

static void Check(int[][] batches, string[] expected, string claim)
{
    List<string> actual = TransmissionReview.Review(batches);
    if (!actual.SequenceEqual(expected)) { throw new InvalidOperationException($"{claim}: {string.Join(", ", actual)}"); }
    Console.WriteLine($"{claim}: {string.Join(", ", actual)}");
}
