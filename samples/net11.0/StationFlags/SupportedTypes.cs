namespace Playbook.Samples;

internal static class SupportedTypes
{
    public static void Run()
    {
        Test<byte>(64, 1, 65); Test<sbyte>(64, 1, 65); Test<short>(64, 1, 65); Test<ushort>(64, 1, 65);
        Test<int>(64, 1, 65); Test<uint>(64, 1, 65); Test<long>(64, 1, 65); Test<ulong>(64, 1, 65);
        Test<nint>(64, 1, 65); Test<nuint>(64, 1, 65);
        Test((E8)64, (E8)1, (E8)65); Test((ES8)64, (ES8)1, (ES8)65);
        Test((E16)64, (E16)1, (E16)65); Test((ES16)64, (ES16)1, (ES16)65);
        Test((E32)64, (E32)1, (E32)65); Test((ES32)64, (ES32)1, (ES32)65);
        Test((E64)64, (E64)1, (E64)65); Test((ES64)64, (ES64)1, (ES64)65);
        Test<int>(int.MinValue, 1, int.MinValue | 1);
        Test<ulong>(1UL << 63, 1, (1UL << 63) | 1);
        Test((ES8)sbyte.MinValue, (ES8)1, (ES8)(sbyte.MinValue | 1));
        Test((E64)(1UL << 63), (E64)1, (E64)((1UL << 63) | 1));
        Reject(new Parcel(1)); Reject(1.0f); Reject(1.0); Reject(1m); Reject(Guid.Empty);
        Console.WriteLine("CHECK types: 10 integer/native-integer types and all 8 C# enum backing types; custom struct/float/double/decimal/Guid rejected by both operations");
    }

    private static void Test<T>(T reserved, T bit, T combined) where T : struct
    {
        T value = reserved;
        FlagsChecks.Check(Interlocked.Or(ref value, bit).Equals(reserved), $"Or<{typeof(T)}> prior");
        FlagsChecks.Check(value.Equals(combined), $"Or<{typeof(T)}> result");
        FlagsChecks.Check(Interlocked.And(ref value, reserved).Equals(combined), $"And<{typeof(T)}> prior");
        FlagsChecks.Check(value.Equals(reserved), $"And<{typeof(T)}> result");
    }

    private static void Reject<T>(T initial) where T : struct
    {
        T value = initial;
        bool orRejected = false, andRejected = false;
        try { Interlocked.Or(ref value, initial); } catch (NotSupportedException) { orRejected = true; }
        try { Interlocked.And(ref value, initial); } catch (NotSupportedException) { andRejected = true; }
        FlagsChecks.Check(orRejected && andRejected && value.Equals(initial), $"unsupported {typeof(T)}");
    }

    private readonly record struct Parcel(int Value);
    private enum E8 : byte { None = 0 } private enum ES8 : sbyte { None = 0 }
    private enum E16 : ushort { None = 0 } private enum ES16 : short { None = 0 }
    private enum E32 : uint { None = 0 } private enum ES32 : int { None = 0 }
    private enum E64 : ulong { None = 0 } private enum ES64 : long { None = 0 }
}
