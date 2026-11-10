# Compose Cosmos numeric conversions in server queries

## Goal

Filter and label Bittern marsh readings in Cosmos using supported numeric expression casts with math and concatenation.

## Older approach

EF10 rejects the composed predicate. First filter to a bounded set of documents, materialize asynchronously and apply the conversion/math/label in memory. The client boundary is explicit and transfers the selected documents. Keep this valid alternative when CLR conversion semantics matter.

## Newer approach

EF11 accepts the numeric cast expression without a translation failure. The measured command omits numeric casts, contains FLOOR and ToString, and returns depth=12. This concerns expression-tree Convert nodes from C# casts. It does not add System.Convert.ToDouble(string) translation.

{{snippet:n11-program-cs}}

## Migration

1. Pin the actual SDK, target and provider graph shown below. Restore the independent locks; EF11 requires net11.0. The historical audit used SDK10.0.402 and SDK11 RC2; these consumers use SDK10.0.403 and SDK11 RC3 with Cosmos11 RC3.
2. Keep a selective server predicate before the EF10 materialization boundary. The original comparison reads only the valid document before doing client math and concatenation. Avoid materializing an unbounded container.
3. On EF11, compose numeric casts with the math expression and label in the query. Inspect ToQueryString and execute it. The example checks the actual returned label, not merely SQL text.
4. Treat numeric casts carefully: Cosmos omits numeric casts in the measured command. A cast to int does not establish CLR truncation or range validation. The range control still labels 1e100; do not use the cast as a business validation rule. Rounding/truncation migration remains the separate numeric-projection recipe.
5. Check boundaries independently. The null text document remains null in the explicit null projection. The invalid string method in a predicate is rejected before service execution; there is no supported server string-parsing route here. The EF10 explicit client conversion returns zero for null, throws FormatException for mud and accepts 1e100 as double. Those are CLR observations, not Cosmos conversion promises.
6. Run eng/Test-BitternConversions.ps1 -Record with the pinned SDK host. Each sample global.json pins its SDK; the Net10Dotnet/Net11Dotnet parameters select host executables only. It starts the approved loopback legacy emulator, creates unique owned databases, records real compilation/execution and removes each database. Certificate pinning does not change host trust. Keep async database I/O.

## Constraints

Windows x64 and the approved digest-pinned Cosmos emulator in gateway mode were exercised. Cosmos SDK3.63.1 and Newtonsoft.Json13.0.4 are external dependencies. No cloud parity, other platform, arbitrary casts, string parsing, overflow checking, GA verification or rendering mode is claimed. No experimental opt-in is needed for this bounded API route. The exact eight .NET10 foundations and existing editions remain unchanged. Installed recipe retrieval is offline; package restoration and emulator setup may need network access.

## References

- [Official cast/convert change](https://github.com/dotnet/efcore/pull/35000)
- [Pinned Preview6 source](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview6/efcore.md)
## samples/net10.0/BitternConversions/BitternConversions.csproj

{{snippet:n10-bitternconversions-csproj}}

## samples/net10.0/BitternConversions/Directory.Build.props

{{snippet:n10-directory-build-props}}

## samples/net10.0/BitternConversions/Directory.Build.targets

{{snippet:n10-directory-build-targets}}

## samples/net10.0/BitternConversions/Directory.Packages.props

{{snippet:n10-directory-packages-props}}

## samples/net10.0/BitternConversions/EmulatorTransport.cs

{{snippet:n10-emulatortransport-cs}}

## samples/net10.0/BitternConversions/global.json

{{snippet:n10-global-json}}

## samples/net10.0/BitternConversions/Program.cs

{{snippet:n10-program-cs}}

## samples/net11.0/BitternConversions/BitternConversions.csproj

{{snippet:n11-bitternconversions-csproj}}

## samples/net11.0/BitternConversions/Directory.Build.props

{{snippet:n11-directory-build-props}}

## samples/net11.0/BitternConversions/Directory.Build.targets

{{snippet:n11-directory-build-targets}}

## samples/net11.0/BitternConversions/Directory.Packages.props

{{snippet:n11-directory-packages-props}}

## samples/net11.0/BitternConversions/EmulatorTransport.cs

{{snippet:n11-emulatortransport-cs}}

## samples/net11.0/BitternConversions/global.json

{{snippet:n11-global-json}}

## samples/net11.0/BitternConversions/Program.cs

{{snippet:n11-program-cs}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Independent locked builds and real focused execution were recorded on Windows x64 with SDK10.0.403 and SDK11 RC3. Compilation, service execution and installed offline retrieval establish separate claims. Review the embedded evidence entries for exact inputs, dependencies and command outputs; freshness describes package-build inputs, not the consumer machine or GA. Cosmos uses the approved loopback emulator; cloud parity remains unverified.
