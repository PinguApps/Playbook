using System.Net;
using System.Text;

namespace Playbook.Samples;

internal static class Program
{
    public static void Main()
    {
        RuntimeIdentity.Assert();
        (string Input, string Expected)[] valid = [
            ("192.0.2.41:4701", "192.0.2.41:4701"), ("[2001:db8::41]:4701", "[2001:db8::41]:4701"),
            ("127.0.0.1:0", "127.0.0.1:0"), ("[::1]:65535", "[::1]:65535"),
            ("192.0.2.41", "192.0.2.41:0"), ("2001:db8::41", "[2001:db8::41]:0")];
#if NET11_0_OR_GREATER
        Span<char> chars = stackalloc char[80];
#endif
        foreach (var (input, expected) in valid)
        {
            byte[] source = Encoding.UTF8.GetBytes("#" + input + "!");
            byte[] original = source.ToArray();
            Require(EndpointText.TryRead(source.AsSpan(1, source.Length - 2), out var endpoint), "Valid parse failed.");
            Require(endpoint!.ToString() == expected && source.SequenceEqual(original), "Parse value/input boundary changed.");
            int length = Encoding.UTF8.GetByteCount(expected);
            byte[] output = Enumerable.Repeat((byte)0xCC, length + 9).ToArray();
            Require(EndpointText.TryWrite(endpoint, output.AsSpan(3, length), out int written), "Exact fit failed.");
            Require(written == length && Encoding.UTF8.GetString(output, 3, written) == expected, "Written length/text differs.");
            Require(output.Take(3).Concat(output.Skip(3 + length)).All(b => b == 0xCC), "Write escaped destination.");
            foreach (int size in new[] { 0, 1, length - 1 })
            {
                Array.Fill(output, (byte)0xCC);
                Require(!EndpointText.TryWrite(endpoint, output.AsSpan(3, size), out written) && written == 0, "Short destination accepted.");
                // Failed TryFormat may modify its destination. Only outside boundaries are protected.
                Require(output.Take(3).Concat(output.Skip(3 + size)).All(b => b == 0xCC), "Failed write escaped destination.");
            }
            Array.Fill(output, (byte)0xCC);
            Require(EndpointText.TryWrite(endpoint, output.AsSpan(3, length + 3), out written) && written == length, "Oversized destination failed.");
            Require(output.Skip(3 + written).All(b => b == 0xCC), "Successful write touched suffix.");
#if NET11_0_OR_GREATER
            Require(IPEndPoint.Parse(source.AsSpan(1, source.Length - 2)).Equals(endpoint), "Byte Parse disagrees.");
            Require(endpoint.TryFormat(chars, out int charsWritten) && new string(chars[..charsWritten]) == expected, "Char format disagrees.");
            Require(ParseGeneric<IPEndPoint>(source.AsSpan(1, source.Length - 2)).Equals(endpoint), "UTF8 parsing interface disagrees.");
            IUtf8SpanFormattable formattable = endpoint;
            Require(formattable.TryFormat(output, out written, default, null) && written == length, "UTF8 format interface disagrees.");
#endif
            Console.WriteLine($"ENDPOINT {input} => {expected}; exact/large/short0/1/{length - 1}; guards/input preserved");
        }
        foreach (string invalid in new[] { "", "relay.alder.test:4701", "192.0.2.41:-1", "192.0.2.41:65536", "192.0.2.41:", "[::1", "[::1]:x", "[::1]:+1", "[::1]: 1", "[::1]:4294967296" })
        {
            byte[] input = Encoding.UTF8.GetBytes(invalid);
            Require(!EndpointText.TryRead(input, out var endpoint) && endpoint is null, "Malformed input accepted: " + invalid);
#if NET11_0_OR_GREATER
            try { IPEndPoint.Parse(input); throw new InvalidOperationException("Parse accepted invalid input."); }
            catch (FormatException) { }
#endif
        }
        Require(!EndpointText.TryRead([0xFF, 0xFE], out _), "Invalid UTF8 accepted.");
        Console.WriteLine("PASS EndpointLedger: IPv4/IPv6, port0/65535, default port, malformed/range/UTF8 failures; no DNS/network I/O.");
    }

#if NET11_0_OR_GREATER
    private static T ParseGeneric<T>(ReadOnlySpan<byte> text) where T : IUtf8SpanParsable<T> => T.Parse(text, null);
#endif
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
