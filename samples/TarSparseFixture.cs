using System.Formats.Tar;
using System.Text;

namespace Playbook.Samples;

// Original GNU sparse 1.0 PAX fixture: logical 16 bytes, two stored extents.
public static class TarSparseFixture
{
    public static byte[] Create()
    {
        byte[] stored = new byte[517];
        Encoding.ASCII.GetBytes("2\n0\n3\n12\n2\n").CopyTo(stored, 0);
        "ABCDE"u8.CopyTo(stored.AsSpan(512));
        KeyValuePair<string, string>[] attributes =
        [
            new("GNU.sparse.major", "1"),
            new("GNU.sparse.minor", "0"),
            new("GNU.sparse.realsize", "16"),
            new("GNU.sparse.name", "depth.bin"),
        ];
        using MemoryStream data = new(stored);
        PaxTarEntry entry = new(TarEntryType.RegularFile, "GNUSparseFile.0/depth.bin", attributes) { DataStream = data };
        using MemoryStream output = new();
        using (TarWriter writer = new(output, TarEntryFormat.Pax, leaveOpen: true)) writer.WriteEntry(entry);
        return output.ToArray();
    }
}
