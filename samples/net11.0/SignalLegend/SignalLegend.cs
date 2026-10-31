// snippet:legend
using System.Numerics.Colors;

internal static class SignalLegend
{
    public static (uint RgbaBig, uint RgbaLittle, uint ArgbBig, uint ArgbLittle) Encode(
        ReadOnlySpan<byte> rgbaComponents, Span<byte> argbComponents)
    {
        var rgba = new Rgba<byte>(rgbaComponents);
        Argb<byte> argb = rgba.ToArgb();
        argb.CopyTo(argbComponents);
        return (rgba.ToUInt32BigEndian(), rgba.ToUInt32LittleEndian(),
            argb.ToUInt32BigEndian(), argb.ToUInt32LittleEndian());
    }
}
// endsnippet:legend
