[CmdletBinding()]
param(
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$RecordEvidence
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','PLANETARIUM_CASE') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
$scratch = Join-Path $root ('artifacts/planetarium-' + [guid]::NewGuid().ToString('N'))
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($release in '10','11') {
    $hostDotnet = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$release.0/PlanetariumContracts/PlanetariumContracts.csproj"
    $directory = Join-Path $root "samples/net$release.0/PlanetariumContracts"
    $before = Get-EvidenceInputs $root $sample
    $output = [Collections.Generic.List[string]]::new()
    Push-Location -LiteralPath $directory
    try {
      $cases = if ($release -eq '10') { @('normal','missing') } else { @('normal','duplicate','missing','explicit','empty','null-name') }
      foreach ($case in $cases) {
        $env:PLANETARIUM_CASE = $case
        $destination = Join-Path $scratch "$release-$case"
        $arguments = @('build','PlanetariumContracts.csproj','-c','Release','-t:Rebuild','-p:RestoreLockedMode=true','-p:OpenApiGenerateDocuments=true','-p:OpenApiGenerateDocumentsOptions=--openapi-version OpenApi3_0',"-p:OpenApiDocumentsDirectory=$destination")
        $PSNativeCommandUseErrorActionPreference = $false
        $build = (& $hostDotnet @arguments 2>&1 | Out-String).Trim()
        $exit = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        $output.Add("PLANETARIUM_CASE=$case; $hostDotnet $($arguments -join ' '); exit=$exit`n$build")
        if ($case -in 'empty','null-name') {
          if ($exit -eq 0 -or $build -notmatch $(if ($case -eq 'empty') { 'Unable to find any registered documents' } else { 'NullReferenceException' })) { throw "Expected actual tool rejection for $case; exit=$exit`n$build" }
          Write-Host "PASS .NET $release build-time $case rejected by actual GetDocument."
          continue
        }
        if ($exit -ne 0) { throw "Build failed: $build" }
        $expected = if ($case -eq 'missing') { @('public') } else { @('public','staff') }
        $files = @(Get-ChildItem -LiteralPath $destination -Filter '*.json' | Sort-Object Name)
        if (($files.Name -join ',') -cne (($expected | ForEach-Object { "PlanetariumContracts_$_.json" }) -join ',')) { throw "Wrong generated files for $release/$case : $($files.Name)" }
        foreach ($name in $expected) {
          $file = Join-Path $destination "PlanetariumContracts_$name.json"
          $document = Get-Content -LiteralPath $file -Raw | ConvertFrom-Json -AsHashtable
          $title = "Planetarium $name" + $(if ($case -eq 'explicit' -and $name -eq 'public') { ' (registered)' } else { '' })
          $path = if ($name -eq 'public') { '/planetarium/shows' } else { '/planetarium/projector' }
          if ($document.info.title -cne $title -or $document.openapi -notlike '3.0.*' -or ($document.paths.Keys -join ',') -cne $path) { throw "Wrong generated options/paths: $release/$case/$name" }
          $output.Add("PASS $release/$case/$name version=$($document.openapi); title=$title; only path=$path; SHA256=$((Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant())")
        }
        $dll = Join-Path $directory "bin/Release/net$release.0/PlanetariumContracts.dll"
        $run = (& $hostDotnet $dll | Out-String).Trim()
        $output.Add("PLANETARIUM_CASE=$case; $hostDotnet $dll`n$run")
        Write-Host $run
        Write-Host "PASS .NET $release build-time $case : $($files.Name -join ',')."
      }
      if ($release -eq '11') {
        $env:PLANETARIUM_CASE = 'case'
        $run = (& $hostDotnet (Join-Path $directory 'bin/Release/net11.0/PlanetariumContracts.dll') | Out-String).Trim()
        $output.Add("PLANETARIUM_CASE=case; actual runtime provider enumeration and HTTP`n$run")
        Write-Host $run
      }
      $env:PLANETARIUM_CASE = 'normal'
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $hostDotnet -RuntimeDotnet $hostDotnet
      $compile = Get-Content -LiteralPath 'verification-evidence.json' -Raw | ConvertFrom-Json -AsHashtable
      # Preserve the exact restored graph/assets, independently from framework/runtime/compiler identities.
      $assets = Get-Content -LiteralPath 'obj/project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
      $output.Add('Resolved target assets: ' + (ConvertTo-Json $assets.targets -Depth 30 -Compress))
      $packageRoot = @($assets.packageFolders.Keys)[0]
      foreach ($library in ($assets.libraries.Keys | Sort-Object)) {
        $package = Join-Path $packageRoot ($library.ToLowerInvariant() + '/' + ($library.Replace('/','.').ToLowerInvariant()) + '.nupkg')
        $output.Add("Package $library SHA256=$((Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant())")
      }
      $sdkRoot = Split-Path $hostDotnet -Parent
      $framework = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
      $sdk = $compile.sdk
      $compiler = (& $hostDotnet (Join-Path $sdkRoot "sdk/$sdk/Roslyn/bincore/csc.dll") -version | Out-String).Trim()
      $output.Add("Compiler=$compiler; LangVersion=$($compile.language); framework=$framework; no native/workload assets required.")
      foreach ($asset in @("packs/Microsoft.AspNetCore.App.Ref/$framework/ref/net$release.0/Microsoft.AspNetCore.dll", "shared/Microsoft.AspNetCore.App/$framework/Microsoft.AspNetCore.dll")) {
        $output.Add("$asset SHA256=$((Get-FileHash -LiteralPath (Join-Path $sdkRoot $asset)).Hash.ToLowerInvariant())")
      }
      $after = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Inputs changed during verification.' }
      if ($RecordEvidence) {
        $compile.kind = 'execution'
        $compile.command = "eng/Test-Planetarium.ps1 -Dotnet '$Dotnet' -Net11Dotnet '$Net11Dotnet' -RecordEvidence"
        $compile.scope = 'Real GetDocument build-time enumeration and runtime HTTP: distinct public/staff options/endpoints; missing name; .NET 11 exact duplicates, explicit plus additional names, ordinal provider versus case-insensitive HTTP lookup, empty enumeration and null-name tool failures. Actual package closures and compiler/framework identities; no universal/custom-provider or GA claim.'
        $compile.output = $output -join "`n"
        $compile.sourceCommit = (& $git -C $root rev-parse HEAD).Trim()
        [IO.File]::WriteAllText((Join-Path $directory 'contracts.evidence.json'), (ConvertTo-Json $compile -Depth 40).Replace("`r`n","`n")+"`n")
      }
    }
    finally { Pop-Location }
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}



