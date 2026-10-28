[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Chromium,
    [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
    [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
    [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $records = @()
    foreach ($version in '10.0','11.0') {
        $dotnet = if ($version -eq '10.0') { $Net10Dotnet } else { $Net11Dotnet }
        foreach ($name in 'WetlandPermits','AstronomyBadges') {
            $sample = "samples/net$version/$name/$name.csproj"
            & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
            Push-Location -LiteralPath (Split-Path (Join-Path $root $sample))
            try { & $dotnet publish "$name.csproj" -c Release -p:RestoreLockedMode=true }
            finally { Pop-Location }
            $records += Get-Content -LiteralPath (Join-Path (Split-Path (Join-Path $root $sample)) 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
        }
    }
    $output = Join-Path $root 'artifacts/authentication/browser.json'
    & $Node (Join-Path $PSScriptRoot 'Test-WasmAuthentication.mjs') $Chromium $output
    $report = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
    foreach ($record in $records) {
        $dotnet = if ($record.target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
        $name = [IO.Path]::GetFileNameWithoutExtension($record.sample)
        $results = @($report.results | Where-Object { "net$($_.version)" -eq $record.target -and $_.sample -eq $name })
        if ($results.Count -ne $(if ($name -eq 'AstronomyBadges') { 1 } elseif ($record.target -eq 'net10.0') { 5 } else { 4 })) { throw 'Incomplete real behaviour results.' }
        $after = Get-EvidenceInputs $root $record.sample
        if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
        $record.kind = 'execution'
        $record.command += "; & '$dotnet' publish -c Release -p:RestoreLockedMode=true; & '$Node' '$PSScriptRoot/Test-WasmAuthentication.mjs' '$Chromium' '$output'"
        $record.scope = if ($name -eq 'WetlandPermits') {
            'Actual published WASM and loopback oidc-provider code/PKCE/signing/session/end-session protocol; legacy/current navigation, valid/invalid/missing state, callback, public return and fresh provider interaction after logout. No external-provider or production-security proof.'
        } else { 'Actual WASM, real DI/options/logging/service aliases and account mapping; controlled local JS success/redirect token paths. No external login or real token issuance.' }
        $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
        $record.output = "Chromium $($report.browser); Node $($report.node); $($report.identityProvider)`n" + ($results | ConvertTo-Json -Depth 30)
        $record | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath (Join-Path (Split-Path (Join-Path $root $record.sample)) 'behavior.evidence.json') -Encoding utf8
    }
    Write-Host 'PASS: four unchanged-input real browser execution records saved.'
}
finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
