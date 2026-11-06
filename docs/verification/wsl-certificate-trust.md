# Controlled Windows and WSL certificate trust

PIN-956, 6 November 2026. Pingu performed the owner trust operation; the agent
independently inspected the stores and executed both HTTPS clients. This is
exceptional manual capability evidence under the contribution policy.

## Conditions and inputs

- Windows 10.0.26200.9711; WSL 2.6.3, kernel 6.6.87.2-1, Ubuntu 24.04.4,
  user pingu (uid1000), NAT networking and Windows interoperability enabled.
- Linux SDK 11.0.100-rc.3.26522.148 extracted from repository-pinned SDK image
  `sha256:9fc2f44fba54c579b9b650f6feea29beff050fb030d797eac697de85a94ad77f`.
  Microsoft.NETCore.App and Microsoft.AspNetCore.App 11.0.0-rc.3.26522.148.
- Canonical `samples/net11.0/EstuaryHttps` copied without source changes to
  `/home/pingu/.cache/playbook-pin956/sample`; SDK at the sibling `sdk` directory.
  The checked revision changes only the endpoint port from 54485 to 56485.
  Saved sample compilation evidence records the canonical input hashes.
- Existing certificate: CN=localhost, SHA1
  `A4C492A4903F40831CDF8A14EB29FB9DD5B367C2`, SHA256
  `761D171E1F797E245DC096B98E8159CEF8E684250ED68E7B34461DBB94EFD905`,
  valid 12 August 2026–12 August 2027. Its private PFX existed before this test.
- Windows curl 8.21.0, Schannel; WSL curl 8.5.0, OpenSSL 3.0.13.

## Observed actions and results

All dotnet child commands used `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`,
`DOTNET_GENERATE_ASPNET_CERTIFICATE=false`, `DOTNET_CLI_TELEMETRY_OPTOUT=1` and
`DOTNET_ROOT=/home/pingu/.cache/playbook-pin956/sdk`. No PATH/profile changes,
sudo, broad certificate cleanup, certificate generation or private-key export.

1. RC3 `dev-certs https --check --trust --verbose` initially exited1: existing
   certificate valid but untrusted. The matching Windows CurrentUser Root entry
   and Linux user OpenSSL trust directory were absent.
2. Pingu ran `/home/pingu/.cache/playbook-pin956/sdk/dotnet dev-certs https
   --trust --verbose` from the sample directory. He returned the CLI's partial
   trust warning. The agent found the matching Windows CurrentUser Root entry
   and Linux `~/.aspnet/dev-certs/trust/aspnetcore-localhost-<SHA1>.pem` plus
   `ce275665.0` hash symlink. The machine-readable check reported `Partial`.
   NSS certutil was absent. No browser trust is claimed.
3. Build: `dotnet build EstuaryHttps.csproj -c Release
   -p:RestoreLockedMode=true`, using the absolute RC3 executable: passed with
   zero warnings/errors. Bounded execution used `/usr/bin/timeout 90s`,
   `ASPNETCORE_ENVIRONMENT=Development` and
   `dotnet bin/Release/net11.0/EstuaryHttps.dll`.
4. Before port correction, WSL curl with default trust failed60 (self-signed);
   Windows curl failed7 (connection) while WSL's listener was present.
   `netsh interface ipv4 show excludedportrange protocol=tcp` and its IPv6
   equivalent showed 54434–54533 reserved, including 54485. No reservations were
   changed. Windows loopback bind at 56485 passed before selecting that port.
5. After rebuilding the canonical port56485 sample, Windows
   `C:\Windows\System32\curl.exe --noproxy '*' --max-time 5 --fail --silent
   --show-error https://localhost:56485/survey` exited0 and returned
   `Estuary survey ready`.
6. WSL `/usr/bin/env
   SSL_CERT_DIR=/home/pingu/.aspnet/dev-certs/trust:/usr/lib/ssl/certs
   /usr/bin/curl --noproxy '*' --max-time 5 --fail --silent --show-error
   https://localhost:56485/survey` exited0 with the same body. Both clients used
   normal hostname and certificate validation, without insecure options or
   certificate-validation callbacks.

## Cleanup and limits

Pingu authorized cleanup of all test additions. The agent checked the Linux PEM
fingerprint and hash-link target, removed only that PEM/link pair, and confirmed
the original private PFX remains. Both test listeners (54485 and 56485) are absent.
Windows removal with PowerShell's Certificate provider failed for both the agent
and owner with `The operation is on user root store and UI is not allowed.`
The authorized exact cleanup succeeded using
`C:\Windows\System32\certutil.exe -user -delstore Root A4C492A4903F40831CDF8A14EB29FB9DD5B367C2`.
The agent independently confirmed the Windows entry absent, Linux trust files
absent, original Linux private PFX present and both test listeners absent. Other
pre-existing Windows localhost roots remained present. The corrected cleanup
script reran successfully with the entries already absent. No broad certificate
cleanup was performed. Temporary SDK/sample files remain disposable scratch.

This proves the .NET11 RC3 service with the two named clients, including required
Linux process-local trust-directory configuration. It does not prove NSS/browser
trust, .NET10 Linux execution, arbitrary distros, containers or GA behavior.
Refresh execution proof after relevant sample, SDK/runtime, trust/client or
networking changes. Recheck port availability on the actual host.

Official sources: [dotnet-dev-certs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs)
and [Linux and WSL trust configuration](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0#linux-specific-considerations).
