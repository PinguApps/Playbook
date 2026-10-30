# Configure registry authentication realms accepted by the SDK

## Goal

Estuary publishes a cargo worker through a registry that asks the SDK to obtain a
bearer token. Configure that challenge so the SDK can authenticate without sending
credentials to an unintended destination.

## Requirements and actual artifacts

This edition serves target11 only. The valid net10 publication remains a comparison
inside this body; it adds neither a historical foundation nor an older-target
SDK11 validation route. The ordinary SDK container-publishing feature is used;
there is no independently versioned container task package or standalone
containerize tool requirement.

| Layer | Older comparison | Current example |
| --- | --- | --- |
| SDK / compiler language | 10.0.402 / C#14 | 11.0.100-rc.2.26504.105 / C#15 |
| Target / runtime | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| Executed platform | Linux x64 Docker SDK image | Linux x64 Docker SDK image |
| Published base variant | Azure Linux3 runtime10.0.13 | Azure Linux4 **Beta** runtime11RC2 |
| Registry / copy helper | Distribution3.1.2; ORAS1.3.6 | Same disposable backend and helper |

Exact SDK/base/registry image digests, compiler/task hashes and resolved framework
artifacts are recorded in the sample inputs/evidence, separately from supported
prerequisites. A real deployment needs an authorized registry and token service,
trusted TLS certificates and a compatible runtime/base image. The fixture uses
generated disposable credentials and a child-only CA bundle. It changes no host
trust store, Docker daemon configuration, persistent environment or real login.
There is no UI, rendering, workload or native compiler dependency. RC2 and the
selected Beta base are not GA qualification or a production base-image recommendation.

## Older approach

Save the older project and program together, using SDK10.0.402 with roll-forward
disabled. This is an original framework-dependent cargo worker, not a copied
release-note application.

{{snippet:older}}

{{snippet:program}}

The real older SDK publishes and executes the image with a valid HTTPS challenge.
It also supports the deliberately configured same-host local HTTP fixture. Do not
infer that its authentication protections equal the newer SDK: malicious challenge
controls run only on SDK11, so older tools never receive test credentials for
an unsafe realm.

## Newer approach

Save the same program beside this current project. Select
SDK11.0.100-rc.2.26504.105 using `global.json`, `rollForward: "disable"`.

{{snippet:newer}}

The challenge's `realm` must be an absolute URI with HTTPS. HTTP is permitted
only for a registry deliberately configured as insecure. The actual SDK guards
blocked IP literals, including private, loopback, link-local and unspecified
addresses, and reserved localhost names. This is not a promise to resolve and
approve every DNS address, prevent DNS rebinding, or require ordinary DNS hosts
to equal the registry host. In the verified TLS case, registry `estuary.test`
uses a different token DNS name, `auth.estuary.test`.

The local exception is narrow: an insecure IP-literal registry may use its own
blocked IP as the realm host, independent of port. A different blocked IP remains
invalid. Do not mark an untrusted registry insecure to defeat this validation.
No global validation-disabling switch is taught.

## Migration

1. Inspect the actual `WWW-Authenticate: Bearer` response and identify the intended
   token service. Redact credentials, bearer tokens and secret-bearing URLs before
   saving traces. Stop when ownership or authorization is unclear.
2. Make the service advertise an absolute HTTPS realm, with a certificate trusted
   by the publishing process. Fix the service configuration rather than relying
   on a relative path, non-HTTP scheme or blocked IP literal.
3. Keep the worker project explicit. Publish via the actual SDK's
   `dotnet publish -c Release -t:PublishContainer`, selecting the authorized
   `ContainerBaseImage`, `ContainerRegistry`, repository and tag. Supply credentials
   through that child process's credential configuration; do not put them in the
   project, command-line arguments or logs.
4. For an explicitly authorized disposable local service only, the verified
   `DOTNET_CONTAINER_INSECURE_REGISTRIES=127.0.0.1:5080` child setting permits an
   HTTP realm at that same host. It is not a production TLS migration and does not
   allow the fixture's `127.0.0.2` mismatched-host control.
5. Reproduce the current failures using the real publishing task: relative `token`,
   FTP realm, HTTP realm on a secure registry, private IP, loopback IP, localhost
   and insecure-registry/mismatched-IP controls. Each must fail with CONTAINER1019
   before a token request or image manifest write; a mocked URI validator is insufficient.
6. Confirm actual token acquisition and registry manifest upload for HTTPS and
   the explicit same-host local exception. Retrieve those exact pushed image bytes
   and execute them with networking disabled. Expect `ESTUARY_CONTAINER: copper lanterns`.
7. Remove the owned registry/SDK containers and network. Discard generated
   credentials and the private trust bundle; keep only redacted evidence and
   ordinary image/cache artifacts. Retest with the actual registry before claiming
   hosted-service interoperability, redirect safety or a wider DNS security contract.

## Complete disposable fixture

The authoring fixture below runs behind real TLS and forwards authenticated blob
and manifest operations to Distribution, rather than replacing SDK validation.
Its credentials/token/root key are generated for one run. Never use its constant
permissions or test token endpoint as a production authentication service.
Save the first block as `RegistryFixture.csproj` and the second as `Program.cs`
in a separate directory. Build with SDK10.0.402. The Linux SDK container runs this
net10 helper on its available runtime, explicitly allowing Major/prerelease
roll-forward for the fixture alone; the worker uses its own exact SDK/target.
The helper arguments are the original worker directory, writable work directory,
private Distribution backend URL and worker major version (`10` or `11`).
It requires `estuary.test` and `auth.estuary.test` to map to loopback inside that
disposable container, and the backend to contain `estuary/base:<major>`.

{{snippet:fixture-project}}

{{snippet:fixture}}

## Constraints

The gateway and generated trust/credentials are disposable test infrastructure.
The local insecure exception is scoped to its own host; it grants no blanket
permission to contact private token endpoints. DNS checks, redirect protection,
hosted service interoperability and production authorization policy require their
own qualification. The current selected Azure Linux4 base remains Beta.

## Evidence

`eng/Test-EstuaryRegistry.ps1 -Record` seeds exact base manifests/blobs using ORAS,
starts the disposable Distribution backend, and runs both pinned SDK containers
on an internal network. Only the backend also has a host-loopback seeding port;
the SDK containers have no external network route. Each original worker is built,
executed, then really pushed through the challenge service. Token and HTTP exchanges
are retained with authorization values redacted. Positive image bytes are copied
back through an OCI layout and executed by Docker without networking. The seven
current negative controls must cause no token request and no manifest write.

The authoring harness requires PowerShell 7.4 or later for its HTTP timeout options.

No real credentials leave the disposable service. No SDK internals are called by
reflection. No arbitrary registry, production auth protocol, redirect, DNS-rebinding,
other-platform or GA guarantee follows. Fresh compilation/evidence and installed
offline retrieval remain required. PIN-959/PIN-963, the exact eight foundations
and unrelated Human gates remain open; no recurring service/OS matrix is added.

## Sources

- [Official SDK change and realm requirements](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview5/sdk.md#container-publishing-validates-bearer-token-realms)
- [Pinned SDK authentication implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/sdk/src/Containers/Microsoft.NET.Build.Containers/AuthHandshakeMessageHandler.cs)
- [SDK container authentication](https://learn.microsoft.com/en-us/dotnet/core/containers/overview)
