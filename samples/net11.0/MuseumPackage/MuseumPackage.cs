using System.IO.Packaging;

namespace Playbook.Samples;

public static class MuseumPackage
{
    #region older
    public static byte ReadIndexAssumingSeek(PackagePart part)
    {
        using Stream source = part.GetStream(FileMode.Open, FileAccess.Read);
        return ReadLastThenFirst(source);
    }
    #endregion

    #region adapt
    public static byte ReadIndex(PackagePart part, int maximumBytes)
    {
        using Stream source = part.GetStream(FileMode.Open, FileAccess.Read);
        if (source.CanSeek) return ReadLastThenFirst(source);
        using MemoryStream buffer = BufferForIndex(source, maximumBytes);
        return ReadLastThenFirst(buffer);
    }
    #endregion

    #region sequential
    public static int CountRecords(PackagePart part)
    {
        using Stream source = part.GetStream(FileMode.Open, FileAccess.Read);
        using StreamReader reader = new(source);
        int count = 0;
        while (reader.ReadLine() is not null) count++;
        return count;
    }
    #endregion

    #region bounded
    // The caller owns source; this method owns the returned, rewound buffer.
    public static MemoryStream BufferForIndex(Stream source, int maximumBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumBytes);
        // PackagePart streams expose Length even when Position/Seek are unavailable.
        // Check before allocating the destination or reading a potentially large part.
        if (source.Length > maximumBytes)
            throw new InvalidDataException("Museum part exceeds the import budget.");

        MemoryStream buffer = new();
        try
        {
            byte[] block = new byte[4096];
            while (true)
            {
                // Read at most one byte beyond the remaining budget to detect growth.
                int request = (int)Math.Min(block.Length, maximumBytes - buffer.Length + 1);
                int read = source.Read(block, 0, request);
                if (read == 0) break;
                if (read > maximumBytes - buffer.Length)
                    throw new InvalidDataException("Museum part grew beyond the import budget.");
                buffer.Write(block, 0, read);
            }
            buffer.Position = 0;
            return buffer;
        }
        catch { buffer.Dispose(); throw; }
    }

    public static byte ReadLastThenFirst(Stream seekable)
    {
        seekable.Seek(-1, SeekOrigin.End); // An index parser really needs random access.
        int last = seekable.ReadByte();
        seekable.Position = 0;
        if (seekable.ReadByte() != 'M' || last != '\n')
            throw new InvalidDataException("Unexpected museum index framing.");
        return (byte)last;
    }
    #endregion
}
