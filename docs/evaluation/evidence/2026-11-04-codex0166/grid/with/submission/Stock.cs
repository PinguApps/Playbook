namespace GridLinksEvaluation;

public sealed record Stock(string Code, int Units, string Group)
{
    public static readonly Stock[] North = [new("N1", 30, "open water"), new("N2", 10, "open water"), new("N3", 40, "open water"), new("N4", 20, "open water"), new("NX", 99, "closed")];
    public static readonly Stock[] South = [new("S1", 20, "open water"), new("S2", 40, "open water"), new("S3", 10, "open water"), new("S4", 30, "open water"), new("SX", 99, "closed")];
}
