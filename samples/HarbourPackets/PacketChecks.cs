using System.Security.Cryptography;
using System.Text;

namespace HarbourPackets;

public static class PacketChecks
{
    // Public synthetic fixture keys, never production secrets.
    private static readonly byte[] Key = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
    private static readonly byte[] Data = Encoding.UTF8.GetBytes("Mallow Quay|buoy=7|offset=-9|sequence=42");
    private static readonly byte[] Custom = Encoding.UTF8.GetBytes("Mallow packet v1");

    public static async Task RunAsync()
    {
        Console.WriteLine($"FIXTURE key={Convert.ToHexString(Key)} data={Convert.ToHexString(Data)} customization={Convert.ToHexString(Custom)}");
        byte[] tag = HMACSHA256.HashData(Key, Data);
        // Independently generated with Python's hmac/hashlib, not this verification method.
        Check(Convert.ToHexString(tag) == "02907D3EDAC15E52E6C0051B65B7E0902866C47B2A9791EBEEE9BBF8C2627FC9", "HMAC known answer");
        byte[] bad = (byte[])tag.Clone(); bad[3] ^= 1;
        Check(PacketAuthenticator.Verify(Key, Data, tag), "HMAC valid");
        Check(!PacketAuthenticator.Verify(Key, Data, bad), "HMAC bad");
        Check(!PacketAuthenticator.Verify(Key, Data, tag[..31]), "HMAC truncated");
        Check(!PacketAuthenticator.Verify(Key, Data, []), "HMAC empty");
        Check(!PacketAuthenticator.Verify([99, 98, 97], Data, tag), "HMAC wrong key");
        Check(!PacketAuthenticator.Verify(Key, [1, 2, 3], tag), "HMAC wrong data");
        using (var stream = new MemoryStream(Data))
            Check(await PacketAuthenticator.VerifyAsync(Key, stream, tag, CancellationToken.None) && stream.Position == stream.Length, "HMAC real stream complete");
        using (var hash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, Key))
        {
            hash.AppendData(Data.AsSpan(0, 12));
            Check(PacketAuthenticator.Current(hash, HMACSHA256.HashData(Key, Data[..12])), "HMAC prefix preserves state");
            hash.AppendData(Data.AsSpan(12));
            Check(!PacketAuthenticator.Current(hash, bad), "HMAC mismatch preserves state");
            Check(PacketAuthenticator.Current(hash, tag), "HMAC complete preserves state");
            Check(PacketAuthenticator.Finish(hash, tag), "HMAC finish succeeds");
            Check(PacketAuthenticator.Current(hash, HMACSHA256.HashData(Key, Array.Empty<byte>())), "HMAC success resets");
            hash.AppendData(Data);
            Check(!PacketAuthenticator.Finish(hash, bad), "HMAC finish mismatch");
            Check(PacketAuthenticator.Current(hash, HMACSHA256.HashData(Key, Array.Empty<byte>())), "HMAC mismatch resets");
        }
        using (var stream = new PausingStream(Data))
        using (var cancel = new CancellationTokenSource())
        {
            var verification = PacketAuthenticator.VerifyAsync(Key, stream, tag, cancel.Token).AsTask();
            await stream.Waiting.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await cancel.CancelAsync();
            try { await verification; throw new InvalidOperationException("Ignored cancellation."); }
            catch (OperationCanceledException) { Check(stream.Reads == 2 && stream.BytesRead == 12, "HMAC cancellation after actual prefix read"); }
        }
        Console.WriteLine($"PROVIDER Kmac128.IsSupported={Kmac128.IsSupported}");
        if (!Kmac128.IsSupported)
        {
            if (Environment.GetEnvironmentVariable("REQUIRE_KMAC") == "1") throw new PlatformNotSupportedException("Positive KMAC proof required.");
            Console.WriteLine("KMAC not executed on this host; positive provider evidence is separate.");
            return;
        }
        if (OperatingSystem.IsWindows())
        {
            foreach (System.Diagnostics.ProcessModule module in System.Diagnostics.Process.GetCurrentProcess().Modules)
                if (module.ModuleName.Equals("bcrypt.dll", StringComparison.OrdinalIgnoreCase) || module.ModuleName.Equals("bcryptprimitives.dll", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine($"NATIVE {module.FileName} version={module.FileVersionInfo.FileVersion} sha256={Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(module.FileName)))}");
        }
        byte[] kmac = Kmac128.HashData(Key, Data, PacketAuthenticator.TagLength, Custom);
        Check(Convert.ToHexString(kmac) == "B13A6D6DD298CBA1BF0832444C29B6B9568B300C6F7A001BCF65696A1BC2A2C3", "KMAC known answer (OpenSSL CLI)");
        byte[] wrong = (byte[])kmac.Clone(); wrong[0] ^= 1;
        Check(PacketAuthenticator.VerifyKmac(Key, Data, kmac, Custom), "KMAC valid customized");
        Check(!PacketAuthenticator.VerifyKmac(Key, Data, wrong, Custom), "KMAC bad");
        Check(!PacketAuthenticator.VerifyKmac(Key, Data, kmac[..31], Custom), "KMAC truncated protocol tag");
        Check(!PacketAuthenticator.VerifyKmac(Key, Data, [], Custom), "KMAC empty protocol tag");
        Check(!PacketAuthenticator.VerifyKmac(Key, Data, kmac, []), "KMAC wrong customization");
        Check(!PacketAuthenticator.VerifyKmac(Key, Data, kmac, Encoding.UTF8.GetBytes("Mallow packet v2")), "KMAC changed customization");
        Check(!PacketAuthenticator.VerifyKmac(new byte[32], Data, kmac, Custom), "KMAC wrong key");
        Check(!PacketAuthenticator.VerifyKmac(Key, [1, 2, 3], kmac, Custom), "KMAC wrong data");
        using (var hash = new Kmac128(Key, Custom))
        {
            hash.AppendData(Data.AsSpan(0, 12));
            Check(PacketAuthenticator.CurrentKmac(hash, Kmac128.HashData(Key, Data[..12], 32, Custom)), "KMAC prefix preserves");
            hash.AppendData(Data.AsSpan(12));
            Check(!PacketAuthenticator.CurrentKmac(hash, wrong), "KMAC mismatch preserves");
            Check(PacketAuthenticator.FinishKmac(hash, kmac), "KMAC finish");
            Check(PacketAuthenticator.CurrentKmac(hash, Kmac128.HashData(Key, Array.Empty<byte>(), 32, Custom)), "KMAC success resets");
            hash.AppendData(Data);
            Check(!PacketAuthenticator.FinishKmac(hash, wrong), "KMAC finish mismatch");
            Check(PacketAuthenticator.CurrentKmac(hash, Kmac128.HashData(Key, Array.Empty<byte>(), 32, Custom)), "KMAC mismatch resets");
        }
        Console.WriteLine("PASS: complete HMAC/KMAC verification contract; no timing measurement.");
    }

    private static void Check(bool result, string name)
    {
        if (!result) throw new InvalidOperationException(name);
        Console.WriteLine($"PASS {name}");
    }

    private sealed class PausingStream(byte[] data) : Stream
    {
        public TaskCompletionSource Waiting { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Reads { get; private set; }
        public int BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Reads++;
            if (Reads == 1) { data.AsMemory(0, 12).CopyTo(buffer); BytesRead = 12; return 12; }
            Waiting.SetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Read should have cancelled.");
        }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
