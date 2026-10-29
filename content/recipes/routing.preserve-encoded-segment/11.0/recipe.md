# Preserve encoded route segments in proxy-style requests

## Goal

Keep encoded slashes inside identifiers when direct and proxy-style request targets reach Kestrel, and remove path compensation only after real wire-level comparison.

Exact reproduction uses SDK10.0.402 / framework10.0.13 / C#14 versus SDK11.0.100-rc.2.26504.105 / framework11.0.0-rc.2.26504.105 / C#15. These are tested pins, not minimum supported versions. RC2 is not actual-GA proof. The exact eight approved .NET10 foundations, stable identities, PIN-959/PIN-963/PIN-964 and unrelated Human gates remain unchanged.

## Older approach — .NET 10

A bird register distinguishes `/birds/{id}` from `/birds/{region}/{tag}`. On the actual serviced .NET10.0.13 server, an origin-form `GET /birds/reed%2F17 HTTP/1.1` selects bird-record with an encoded id, but an absolute-form request containing the same path selects regional-record with two segments. The sample first records that raw difference. Its bounded preserve mode restores only the demonstrated ASCII encoded bird-id shape from RawTarget before routing; raw parsing is retained separately in the response. Existing applications may instead reject that unsupported request shape rather than rewrite it.

This is a historical workaround, not a general proxy parser or an authorization normalization rule. The final decode mode is a negative control demonstrating why Uri.UnescapeDataString on the whole Path is unsafe. Neither handler grants access to protected data. Build and run the standalone files below with `dotnet run --project BirdRoutes.csproj -c Release`. No external package is restored.

### BirdRoutes.csproj

{{snippet:n10-birdroutes-csproj}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### Program.cs

{{snippet:n10-program-cs}}

### RuntimeIdentity.cs

{{snippet:n10-runtimeidentity-cs}}

## Newer approach — .NET 11

The RC2 Kestrel parser preserves encoded slash segments in both request-target forms. The same original fixture compiles for .NET11; its NET10-only preserve branch is absent. Leave Request.Path intact in the recommended raw/preserve hosts. The decode negative control still changes the selected handler, so a parser upgrade cannot make arbitrary application decoding safe.

Build/run these complete files in a separate directory using the pinned .NET11 SDK. Each run starts an ephemeral real server, writes paired structured results and exits. RawTarget, parser Path, middleware Path, selected endpoint, handler and route values are independent observations. The TCP request writer sends exact ASCII bytes and closes each connection, avoiding high-level URI normalization.

### BirdRoutes.csproj

{{snippet:n11-birdroutes-csproj}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### Program.cs

{{snippet:n11-program-cs}}

### RuntimeIdentity.cs

{{snippet:n11-runtimeidentity-cs}}

## Migration

1. Identify the actual server/runtime and capture origin-form and absolute-form bytes reaching it. Check intermediary rewrites separately; a client-side Uri comparison is insufficient.
2. Record RawTarget and parsed Path before authored middleware, then middleware Path, selected route and route values. Pair `%2F`, `%2f`, ordinary slash and identifier-case controls.
3. Run the old consumer first. Retain a bounded workaround or reject unsupported inputs if the actual baseline still splits encoded identifiers; do not invent a universal serviced-old result.
4. Upgrade and rerun the same wire inputs. Remove only the NET10 compensation whose purpose the new parser now meets. Keep business validation and route-specific authorization intact.
5. Exercise double encoding, incomplete/bad escapes, encoded NUL/CRLF and a literal space in the request line. Distinguish parser rejection from application matching; do not treat every accepted path as a valid identifier.
6. Keep the whole-path decode control as a regression demonstration. The encoded id must not silently select the regional handler in the recommended host.

## Constraints

RawTarget is the received request target, Path is the parsed server value, route values come from matching that Path, and middleware can change it before matching. Never assume those four representations are interchangeable.

The serviced .NET10.0.13 baseline actually splits an absolute-form %2F while its origin-form counterpart retains it. RC2 retains %2F/%2f on both forms. Ordinary slash and double-encoding already overlap; later serviced baselines must be measured rather than inferred from release-family labels.

The older preserve middleware is intentionally limited to this exact ASCII bird-id shape and HTTP absolute targets. It is not a general reverse-proxy parser. Prefer rejecting unsupported targets at the boundary or upgrading; never generalize it to Unicode, query strings, other schemes or arbitrary rewrites.

The decode mode is an explicitly unsafe failure control. Decoding the entire Path changes route boundaries on both targets, including double-encoded input. Remove demonstrated compensation; do not add decoding to authorization or convert a literal encoded identifier into a different resource key.

These diagnostic handlers deliberately report malformed/control values rather than accepting them into business storage. A real application must validate the opaque identifier against its own allowlist and enforce authorization after final routing. Parser acceptance is not application validity. Do not log untrusted control text without structured escaping.

No IIS, HTTP.sys, proxy, WAF, TLS, HTTP2/3, other OS or authorization-bypass protection claim follows from this loopback Kestrel fixture.

The complete files, configuration, lock, native-tool acquisition conditions and migration steps above are included offline. Restore/download preparation can need network access; installed recipe retrieval does not execute the samples or acquire missing prerequisites. There is no new recurring platform/browser matrix or public publication.

## Evidence

Actual raw TCP/Kestrel consumers execute nine paired targets in each of raw/preserve/decode modes per runtime, plus malformed request-line controls. They record RawTarget/Path/middleware/endpoint/handler/route values. Old encoded absolute targets split; the bounded older restoration works; current pairs retain the encoded segment without it. Ordinary slash, bad/incomplete escapes, double encoding and CRLF show the measured overlap. Encoded NUL is400 origin-form on both,200 absolute-form on10 and400 absolute-form on11; accepted CRLF is not asserted safe. Literal-space request lines return400. No mock context or proxy proof is substituted.

Fresh compilation/execution records retain exact commands, source/input hashes, resolved dependency graphs, platform and measured SDK/framework/native identities. Source audit metadata is reconciled with the actual selected artifacts separately from loaded runtime bytes. Run the repository's eng/Test-WebMigrationBatch.ps1 with both absolute SDK hosts and -RecordEvidence to refresh managed/tool/publish proof; browser proof must be captured independently after relevant layout/input changes. Installed ordinary-artifact list/get inspection compares the complete canonical body, goal, target11/edition11, applicability, fresh evidence and explicit unavailable target10 response. Proxy blocking establishes proxy independence, not network isolation or additional runtime execution.

## References

- [Pinned ASP.NET Preview4 encoded-slash change](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview4/aspnetcore.md)
- [ASP.NET route matching and encoded characters](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-11.0#routing-with-special-characters)
