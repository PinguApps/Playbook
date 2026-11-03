using System.Numerics;
namespace Observatory;
// snippet:contract
public static class Results
{
    // Agreement for this instrument only: finite values, inclusive combined bound.
    public static bool Accept<T>(T actual, T expected, T absolute, T relative)
        where T : IFloatingPointIeee754<T>
    {
        if (!T.IsFinite(actual) || !T.IsFinite(expected) ||
            !T.IsFinite(absolute) || !T.IsFinite(relative) || absolute < T.Zero || relative < T.Zero)
            return false;
        return T.Abs(actual - expected) <= T.Max(absolute, relative * T.Max(T.Abs(actual), T.Abs(expected)));
    }

    public sealed record Saved(string InputId, string AlgorithmVersion, string Output);
    // Keep history. Commit a new version only after explicit validation.
    public static Saved Propose(Saved historical, string version, string output) =>
        historical with { AlgorithmVersion = version, Output = output };
}
// endsnippet:contract
