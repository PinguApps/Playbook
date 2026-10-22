# Drive TLS with caller-owned buffers and I/O (experimental)

## Goal

Integrate encrypted dispatch messages into a scheduler which owns the incoming and outgoing byte buffers. TLS processes records; the application chooses how and when to transfer them.

## Older approach

.NET 10 has SslStream, not the public TlsBufferSession state machine. Supply a Stream (including a custom transport adapter) and let SslStream perform the handshake and asynchronous reads/writes. That remains a sound choice when Stream-based scheduling fits. This original client sends one parcel and reads its echo over a real loopback TCP stream in the verification project; leaveInnerStreamOpen preserves caller ownership of that transport.

{{snippet:older}}

## Newer approach

**Experimental SYSLIB5007.** .NET 11 exposes TlsContext, TlsSession, TlsBufferSession, TlsSocketSession and TlsOperationStatus. This recipe deliberately uses TlsBufferSession: it performs no I/O and does not own a socket. TlsSocketSession instead binds a socket and takes ownership of its handle; do not substitute it when the scheduler must own transfers.

The complete bounded local pump below transfers actual ciphertext between two sessions in memory. Each handshake call offers at most 113 output bytes, so the peer accumulates partial records. It retains unconsumed input, including when NeedMoreData consumes zero. In a network integration, replace queue transfer with your transport's send/read readiness operations and add cancellation, deadlines, EOF and memory limits. This pump is an executable comparison, not a production network transport.

{{snippet:newer}}

## Migration

1. Create a net11.0 console project with C# 15.0, nullable and implicit usings. Add Exchange.cs above, the certificate helper below and the following statements in Program.cs (using System.Net.Security, System.Security.Authentication, System.Security.Cryptography.X509Certificates and Samples). Apply scoped #pragma warning disable/restore SYSLIB5007 around these experimental call sites too. No external package or workload is needed. The certificate variable is created with `using X509Certificate2 certificate = TransportCertificate.Create();` before these statements.

{{snippet:usage}}

The helper creates a private, short-lived fictional test identity, trusts only that certificate and checks the target name. It never modifies certificate stores. Production must use its real trust, revocation and identity policy. NoCheck and disabled downloads make the private fixture independent of public revocation services; they are not a production recommendation. Windows reloads the PFX into an ordinary temporary key container because Schannel cannot use this freshly generated ephemeral key directly. Disposing the certificate releases its key; PersistKeySet is not used.

{{snippet:certificate}}

Keep contexts alive until their sessions are disposed; using declarations above dispose in reverse order. Do not mutate shared authentication options or concurrently operate on one session. Copy or finish sending exactly bytesWritten before reusing output storage; advance input only by bytesConsumed. Spans are borrowed for the synchronous call, and pooled storage must not be returned while the transport still uses it. The sample copies output to make this ownership visible, rather than promising zero allocations.

## Constraints

- Complete means progress, not necessarily completed authentication: inspect IsHandshakeComplete. Continue Read after authentication to process post-handshake records such as tickets; Handshake alone does not consume them.
- NeedMoreData means retain any remainder and accumulate more ciphertext. EOF before a complete record/close must be handled as truncation, not a successful empty record.
- DestinationTooSmall requires draining staged output with DrainPendingOutput (or repeating with more space). Do not resend already-consumed plaintext. Handshake retries in this sample drain pending bytes before taking further input.
- NeedsCertificateValidation requires a decision before proceeding. AcceptWithDefaultValidation applies the configured trust and hostname policy and records the result; non-None rejects this fixture. Custom validation must explicitly pass the actual result to SetRemoteCertificateValidationResult. Never accept every certificate merely to advance the state.
- NeedsTlsContext supports server SNI/deferred context selection; CertificateRequested supports client certificate selection, including SetClientCertificateContext(null) to decline. This fixed-context, no-client-auth example fails on those unexpected states. Supporting them requires the application's own identity/selection policy.
- Closed ends processing. Send the bytes produced by Shutdown to deliver close_notify; disposing alone is not a peer notification.

RC2 Read requires destination space for the entire decrypted record: an undersized plaintext buffer can throw InvalidOperationException rather than return DestinationTooSmall. The 16,384-byte read buffer covers the ordinary TLS record plaintext bound used here. Ciphertext may exceed it through headers/tags; retain the complete record separately. Bound transport queues and impose handshake/application deadlines in real use. Protocol support, cipher suites and post-handshake client authentication vary by native provider. This Windows Schannel route does not certify Linux/OpenSSL, Apple TLS, browser, mobile or socket-session behavior.

## Evidence

The actual .NET 10 project compiles and runs a TCP SslStream echo. The RC2 project compiles and runs two real TLS state machines, fragmented handshakes, bidirectional encrypted messages, 50,000-byte multi-record transfer, wrong-host certificate rejection and close_notify. There is no fake TLS provider or mock handshake. In-memory ciphertext transfer establishes caller-owned I/O, not socket readiness, throughput or other-provider support. SYSLIB5007 is explicitly opted in; experimental stability persists independently of GA. Exact SDK/runtime records accompany the recipe; GA reconciliation remains required.
