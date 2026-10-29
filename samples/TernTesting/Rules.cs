// snippet:rules
namespace Tern.Rules;
public static class Capacity
{
    public static int Remaining(int total, int reserved) => checked(total - reserved);
}
// endsnippet:rules
