# Transport platforms evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-752-755-756.md).

## Actual evidence and limits

Eight independent console projects compile with zero warnings/errors under SDK
10.0.402/C# 14.0 and 11.0.100-rc.2.26504.105/C# 15.0. Actual runtime versions are
10.0.13 and 11.0.0-rc.2.26504.105. Samples inherit only the isolated samples build
boundary; no external packages/workloads or new automatic update grants are needed.
Evidence includes declared shared input hashes and exact build/execution output.

- TLS: .NET 10 TCP SslStream echo; RC2 caller-owned TLS 1.3 state machines,
  fragmented handshake, small ciphertext output, bidirectional messages,
  50,000-byte multi-record transfer, hostname rejection and close_notify. These
  are real cryptographic operations with in-memory ciphertext delivery, not a
  mock provider or socket-readiness/throughput test. RC2 plaintext undersizing
  throws; the implementation uses a complete-record destination. Schannel needs
  the fixture PFX reload; certificate stores are untouched.
- QUIC: both releases establish a real loopback TLS 1.3 connection through
  MsQuic 2.5.11.159415436 and exchange two original messages. RC2 checks default
  127, configured 240/16 and 0/255. This proves actual API/native use, not arrival
  order, fairness, congestion behavior or bandwidth.
- Crash: two network-disabled Linux x64 containers compile unchanged canonical
  inputs and terminate their own child with FailFast. Both check an actual
  `.crashreport.json` file for DispatchFailure and reject unexpected non-report
  artifacts. .NET 10 uses createdump; RC2's in-process report also prints managed
  frames/modules and stores its report under the configured private root. Child
  exit is 134. Crash directories and containers are removed; no production process
  or host crash configuration is used. Minidump precedence and the NativeAOT-only DOTNET_DbgCreateDumpToolPath directory switch are source-reviewed. That switch is not a CoreCLR workaround or a verified NativeAOT route.
- Hardware: real Windows x64 CPUID and runtime execution; the newer policy also
  runs under .NET 10. Clearing each new required bit verifies policy rejection
  only. This does not execute an unsupported CPU or certify OS/application support.

Other TLS providers, Linux/macOS QUIC, macOS/mobile crash capture, NativeAOT,
Arm64 and unsupported hardware startup are not executed. They are constrained or
source-reviewed, not required Human proof for these bounded routes. RC2 execution
is not GA verification. Generated evidence and the small source manifest remain
reviewable under the pragent-ignore assessment; no exclusion is justified.
