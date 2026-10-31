# Inspect typed DNS routing and metadata records

## Goal

Read A/AAAA addresses, mail routing, text metadata, aliases, reverse names and
name servers with their record lifetimes. A Reedbank station inspector compares
zone records from an explicitly chosen resolver without connecting to any
advertised address. TXT string boundaries and MX preference must survive the
projection; a successful empty answer must remain distinct from an absent name.

## Shared resolver model

Use the resolver/envelope explanation in
[Resolve advertised service targets and ports](../../dns.resolve-service-records/11.0/recipe.md).
It owns SRV priority/weight selection, service-unavailable handling and connection
policy. This recipe extends that model to non-SRV records; it selects no service.
Essential instructions for this inspector follow offline.

Both implementations return this application projection. Values is an ordered
field list: an address; MX exchange then decimal preference; each TXT string;
or the single alias/reverse/server name. Records stays separate from Status and
NegativeTtl. ReverseName means the PTR query kind, not a memory pointer.

{{snippet:model}}

## Older approach

.NET 10's built-in address lookup lacks the full typed records/TTL/status
envelope. DnsClient 1.8.0 is a working managed alternative, not an unsupported
backport of .NET 11. The actual comparison consumes its lib/net8.0/DnsClient.dll
on net10.0, pins the package and keeps a lock file. Disable the package cache for
this diagnostic inspector and retain caller cancellation, including the package's
possible DnsResponseException cancellation wrapper.

{{snippet:older}}

Set TargetFramework=net10.0, LangVersion=14.0, nullable/implicit usings enabled.
Use PackageReference Include="DnsClient" and PackageVersion Include="DnsClient"
Version="1.8.0" in the sample's isolated Directory.Packages.props. Restore before
using RestoreLockedMode=true; retain packages.lock.json. No third-party source
implementation is copied. This comparison adds no .NET 10 foundation.

## Newer approach

.NET 11 provides DnsResolver and typed record results in System.Net, without an
extra NuGet package. The reusable resolver below disposes its resources and
preserves the result's negative-cache TTL. TrimEnd removes only DNS presentation
root dots from names; TXT strings are preserved individually.

{{snippet:newer}}

## Migration

1. Build net11.0 with the .NET 11 SDK/shared framework. The canonical project
   declares C# 15.0, nullable and implicit usings. No workload, package, rendering
   mode or experimental opt-in is needed. Exact RC2 pins identify the test, not
   an assertion about final GA behavior. Remove DnsClient only if no other use
   still needs it.
2. Create ZoneInspector with a trusted IPEndPoint, keep it for repeated queries,
   and dispose it afterward. Call QueryAsync(name, RecordKind.Mx, token) for mail
   records, or choose any listed kind. Supply the caller's CancellationToken.
   For A and AAAA, the helper explicitly requests InterNetwork or InterNetworkV6;
   it does not treat address-family order as routing policy. System-configured
   queries are available through corresponding Dns.Resolve*Async methods; the
   fixture deliberately uses explicit servers and never changes machine DNS.
3. For PTR, pass a reverse owner such as 23.2.0.192.in-addr.arpa with
   RecordKind.ReverseName. The framework also offers ResolvePtrAsync(IPAddress)
   if you want it to construct that name; that overload and IPv6 reverse-name
   construction are not exercised by this sample. CNAME inspects the alias
   target; it does not establish that target's addresses or identity.
4. Treat NoError with zero Records as NODATA; NxDomain means the queried name
   is absent. Other status values, such as ServerFailure/Refused, remain separate
   from transport/argument/disposal exceptions and OperationCanceledException.
   Do not interpret an empty list alone as success or nonexistence.
5. Preserve each record's Ttl if implementing a cache. For a negative answer,
   NegativeCacheTtl is best-effort: RFC2308 uses min(SOA record TTL, SOA MINIMUM).
   No authority SOA means zero here. Windows DnsQueryEx can drop the authority
   records, so the same wire answer can produce zero on Windows and 45 seconds
   on Linux. Zero supplies no usable negative-cache duration. Never substitute a
   guessed cache duration, use a positive record TTL for a missing name, or
   mistake the lifetime for a lease on the advertised service. This inspector
   implements no cache or retry policy of its own.

## Constraints

Windows uses native DnsQueryEx in DNSAPI. Explicit servers must all have one
address family and use port 0/default or 53; another nonzero port throws
PlatformNotSupportedException and mixed families throw ArgumentException.
On Windows, the local fixture requires free loopback UDP 53 on 127.0.0.1 and 127.0.0.2. It fails
if occupied; do not stop an unrelated DNS service or change resolver settings.

The current Linux route builds/parses DNS messages in managed code and uses
framework sockets over the OS network stack. It supports explicit custom ports;
no separate libresolv package is required for this explicit-server route. Without
explicit servers it consults /etc/resolv.conf. The real Linux fixture uses two
ephemeral loopback ports inside a network-none container. Normal runtime native
dependencies and working UDP/socket support are still required. These tests do
not measure system resolver suffix/search/cache behavior, TCP truncation fallback,
public zones, encrypted DNS, DNSSEC or reachability of advertised destinations.

Windows x64 and Linux x64 are executed. macOS contracts are source-reviewed only;
mobile/browser/WASI routes and other native configurations are not claimed.
OS caching can affect results; unique fixture query names avoid cross-run answers.
No DNS response authenticates a peer or authorizes connecting to it. Keep endpoint
allowlists and TLS validation in the actual client. No experimental diagnostics
exist in the reviewed RC2 contract; GA reconciliation remains PIN-962's work.

## Evidence

SDK 10.0.402/runtime 10.0.13 with DnsClient 1.8.0 and SDK 11RC2/runtime 11RC2 with
framework-only APIs compile and execute against actual authoritative loopback UDP
fixtures. They verify two A addresses (TTL 91), two AAAA addresses (TTL 118),
MX exchange/preference 17 (TTL 105), two separate TXT strings (TTL 106),
CNAME/PTR/NS names (TTLs 95/102/92). Each record set uses a consistent TTL;
native resolvers can normalize unequal TTLs within a set. Negative fixtures
distinguish NODATA/NXDOMAIN, SOA TTL 120/MINIMUM 45 and absent
SOA. Two server endpoints return different station metadata for the same name.
Cancellation runs before a query and after a dropped request reaches the server.
Windows RC2 also rejects a custom port and mixed address families.

Separate Linux execution records use the exact digest-pinned 10.0.13/11RC2 runtime
images, read-only application mounts, private tmpfs and --network none with
loopback available. They verify all the same fields plus positive negative-TTL
and custom-port behavior. Containers and sockets are removed; no public zone,
host resolver/trust configuration or persistent setting is changed.

Refresh with eng/Test-IdentityRecords.ps1 -Record -VerifySources. Saved records
retain exact commands/inputs, resolved package/archive/executed assembly, compiler,
CoreLib, audit-matching name-resolution references and Windows DNSAPI identity.
RC2 is not GA proof. PIN-959, PIN-962 and existing Human gates remain open, with
exactly eight foundations and no new recurring OS/client/native matrix.
