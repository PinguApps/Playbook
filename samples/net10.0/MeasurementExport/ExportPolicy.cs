namespace Playbook.Samples;

// snippet:policy
public static class ExportPolicy
{
    // On .NET 10 this selects that runtime's scale-based binary rounding.
    public static double Binary(double measurement, int digits) =>
        Math.Round(measurement, digits, MidpointRounding.ToEven);

    // Supply a decimal source quantity; a binary-to-decimal cast cannot recover intent.
    public static decimal DecimalQuantity(decimal measurement, int digits) =>
        decimal.Round(measurement, digits, MidpointRounding.ToEven);

    // An explicit bounded policy, not a universal historical-output emulator.
    public static double ScaleRoundDivide(double measurement, int digits)
    {
        if (digits is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(digits));
        if (!double.IsFinite(measurement)) throw new ArgumentOutOfRangeException(nameof(measurement));
        double scale = Math.Pow(10, digits);
        double scaled = measurement * scale;
        if (!double.IsFinite(scaled)) throw new OverflowException("Scaled measurement is not finite.");
        return Math.Round(scaled, MidpointRounding.ToEven) / scale;
    }
}
// endsnippet:policy
