# Encrypt a CMS envelope for a pure ML-KEM recipient

## Goal

A fictional harbour dispatch office sends an encoded CMS envelope to a recipient whose authenticated certificate contains a pure ML-KEM public key. Keep content encryption, recipient key encapsulation and certificate trust as separate responsibilities. The platform and package asset must implement this recipient type.

## Older approach

The .NET 10 comparison encrypts CMS to an RSA recipient with OAEP-SHA256 and AES-256-CBC content encryption. This is a valid classical envelope, not post-quantum protection. Although .NET 10 provides ML-KEM primitives, this baseline does not provide the newer CMS KEM recipient contract. A raw encapsulation call is not a compatible CMS envelope implementation.

{{snippet:older}}

## Newer approach

The .NET 11 Pkcs net11.0 asset supports pure ML-KEM CMS on the non-Windows route. MLKem.IsSupported is necessary but not a cross-platform CMS support guarantee, so the example also rejects Windows. The receiver inspects the decoded recipient type before decrypting with its private key.

{{snippet:newer}}

CmsRecipient.CreateForKeyEncapsulation includes the user keying material (UKM) field even when the supplied span is empty. A normal CmsRecipient constructor instead omits UKM. These are different encodings; choose the one required by the receiver's protocol, rather than treating an empty field as absent. The library derives/wraps the content key internally.

## Migration

1. Create a net11.0 console project with C# 15.0, nullable and implicit usings enabled. Add System.Security.Cryptography.Pkcs version 11.0.0-rc.2.26504.105 for the verified RC2 baseline. Import System.Security.Cryptography, System.Security.Cryptography.Pkcs, System.Security.Cryptography.X509Certificates and Harbour.Envelopes. The selected asset is lib/net11.0/System.Security.Cryptography.Pkcs.dll, not the Windows runtime asset or the package's older-target assemblies.
2. Run on a supported non-Windows provider with pure ML-KEM capability; the recorded route is Linux x64/OpenSSL 3.5.5. Check MLKem.IsSupported and retain the explicit Windows rejection. A newer package's net10.0 asset does not establish the same public contract; no conditional .NET 10 route is offered.
3. Supply an authenticated recipient X509Certificate2 containing an ML-KEM public key matching the recipient's private key. For an already trusted PEM certificate, X509Certificate2.CreateFromPem loads the public certificate. Loading does not validate identity, chain, purpose or expiry: apply your trust policy before Encrypt. The sample's ephemeral certificate is only a local test identity signed by a temporary RSA issuer.
4. Pass the message bytes and trusted certificate to DispatchEnvelope.Encrypt. Transmit/store the returned CMS bytes. On the receiver, load the protected private key through your key-management boundary into MLKem and call DispatchEnvelope.Decrypt with that key and the encoded envelope. This example expects exactly one KEM recipient; a multi-recipient application must select and validate its intended recipient explicitly.
5. Dispose the certificate and key. Treat decode, recipient-shape or unwrap failures as operation failures; never fall back to another algorithm after failure. Erase caller-owned secret/message buffers when their lifetime ends. Do not export private key material for transport or log decrypted messages.

## Constraints

Pure ML-KEM and these demonstrated CMS members have no experimental diagnostic in the .NET 11 assets inspected. The related CompositeMLKem API remains experimental (SYSLIB5006), and its CMS Decrypt overload always throws PlatformNotSupportedException in RC2. HPKE is also experimental (SYSLIB5009) and is a separate construction. Their presence does not enable composite recipients here. Windows CNG support for ML-KEM does not enable this non-Windows CMS implementation.

EnvelopedCms with AES-CBC provides confidentiality, not authenticated message content or sender identity. Never accept arbitrary decrypted dispatch commands as trustworthy solely because decryption succeeded. Use an agreed authenticated/signed protocol and certificate policy for production. This goal does not implement a signing/trust policy, provision a PKI, or claim native/third-party interoperability, Apple proof, browser support, FIPS approval or a full protocol security audit.

## Evidence

The .NET 10 RSA envelope and .NET 11 pure ML-KEM envelope actually compile, encode, decode and decrypt original dispatch bytes. Linux checks confirm a KemRecipientInfo, reject a wrong ML-KEM key and execute the unsupported composite decryption overload to verify its PlatformNotSupportedException. Public certificates/private keys are generated ephemerally; no external account or trusted resource is required. Exact package lock, RC2 SDK, Linux image and OpenSSL 3.5.5 evidence accompany this recipe. GA reconciliation remains pending; compilation alone is not the round-trip proof.
