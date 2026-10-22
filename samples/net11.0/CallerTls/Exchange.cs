// snippet:buffer-tls
using System.Net.Security;
using System.Security.Authentication;

#pragma warning disable SYSLIB5007 // Deliberately review and opt in to the experimental state machine.
namespace Samples;

public static class DispatchHandshake
{
    public static void Authenticate(TlsBufferSession client, TlsBufferSession server)
    {
        List<byte> toClient = [];
        List<byte> toServer = [];
        for (int turn = 0; turn < 20000; turn++)
        {
            Step(client, toClient, toServer);
            Step(server, toServer, toClient);
            if (client.IsHandshakeComplete && server.IsHandshakeComplete && toClient.Count == 0 && toServer.Count == 0)
            {
                return;
            }
        }

        throw new AuthenticationException("Handshake exceeded the bounded local pump.");
    }

    private static void Step(TlsBufferSession session, List<byte> incoming, List<byte> outgoing)
    {
        // Deliberately deliver short fragments; retain every unconsumed byte.
        byte[] input = incoming.ToArray();
        byte[] output = new byte[113];
        int consumed;
        int written;
        bool reading = session.IsHandshakeComplete && !session.HasPendingOutput;
        TlsOperationStatus status = reading
            ? session.Read(input, output, out consumed, out written)
            : session.Handshake(input, output, out consumed, out written);
        if (reading && written != 0) { throw new IOException("Unexpected application data during handshake."); }
        incoming.RemoveRange(0, consumed);
        if (!reading) { outgoing.AddRange(output.AsSpan(0, written).ToArray()); }

        switch (status)
        {
            case TlsOperationStatus.Complete:
            case TlsOperationStatus.NeedMoreData:
            case TlsOperationStatus.DestinationTooSmall:
                break;
            case TlsOperationStatus.NeedsCertificateValidation:
                if (session.AcceptWithDefaultValidation() != SslPolicyErrors.None)
                {
                    throw new AuthenticationException("Peer certificate rejected.");
                }
                break;
            default:
                // This fixed server context does not support deferred SNI selection or client certificates.
                throw new AuthenticationException($"Unexpected TLS state: {status}");
        }
    }

    public static byte[] Transfer(TlsBufferSession sender, TlsBufferSession receiver, byte[] plaintext)
    {
        List<byte> ciphertext = [];
        byte[] output = new byte[127];
        int offset = 0;
        while (offset < plaintext.Length || sender.HasPendingOutput)
        {
            int consumed = 0;
            int written;
            TlsOperationStatus status = sender.HasPendingOutput
                ? sender.DrainPendingOutput(output, out written)
                : sender.Write(plaintext.AsSpan(offset), output, out consumed, out written);
            offset += consumed;
            ciphertext.AddRange(output.AsSpan(0, written).ToArray());
            if (status is not (TlsOperationStatus.Complete or TlsOperationStatus.DestinationTooSmall) || (consumed == 0 && written == 0))
            {
                throw new IOException($"TLS write made no progress: {status}");
            }
        }

        List<byte> recovered = [];
        // RC2 requires enough plaintext space for a full record; undersizing throws.
        output = new byte[16384];
        byte[] wire = ciphertext.ToArray();
        offset = 0;
        // Read can retain decrypted data; continue with empty input until NeedMoreData.
        for (int turn = 0; turn < wire.Length + plaintext.Length + 100; turn++)
        {
            TlsOperationStatus status = receiver.Read(wire.AsSpan(offset), output, out int consumed, out int written);
            offset += consumed;
            recovered.AddRange(output.AsSpan(0, written).ToArray());
            if (status == TlsOperationStatus.NeedMoreData && offset == wire.Length)
            {
                return recovered.ToArray();
            }
            if (status is not (TlsOperationStatus.Complete or TlsOperationStatus.NeedMoreData or TlsOperationStatus.DestinationTooSmall))
            {
                throw new IOException($"Unexpected TLS read state: {status}");
            }
        }

        throw new IOException("TLS read exceeded its bound.");
    }
}
#pragma warning restore SYSLIB5007
// endsnippet:buffer-tls
