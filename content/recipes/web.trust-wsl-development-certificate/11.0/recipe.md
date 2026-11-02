# Use one trust operation for Windows and WSL

## Goal

A fictional estuary survey service runs on localhost inside WSL. Its developer needs both Linux clients and Windows clients to trust the same development certificate. Treat certificate trust and an actual successful trusted HTTPS request as separate checks.

## Older approach

Existing installations may export the WSL development certificate's public part, review its thumbprint and import that public certificate into the Windows Current User trusted roots separately from Linux trust. Keep a working reviewed setup; do not distribute the private key just to establish trust. Do not run broad cleanup or import commands that clear other development certificates.

Current Microsoft dev-certs documentation also describes cross-environment trust for .NET10 and later. Placement in the .NET11 release notes does not prove this was unavailable in every serviced .NET10 SDK. This recipe makes no verified older-target command-route claim: the .NET10 service below is a compiled comparison, and installed Linux .NET10 trust propagation has not been exercised.

## Newer approach

Run dotnet dev-certs https --trust from the intended WSL user environment with a supported SDK. With Windows interoperability enabled, the operation also attempts Windows Current User trust. Running the Windows command does not mean trust propagates into every Linux distro, container, browser-specific store or remote machine. Check the actual CLI output and each consuming client; an attempted Windows import is not success.

The authoring environment is WSL2 Ubuntu24.04.4 and WSL2.6.3, with an isolated Linux SDK11.0.100-rc.2.26504.105. Its actual dev-certs https --help exposes --trust, --check and --check-trust-machine-readable. No trust, generation, import or clean command was run for this guidance. CLI help proves command discovery only.

The canonical services below bind Kestrel to loopback port54485 using UseHttps and expose /survey. Both compile with their declared SDKs and framework assets. Execution using the user's real development certificate, Windows/WSL trust and cross-environment connectivity remain pending H_WSL_PROOF / PIN-956. Certificate generation alone cannot clear that gate.
### n10-directory-build-props
{{snippet:n10-directory-build-props}}

### n10-directory-build-targets
{{snippet:n10-directory-build-targets}}

### n10-directory-packages-props
{{snippet:n10-directory-packages-props}}

### n10-global-json
{{snippet:n10-global-json}}

### n10-estuaryhttps-csproj
{{snippet:n10-estuaryhttps-csproj}}

### n10-program-cs
{{snippet:n10-program-cs}}

### n11-directory-build-props
{{snippet:n11-directory-build-props}}

### n11-directory-build-targets
{{snippet:n11-directory-build-targets}}

### n11-directory-packages-props
{{snippet:n11-directory-packages-props}}

### n11-global-json
{{snippet:n11-global-json}}

### n11-estuaryhttps-csproj
{{snippet:n11-estuaryhttps-csproj}}

### n11-program-cs
{{snippet:n11-program-cs}}


## Migration

1. Confirm WSL2, the intended Linux distro/user, Windows interoperability and access to Windows Current User certificate stores. Restore/build each service using SDK10.0.402/C#14/net10 or SDK11 RC2/C#15/net11. No external packages or workloads are needed. Keep runtime requirements separate from these exact verification pins.
2. Have the owner review the specific trust operation in the WSL environment. The owner runs dotnet dev-certs https --trust there, reviews prompts and retains the certificate thumbprint and actual command output. Do not run it under a different/root identity just to bypass a prompt.
3. Check trust with dotnet dev-certs https --check --trust and inspect the Windows Current User store for the same public certificate. Linux OpenSSL/NSS consumers can require documented trust-directory configuration; review CLI output and use session-scoped configuration rather than silently editing profiles or persistent environment variables.
4. Run the built EstuaryHttps service in WSL using the matching runtime. Request https://localhost:54485/survey from WSL and Windows with normal certificate validation. Expect Estuary survey ready. Do not disable verification, pin around a trust failure or infer connectivity from a build. WSL networking/localhost forwarding and port availability are prerequisites, not guaranteed by certificate trust.
5. Record exact SDK/runtime, distro, certificate identity, commands and both client results through the existing Human evidence process. Leave H_WSL_PROOF pending until those actual observations are returned. Preserve other development certificates and owner trust settings; remove only explicitly agreed test resources.

## Constraints

Development localhost only; no production certificate advice or external provider. Kestrel server HTTP endpoints have no Blazor rendering requirement. The framework assets are Microsoft.AspNetCore.App for each declared target. Both canonical services have compilation evidence only; no Linux execution, trusted Windows/WSL request or browser-store behavior is claimed. The 11.0 catalogue edition retains the .NET10 comparison without adding a ninth .NET10 foundation or claiming final-GA proof. Trust changes require the owner and remain pending; this Agent delivery does not perform them.

## Evidence

Official dotnet-dev-certs and ASP.NET Core HTTPS references are included in the source metadata. The cross-environment action is documented there; current documentation does not replace proof against the actual SDK/distro/client combination. The saved compilation records identify actual artifacts and inputs.
