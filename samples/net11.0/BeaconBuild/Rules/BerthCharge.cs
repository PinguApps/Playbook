// snippet:rules
namespace Beacon.Rules;

public static class BerthCharge
{
    public static int Calculate(int crates, int creditsPerCrate) => checked(crates * creditsPerCrate);
}
// endsnippet:rules
