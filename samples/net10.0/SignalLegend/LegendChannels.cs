// snippet:channels
namespace SignalFormats;

internal readonly record struct Rgba<T>(T R, T G, T B, T A) where T : struct
{
    public Rgba(ReadOnlySpan<T> source) : this(Channel(source, 0), Channel(source, 1), Channel(source, 2), Channel(source, 3)) { }
    private static T Channel(ReadOnlySpan<T> source, int index)
    {
        if (source.Length < 4) throw new ArgumentException("Four components required.", nameof(source));
        return source[index];
    }
    public Argb<T> ToArgb() => new(A, R, G, B);
    public void CopyTo(Span<T> destination)
    {
        if (destination.Length < 4) throw new ArgumentException("Four components required.", nameof(destination));
        destination[0] = R; destination[1] = G; destination[2] = B; destination[3] = A;
    }
}
internal readonly record struct Argb<T>(T A, T R, T G, T B) where T : struct
{
    public Argb(ReadOnlySpan<T> source) : this(Channel(source, 0), Channel(source, 1), Channel(source, 2), Channel(source, 3)) { }
    private static T Channel(ReadOnlySpan<T> source, int index)
    {
        if (source.Length < 4) throw new ArgumentException("Four components required.", nameof(source));
        return source[index];
    }
    public Rgba<T> ToRgba() => new(R, G, B, A);
    public void CopyTo(Span<T> destination)
    {
        if (destination.Length < 4) throw new ArgumentException("Four components required.", nameof(destination));
        destination[0] = A; destination[1] = R; destination[2] = G; destination[3] = B;
    }
}
internal static class Rgba
{
    public static Rgba<byte> CreateBigEndian(uint word) => new((byte)(word >> 24), (byte)(word >> 16), (byte)(word >> 8), (byte)word);
    public static Rgba<byte> CreateLittleEndian(uint word) => new((byte)word, (byte)(word >> 8), (byte)(word >> 16), (byte)(word >> 24));
    public static uint ToUInt32BigEndian(this Rgba<byte> c) => ((uint)c.R << 24) | ((uint)c.G << 16) | ((uint)c.B << 8) | c.A;
    public static uint ToUInt32LittleEndian(this Rgba<byte> c) => ((uint)c.A << 24) | ((uint)c.B << 16) | ((uint)c.G << 8) | c.R;
}
internal static class Argb
{
    public static Argb<byte> CreateBigEndian(uint word) => new((byte)(word >> 24), (byte)(word >> 16), (byte)(word >> 8), (byte)word);
    public static Argb<byte> CreateLittleEndian(uint word) => new((byte)word, (byte)(word >> 8), (byte)(word >> 16), (byte)(word >> 24));
    public static uint ToUInt32BigEndian(this Argb<byte> c) => ((uint)c.A << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B;
    public static uint ToUInt32LittleEndian(this Argb<byte> c) => ((uint)c.B << 24) | ((uint)c.G << 16) | ((uint)c.R << 8) | c.A;
}
// endsnippet:channels
