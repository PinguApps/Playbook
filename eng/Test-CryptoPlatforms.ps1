[CmdletBinding()]
param(
    [string]$Docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe',
    [string]$Image = 'mcr.microsoft.com/dotnet/sdk@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1',
    [string]$Packages = (Join-Path $env:USERPROFILE '.nuget/packages')
)

# Bounded authoring check; uses the existing native-capability image, not a CI matrix.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$results = Join-Path $root 'artifacts/crypto-platforms'
New-Item -ItemType Directory -Path $results -Force | Out-Null
foreach ($name in @('KeyMaterial', 'PostQuantum')) {
    $sample = "samples/net11.0/$name/$name.csproj"
    $before = Get-EvidenceInputs $root $sample
    $program = @'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. /source/eng/Evidence.ps1
$name = $env:CRYPTO_SAMPLE
$sample = "samples/net11.0/$name/$name.csproj"
$inputs = Get-EvidenceInputs /source $sample
foreach ($relative in $inputs.Keys) {
    $destination = Join-Path /work $relative
    New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path /source $relative) -Destination $destination
}
Set-Location "/work/samples/net11.0/$name"
$native = (& /usr/bin/openssl version | Out-String).Trim()
if ($native -notmatch '^OpenSSL 3\.5\.5 ') { throw "Unexpected OpenSSL: $native" }
$sdk = (& /usr/share/dotnet/dotnet --version | Out-String).Trim()
if ($sdk -cne '11.0.100-rc.2.26504.105') { throw "Unexpected SDK: $sdk" }
$sdkInfo = (& /usr/share/dotnet/dotnet --info | Out-String).Trim()
$workloads = (& /usr/share/dotnet/dotnet workload list | Out-String).Trim()
$output = (& /usr/share/dotnet/dotnet build "$name.csproj" --configuration Release -p:RestoreLockedMode=true -p:NuGetAudit=false | Out-String).Trim()
$output += "`n" + (& /usr/share/dotnet/dotnet "./bin/Release/net11.0/$name.dll" | Out-String).Trim()
$output += "`n$native"
@{ sdkInfo = $sdkInfo; workloadInfo = $workloads; output = $output; host = [Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture } |
    ConvertTo-Json -Depth 5 | Set-Content "/results/$name.json"
Write-Host $output
'@
    & $Docker run --rm --init --network none -e DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 `
        -e "CRYPTO_SAMPLE=$name" -e NUGET_PACKAGES=/packages `
        --mount "type=bind,source=$root,target=/source,readonly" --mount "type=bind,source=$results,target=/results" `
        --mount "type=bind,source=$Packages,target=/packages,readonly" $Image /usr/bin/pwsh -NoProfile -Command $program
    $actual = Get-Content (Join-Path $results "$name.json") -Raw | ConvertFrom-Json
    $after = Get-EvidenceInputs $root $sample
    if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Crypto inputs changed during verification.' }
    $manifest = Get-Content (Join-Path $root "samples/net11.0/$name/verification.json") -Raw | ConvertFrom-Json
    $record = [ordered]@{
        kind = 'compile-and-run'; sample = $sample; target = 'net11.0'; sdk = '11.0.100-rc.2.26504.105'; language = '15.0'
        command = "eng/Test-CryptoPlatforms.ps1 -Docker '$Docker' -Image '$Image'; container: /usr/share/dotnet/dotnet build $name.csproj --configuration Release -p:RestoreLockedMode=true -p:NuGetAudit=false; /usr/share/dotnet/dotnet ./bin/Release/net11.0/$name.dll"
        scope = 'Actual Linux x64/OpenSSL crypto operations in a network-disabled authoring container, built from unchanged inputs with locked packages. No mocks, Windows CMS, Apple or GA proof.'
        recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); sourceCommit = (& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
        sourceDirty = [bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain -- samples eng/Test-CryptoPlatforms.ps1)
        inputs = $after; dependencies = @($manifest.dependencies); workloads = @(); nativeToolchains = @($manifest.nativeToolchains)
        host = $actual.host; sdkInfo = $actual.sdkInfo; workloadInfo = $actual.workloadInfo; output = $actual.output
    }
    [IO.File]::WriteAllText((Join-Path $root "samples/net11.0/$name/verification-evidence.json"), (ConvertTo-Json $record -Depth 30).Replace("`r`n", "`n") + "`n")
}
