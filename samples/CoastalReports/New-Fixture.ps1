# Generate only disposable synthetic archive input on a DSA-supporting Windows host.
# This is not macOS setup, production key migration or a new signing recommendation.
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'This fixture generator is scoped to Windows.' }
$key = [Security.Cryptography.DSA]::Create(1024)
try {
  $payload = [Text.Encoding]::UTF8.GetBytes("survey=Gannet Shoal;depth-mm=14320;datum=chart;revision=0`n")
  $signature = $key.SignData($payload, [Security.Cryptography.HashAlgorithmName]::SHA1,
    [Security.Cryptography.DSASignatureFormat]::IeeeP1363FixedFieldConcatenation)
  $fixture = [ordered]@{
    purpose = 'Synthetic public-only DSA1024/SHA1/P1363 archive fixture; never a production credential'
    generatedWith = [Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
    generatedOn = [Runtime.InteropServices.RuntimeInformation]::OSDescription
    payload = [Convert]::ToBase64String($payload)
    publicKeySpki = [Convert]::ToBase64String($key.ExportSubjectPublicKeyInfo())
    signature = [Convert]::ToBase64String($signature)
  }
  [IO.File]::WriteAllText((Join-Path $PSScriptRoot 'legacy-fixture.json'), ($fixture | ConvertTo-Json).Replace("`r`n","`n") + "`n")
}
finally { $key.Dispose() }
