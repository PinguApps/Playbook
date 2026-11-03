namespace CwmSeedBank;

// snippet:rules
public static class Germination
{
    public static bool Ready(int viableSeeds, int totalSeeds) =>
        totalSeeds > 0 && viableSeeds * 100L >= totalSeeds * 80L;
}

public static class ColdStore
{
    public static bool InRange(int temperatureCelsius) =>
        temperatureCelsius is >= -20 and <= -16;
}
// endsnippet:rules
