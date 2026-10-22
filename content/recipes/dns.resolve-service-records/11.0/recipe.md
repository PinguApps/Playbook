# Resolve advertised service targets and ports

## Goal

Find the target host, port, priority, weight and lifetime advertised by an SRV record. Distinguish a missing DNS name from a successful lookup with no service records before deciding where to connect.

## Older approach

.NET 10's built-in host-address lookup does not return SRV records. DnsClient 1.8.0 is a valid managed alternative with an actual net8.0 assembly consumed by the net10.0 comparison. The sample pins that package, explicitly chooses a resolver, disables its result cache and normalizes cancellation because this package can wrap cancellation in DnsResponseException. This is a previous-release comparison, not an extra .NET 10 catalogue foundation.

{{snippet:older}}

For the older project, set TargetFramework=net10.0, LangVersion=14.0, nullable/implicit usings enabled, and reference DnsClient **1.8.0**. In a centrally managed project put PackageVersion in its own Directory.Packages.props and PackageReference in the project. Restore and retain packages.lock.json. No third-party source implementation is copied here.

## Newer approach

.NET 11 adds typed Dns.ResolveSrvAsync using the system resolver and DnsResolver.ResolveSrvAsync for an explicitly configured resolver. DnsResult<SrvRecord> carries ResponseCode, Records and NegativeCacheTtl. The example uses a reusable, disposable custom resolver and projects advertised record fields into an application result; it deliberately does not cache or choose a winning endpoint.

{{snippet:newer}}

## Migration

1. Use net11.0/C# 15.0 and System.Net. These APIs are framework assets, with no extra package/workload or experimental diagnostic in the reviewed RC2 contract. The older package need not remain merely for this SRV operation; check other package uses before removing it.
2. Select system configuration via Dns.ResolveSrvAsync(name, token), or supply trusted server endpoints to DnsResolver. Do not change machine DNS settings to run this sample. Example for an application-owned DNS endpoint:

{{snippet:usage}}

Supply resolverEndpoint as a trusted IPEndPoint and cancellationToken from the caller. A public resolver will not contain this fictional zone; the verification fixture serves it locally.

3. Handle NoError plus an empty Records list (NODATA) separately from NxDomain (name absent), ServerFailure, Refused and other statuses. NoError is not evidence that a service record exists. Invalid names/options can throw argument exceptions; disposed resolvers throw ObjectDisposedException. Honor OperationCanceledException. A DNS status is not a connection success signal.
4. The returned TTL describes record lifetime, not a connection lease. If adding a cache, keep positive and negative caching separate and inspect the original DnsResult.NegativeCacheTtl. Zero means no usable negative-cache duration; do not invent one. Windows' native resolver can discard negative authority/SOA records, yielding zero. This projection intentionally drops negative-cache metadata because it does not cache.
5. SRV lookup does not select or connect an endpoint. Use lowest priority first and weight-based selection among that priority following RFC 2782; record order is not a load-balancing policy. Target `.` means the service is unavailable. Resolve selected targets to addresses as necessary; SrvRecord.Addresses may contain additional-section address records but need not. Preserve TLS name validation, origin/port restrictions and failure/retry policy before connecting to advertised destinations.

## Constraints

Windows uses DnsQueryEx. Custom servers must all use one address family, and ports must be 0/default or 53; another nonzero port throws PlatformNotSupportedException and mixed families throw ArgumentException. Linux/macOS use the managed DNS resolver route with platform resolver configuration; these are source-reviewed platforms, not native execution claims here. Android/WASI are unsupported by these typed APIs; browser/mobile routes are not claimed. Resolver policy, search suffixes, OS caching and network reachability still matter. Do not equate an SRV answer with authenticated service discovery or DNSSEC validation.

## Evidence

Actual .NET 10 and RC2 executables query a controlled UDP DNS server bound only to loopback (127.0.0.1:53 on Windows; an ephemeral port elsewhere). It returns the original collector.harbour.test record with port 4719, priority 7, weight 11 and TTL 90. Unique query names avoid stale cross-run cache answers. Both checks assert every projected field, distinguish NXDOMAIN/NODATA and check cancellation; the Windows RC2 run also checks custom-port rejection. No system resolver settings, hosts entries, accounts or external zones are changed. This is a real resolver/wire exchange, not a mock, external DNS proof, endpoint-connection test or GA reconciliation.
