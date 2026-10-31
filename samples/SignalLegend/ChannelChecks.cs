#if NET11_0_OR_GREATER
using System.Numerics.Colors;
#else
using SignalFormats;
#endif

byte[] channels = [0x12, 0x34, 0x56, 0x78];
var rgba = new Rgba<byte>(channels);
var argb = rgba.ToArgb();
Equal((byte)0x12, rgba.R); Equal((byte)0x34, rgba.G);
Equal((byte)0x56, rgba.B); Equal((byte)0x78, rgba.A);
Equal((byte)0x78, argb.A); Equal((byte)0x12, argb.R);
Equal((byte)0x34, argb.G); Equal((byte)0x56, argb.B);
Equal(rgba, argb.ToRgba());
Equal(0x12345678u, rgba.ToUInt32BigEndian());
Equal(0x78563412u, rgba.ToUInt32LittleEndian());
Equal(0x78123456u, argb.ToUInt32BigEndian());
Equal(0x56341278u, argb.ToUInt32LittleEndian());
foreach (uint word in new uint[] { 0, 0x12345678, 0xFE0180AB, uint.MaxValue })
{
    Equal(word, Rgba.CreateBigEndian(word).ToUInt32BigEndian());
    Equal(word, Rgba.CreateLittleEndian(word).ToUInt32LittleEndian());
    Equal(word, Argb.CreateBigEndian(word).ToUInt32BigEndian());
    Equal(word, Argb.CreateLittleEndian(word).ToUInt32LittleEndian());
}
Equal(rgba, Rgba.CreateBigEndian(0x12345678));
Equal(rgba, Rgba.CreateLittleEndian(0x78563412));
Equal(argb, Argb.CreateBigEndian(0x78123456));
Equal(argb, Argb.CreateLittleEndian(0x56341278));
Check<byte>([0x12, 0x34, 0x56, 0x78], 0xEE);
// Components are stored verbatim: these are not normalized/clamped to [0, 1].
Check<float>([-2.5f, 1.25f, 300f, 0.5f], -99f);
byte[] destination = new byte[5]; destination[4] = 0xEE;
Equal((0x12345678u, 0x78563412u, 0x78123456u, 0x56341278u), SignalLegend.Encode(channels, destination));
if (!destination.AsSpan().SequenceEqual(new byte[] { 0x78, 0x12, 0x34, 0x56, 0xEE })) throw new Exception("Wire channel order");
Console.WriteLine($"PASS SignalLegend: RGBA=12345678/78563412; ARGB=78123456/56341278; byte/float, four packed round trips, span lengths 0..5, equality/hash. Runtime={Environment.Version}; CoreLib={typeof(object).Assembly.Location}");

static void Check<T>(T[] values, T sentinel) where T : struct
{
    var rgba = new Rgba<T>(values); var argb = rgba.ToArgb();
    Equal(rgba, argb.ToRgba());
    var same = new Rgba<T>(values[0], values[1], values[2], values[3]);
    Equal(true, rgba.Equals(same)); Equal(true, rgba.Equals((object)same));
    Equal(false, rgba.Equals((object)argb)); Equal(false, rgba.Equals(null));
    Equal(rgba.GetHashCode(), same.GetHashCode());
    var sameArgb = new Argb<T>(values[3], values[0], values[1], values[2]);
    Equal(true, argb.Equals(sameArgb)); Equal(true, argb.Equals((object)sameArgb));
    Equal(false, argb.Equals((object)rgba)); Equal(false, argb.Equals(null));
    Equal(argb.GetHashCode(), sameArgb.GetHashCode());
    var changed = new Rgba<T>(values[1], values[0], values[2], values[3]);
    Equal(false, rgba.Equals(changed)); Equal(false, argb.Equals(changed.ToArgb()));
    // Equal values require equal hashes; unequal values need not have distinct hashes.
    Equal(1, new HashSet<Rgba<T>> { rgba, same }.Count);
    Equal(1, new HashSet<Argb<T>> { argb, sameArgb }.Count);
    T[] rgbaLong = [..values, sentinel];
    T[] argbLong = [values[3], values[0], values[1], values[2], sentinel];
    Equal(rgba, new Rgba<T>(rgbaLong)); Equal(argb, new Argb<T>(argbLong));
    foreach (int length in new[] { 0, 1, 2, 3 })
    {
        Throws<ArgumentException>(() => new Rgba<T>(new T[length]));
        Throws<ArgumentException>(() => new Argb<T>(new T[length]));
        T[] tooShort = Enumerable.Repeat(sentinel, length).ToArray();
        Throws<ArgumentException>(() => rgba.CopyTo(tooShort));
        Throws<ArgumentException>(() => argb.CopyTo(tooShort));
        if (tooShort.Any(x => !EqualityComparer<T>.Default.Equals(x, sentinel))) throw new Exception("Short copy mutated destination");
    }
    foreach (int length in new[] { 4, 5 })
    {
        T[] copy = Enumerable.Repeat(sentinel, length).ToArray();
        rgba.CopyTo(copy); Equal(rgba, new Rgba<T>(copy));
        if (length == 5) Equal(sentinel, copy[4]);
        argb.CopyTo(copy); Equal(argb, new Argb<T>(copy));
        if (length == 5) Equal(sentinel, copy[4]);
    }
}
static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
