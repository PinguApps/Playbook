using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Playbook.Samples;

internal static class CalibrationChecks
{
    private static void Main()
    {
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; vector128Float={Vector128<float>.IsSupported}; vector512Hardware={Vector512.IsHardwareAccelerated}");
        Console.WriteLine($"tensor={typeof(TensorPrimitives).Assembly.FullName}; location={typeof(TensorPrimitives).Assembly.Location}");
        Require(!typeof(TensorPrimitives).GetCustomAttributesData().Concat(typeof(TensorPrimitives).GetMethods().Where(method => method.Name == "Clamp").SelectMany(method => method.GetCustomAttributesData())).Any(attribute => attribute.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.ExperimentalAttribute"), "Selected Clamp API unexpectedly experimental.");
        Console.WriteLine("METADATA: TensorPrimitives/Clamp have no Experimental attribute.");
        Check<float>();
        Check<decimal>();
        Console.WriteLine("PASS: calibration bounds validated before writes; float/decimal scalar and pairwise controls.");
    }

    private static void Check<T>() where T : INumber<T>
    {
        foreach (int length in new[] { 1, 3, 33 })
        {
            T[] values = Enumerable.Range(0, length).Select(i => T.CreateChecked(i % 7 - 2)).ToArray();
            foreach (string bounds in new[] { "ordered", "equal", "reversed" })
            {
                T lo = T.CreateChecked(bounds == "reversed" ? 4 : 1);
                T hi = T.CreateChecked(bounds == "equal" ? 1 : 3);
                T[] lower = Enumerable.Repeat(lo, length).ToArray();
                T[] upper = Enumerable.Repeat(hi, length).ToArray();
                foreach (bool pairwise in new[] { false, true })
                {
                    T[] raw = Enumerable.Repeat(T.CreateChecked(91), length + 1).ToArray();
                    Exception? failure = null;
                    try
                    {
                        if (pairwise) TensorPrimitives.Clamp<T>(values, lower, upper, raw);
                        else TensorPrimitives.Clamp<T>(values, lo, hi, raw);
                    }
                    catch (ArgumentException ex) { failure = ex; }
                    Console.WriteLine($"RAW type={typeof(T).Name}; length={length}; pairwise={pairwise}; bounds={bounds}; input={Join(values)}; lower={lo}; upper={hi}; exception={failure?.GetType().Name ?? "none"}; output={Join(raw)}");
                    T[] output = Enumerable.Repeat(T.CreateChecked(91), length + 1).ToArray();
                    if (bounds == "reversed")
                    {
                        MustReject(() => Apply(values, lower, upper, output, pairwise), output);
                    }
                    else
                    {
                        Apply(values, lower, upper, output, pairwise);
                        T[] expected = values.Select(x => T.Clamp(x, lo, hi)).Append(T.CreateChecked(91)).ToArray();
                        Require(output.SequenceEqual(expected), "Incorrect corrected values/tail.");
                        Require(failure is null && raw.SequenceEqual(expected), "Valid raw clamp failed.");
                    }
                }
            }

            // An invalid late pair must be discovered before the first output write.
            T[] lows = Enumerable.Repeat(T.One, length).ToArray();
            T[] highs = Enumerable.Repeat(T.CreateChecked(3), length).ToArray();
            lows[^1] = T.CreateChecked(4);
            T[] destination = Enumerable.Repeat(T.CreateChecked(91), length + 1).ToArray();
            T[] rawLate = destination.ToArray();
            string rawFailure = "none";
            try { TensorPrimitives.Clamp<T>(values, lows, highs, rawLate); }
            catch (ArgumentException ex) { rawFailure = ex.GetType().Name; }
            Console.WriteLine($"RAW LATE type={typeof(T).Name}; length={length}; lower={Join(lows)}; upper={Join(highs)}; exception={rawFailure}; output={Join(rawLate)}");
            MustReject(() => CalibrationPolicy.Clamp<T>(values, lows, highs, destination), destination);
            highs[^1] = T.CreateChecked(5);
            CalibrationPolicy.Clamp<T>(values, lows, highs, destination);
            Require(values.Select((value, i) => T.Clamp(value, lows[i], highs[i])).Append(T.CreateChecked(91)).SequenceEqual(destination), "Varying bound values failed.");
            Array.Fill(destination, T.CreateChecked(91));
            MustReject(() => CalibrationPolicy.Clamp<T>(values, lows.AsSpan(1), highs, destination), destination);
            T[] shortOutput = Enumerable.Repeat(T.CreateChecked(91), length - 1).ToArray();
            MustReject(() => CalibrationPolicy.Clamp<T>(values, T.Zero, T.One, shortOutput), shortOutput);
        }
        if (typeof(T) == typeof(float))
        {
            float[] output = [91];
            MustReject(() => CalibrationPolicy.Clamp<float>([2], float.NaN, 3, output), output);
        }
    }

    private static void Apply<T>(T[] values, T[] lower, T[] upper, T[] output, bool pairwise) where T : INumber<T>
    {
        if (pairwise) CalibrationPolicy.Clamp<T>(values, lower, upper, output);
        else CalibrationPolicy.Clamp<T>(values, lower[0], upper[0], output);
    }

    private static void MustReject<T>(Action action, T[] output) where T : INumber<T>
    {
        T[] before = output.ToArray();
        try { action(); }
        catch (ArgumentException) { Require(output.SequenceEqual(before), "Rejected bounds modified output."); return; }
        throw new InvalidOperationException("Invalid bounds were accepted.");
    }

    private static string Join<T>(IEnumerable<T> values) => string.Join(',', values.Select(x => Convert.ToString(x, CultureInfo.InvariantCulture)));
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
