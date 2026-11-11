namespace Playbook.Samples;

// snippet:generic-hierarchy
public closed class ShelfValue<T>;
public sealed class SingleShelf<T> : ShelfValue<T>;
public sealed class ShelfArray<T> : ShelfValue<T[]>;

public static class ShelfMatching
{
    public static int Text(ShelfValue<string> value) => value switch
    {
        SingleShelf<string> => 1
    };

    public static int Unknown<T>(ShelfValue<T> value) => value switch
    {
        SingleShelf<T> => 1,
        ShelfValue<T> => 2
    };
}
// endsnippet:generic-hierarchy

// snippet:hidden-hierarchy
public closed class IntakeDecision;
public sealed class AcceptedIntake : IntakeDecision;
internal sealed class DeferredIntake : IntakeDecision;
public class OpenIntake : IntakeDecision;
// endsnippet:hidden-hierarchy
