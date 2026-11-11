# Use one trust operation for Windows and WSL

## Goal

A fictional estuary survey service runs on localhost inside WSL. Its developer needs both Linux clients and Windows clients to trust the same development certificate. Treat certificate trust and an actual successful trusted HTTPS request as separate checks.

## Older approach

Existing installations may export the WSL development certificate's public part, review its thumbprint and import that public certificate into the Windows Current User trusted roots separately from Linux trust. Keep a working reviewed setup; do not distribute the private key just to establish trust. Do not run broad cleanup or import commands that clear other development certificates.

Current Microsoft dev-certs documentation also describes cross-environment trust for .NET10 and later. Placement in the .NET11 release notes does not prove this was unavailable in every serviced .NET10 SDK. This recipe makes no verified older-target command-route claim: the .NET10 service below is a compiled comparison, and installed Linux .NET10 trust propagation has not been exercised.

## Newer approach

Run dotnet dev-certs https --trust from the intended WSL user environment with a supported SDK. With Windows interoperability enabled, the operation also attempts Windows Current User trust. Running the Windows command does not mean trust propagates into every Linux distro, container, browser-specific store or remote machine. Check the actual CLI output and each consuming client; an attempted Windows import is not success.

The checked environment is WSL2 Ubuntu24.04.4 and WSL2.6.3, with isolated Linux SDK11.0.100 and matching runtime. The owner ran the trust operation on an existing certificate. Windows Current User trust and Linux OpenSSL trust-directory files were added, but the CLI reported partial trust. NSS certutil was absent; browser-store trust was not established. A partial-trust message must be investigated for the clients actually in use.

The canonical services below bind Kestrel to loopback port56485 using UseHttps and expose /survey. Both compile with their declared SDKs and framework assets. The .NET11 service returned Estuary survey ready to Windows Schannel curl and WSL OpenSSL curl with normal certificate validation. The Linux invocation supplied the CLI trust directory through SSL_CERT_DIR. This proves those clients and conditions, not all Linux clients or browser stores. Port 54485 was inside a Windows excluded range and appears to have prevented localhost forwarding; check port availability on both systems before running.
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

1. Confirm WSL2, the intended Linux distro/user, Windows interoperability and access to Windows Current User certificate stores. Restore/build each service using SDK10.0.403/C#14/net10 or SDK11 GA/C#15/net11. No external packages or workloads are needed. Keep runtime requirements separate from these exact verification pins.
2. Have the owner review the specific trust operation in the WSL environment. The owner runs dotnet dev-certs https --trust there, reviews prompts and retains the certificate thumbprint and actual command output. Do not run it under a different/root identity just to bypass a prompt.
3. Check trust with dotnet dev-certs https --check --trust and inspect the Windows Current User store for the same public certificate. The machine-readable check can report Partial even when the intended clients work. For Ubuntu OpenSSL curl, use the CLI's user trust directory in a child process: `SSL_CERT_DIR="$HOME/.aspnet/dev-certs/trust:/usr/lib/ssl/certs" /usr/bin/curl --noproxy '*' --fail https://localhost:56485/survey`. NSS/browser trust additionally needs certutil and the relevant browser store; it was not tested here. Do not silently edit profiles or persistent environment variables.
4. Run the built EstuaryHttps service in WSL using the matching runtime and Development environment. Request https://localhost:56485/survey from WSL and Windows with normal certificate validation. Expect Estuary survey ready. In Windows PowerShell, use `C:\Windows\System32\curl.exe --noproxy '*' --fail https://localhost:56485/survey`. Do not disable verification, pin around a trust failure or infer connectivity from a build. WSL networking/localhost forwarding and port availability are prerequisites, not guaranteed by certificate trust.
5. Record exact SDK/runtime, distro, certificate identity, commands and both client results through the existing Human evidence process. Preserve other development certificates and owner trust settings. After a controlled test, remove only its newly added trust entries and stop its service; preserve pre-existing private certificates. Never use broad dev-certs cleanup for this purpose.

## Constraints

Development localhost only; no production certificate advice or external provider. Kestrel server HTTP endpoints have no Blazor rendering requirement. The framework assets are Microsoft.AspNetCore.App for each declared target. The .NET10 comparison has compilation evidence only; execution evidence covers the .NET11 RC3 service and the two named clients. Browser-store behavior and final-GA behavior are unverified. The 11.0 catalogue edition retains the .NET10 comparison without adding a ninth .NET10 foundation. Trust changes require the owner.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Official dotnet-dev-certs and ASP.NET Core HTTPS references are included in the source metadata. The saved compilation records identify actual artifacts and inputs. The controlled Windows/WSL observation, certificate identity, exact commands, failures and cleanup status are recorded in docs/verification/wsl-certificate-trust.md. That observation is RC evidence for the stated clients, not universal trust or GA verification.
