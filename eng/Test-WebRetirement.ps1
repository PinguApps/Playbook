[CmdletBinding()]
param(
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$RecordEvidence
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$scratch = Join-Path $root ('artifacts/web-retirement-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $scratch | Out-Null
$audit = Get-Content (Join-Path $root 'docs/planning/audits/web.assets.json') -Raw | ConvertFrom-Json
function Copy-Consumer([string]$Source, [string]$Destination) {
  New-Item -ItemType Directory -Force -Path $Destination | Out-Null
  Get-ChildItem -LiteralPath $Source | Where-Object { $_.Name -notin 'bin','obj','verification-evidence.json' } | Copy-Item -Destination $Destination -Recurse
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($release in '10','11') {
    $hostDotnet = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
    $installation = Split-Path $hostDotnet -Parent
    $baseline = $audit.baselines | Where-Object target -CEQ "net$release.0"
    foreach ($name in 'NightArchive','StarSurvey') {
      $extension = if ($name -eq 'StarSurvey') { 'fsproj' } else { 'csproj' }
      $sample = "samples/net$release.0/$name/$name.$extension"
      $directory = Join-Path $root (Split-Path $sample)
      $observations = [Collections.Generic.List[string]]::new()
      Push-Location $directory
      try {
        & $hostDotnet restore --locked-mode
        $assets = Get-Content 'obj/project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
        $manifest = Get-Content 'verification.json' -Raw | ConvertFrom-Json -AsHashtable
        $resolved = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
        if ((ConvertTo-Json -InputObject $resolved -Compress) -cne (ConvertTo-Json -InputObject @($manifest.dependencies) -Compress)) { throw "Unexpected graph: $sample" }
        $observations.Add('RESOLVED-ASSETS ' + (ConvertTo-Json $assets.targets -Depth 50 -Compress))
        $packageRoot = @($assets.packageFolders.Keys)[0]
        foreach ($library in $resolved) {
          $identity = $library.Split('/',2)
          $packageDirectory = Join-Path $packageRoot $library.ToLowerInvariant()
          $package = Join-Path $packageDirectory ($library.Replace('/','.').ToLowerInvariant() + '.nupkg')
          $hash = (Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant()
          $audited = @($baseline.packages | Where-Object { "$($_.id)/$($_.version)" -ieq $library })
          if ($audited.Count -eq 1) {
            if ($hash -cne $audited[0].sha256) { throw "Audited package mismatch: $library" }
            foreach ($asset in $audited[0].assets) {
              if ((Get-FileHash -LiteralPath (Join-Path $packageDirectory $asset.path)).Hash.ToLowerInvariant() -cne $asset.surface.sha256) { throw "Audited asset mismatch: $library" }
            }
          } elseif ($library -notin @(
            'FSharp.Core/10.1.402','FSharp.Core/11.0.101-rc2.26504.105',
            'Microsoft.AspNetCore.Mvc.Razor.Extensions/6.0.0','Microsoft.AspNetCore.Razor.Language/6.0.0',
            'Microsoft.CodeAnalysis.Analyzers/3.3.2','Microsoft.CodeAnalysis.Common/4.0.0',
            'Microsoft.CodeAnalysis.CSharp/4.0.0','Microsoft.CodeAnalysis.Razor/6.0.0',
            'Microsoft.Extensions.DependencyModel/10.0.13')) { throw "Unqualified additional package: $library" }
          # These additional exact assets form the F# core/runtime Razor compiler closure.
          # Real view compilation/rendering below qualifies this consumer, not arbitrary older-target support.
          $observations.Add("PACKAGE $library SHA256=$hash auditMatch=$($audited.Count -eq 1)")
        }
        foreach ($assembly in 'Microsoft.AspNetCore.Http.Features.dll','Microsoft.AspNetCore.Mvc.Razor.dll','Microsoft.AspNetCore.Mvc.Core.dll') {
          $reference = $baseline.framework | Where-Object name -CEQ $assembly
          $path = Join-Path $installation $reference.path
          $hash = (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()
          if ($hash -cne $reference.sha256) { throw "Audited framework mismatch: $assembly" }
          $observations.Add("AUDIT-MATCH $path SHA256=$hash")
          $runtime = Join-Path $installation "shared/Microsoft.AspNetCore.App/$($baseline.referencePack)/$assembly"
          $observations.Add("RUNTIME $runtime VERSION=$([Diagnostics.FileVersionInfo]::GetVersionInfo($runtime).ProductVersion) SHA256=$((Get-FileHash -LiteralPath $runtime).Hash.ToLowerInvariant())")
        }
        foreach ($relative in @("sdk/$($baseline.sdk)/Roslyn/bincore/csc.dll", "sdk/$($baseline.sdk)/FSharp/fsc.dll",
            "dotnet.exe", "shared/Microsoft.NETCore.App/$($baseline.referencePack)/coreclr.dll",
            "sdk/$($baseline.sdk)/Sdks/Microsoft.NET.Sdk.Razor/targets/Microsoft.NET.Sdk.Razor.Compilation.targets")) {
          $path = Join-Path $installation $relative
          $observations.Add("ARTIFACT $path VERSION=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) SHA256=$((Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant())")
        }
        if ($name -eq 'StarSurvey') {
          $templatePath = Join-Path $installation "templates/$($baseline.referencePack)/$($baseline.templatePackage.file)"
          if ((Get-FileHash -LiteralPath $templatePath).Hash.ToLowerInvariant() -cne $baseline.templatePackage.sha256) { throw 'Template package differs from source audit.' }
          $archive = [IO.Compression.ZipFile]::OpenRead($templatePath)
          try {
            foreach ($entry in $baseline.templatePackage.files | Where-Object path -Match '^content/StarterWeb-FSharp/.*\.(fs|fsproj)$') {
              $stream = $archive.GetEntry($entry.path).Open()
              try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() } finally { $stream.Dispose() }
              if ($hash -cne $entry.sha256) { throw "Template entry mismatch: $($entry.path)" }
              $observations.Add("AUDIT-TEMPLATE $($entry.path) SHA256=$hash")
            }
          } finally { $archive.Dispose() }
          if ($release -eq '11') {
            $publish = Join-Path $scratch 'published'
            $publishLog = (& $hostDotnet publish -c Release --no-restore -o $publish | Out-String).Trim()
            if (-not (Test-Path (Join-Path $publish 'StarSurvey.Views.dll')) -or
                @(Get-ChildItem $publish -Recurse -Filter '*.cshtml').Count -ne 0 -or
                @(Get-ChildItem $publish -Recurse -Filter '*RuntimeCompilation*').Count -ne 0) { throw 'Published consumer is not source-free/package-free with a compiled view assembly.' }
            Push-Location $publish
            try { $runLog = (& $hostDotnet './StarSurvey.dll' | Out-String).Trim() } finally { Pop-Location }
            $observations.Add("COMMAND $hostDotnet publish -c Release --no-restore -o $publish\n$publishLog\nCOMMAND cwd=$publish; $hostDotnet ./StarSurvey.dll\n$runLog")
            $observations.Add("PUBLISHED-VIEWS SHA256=$((Get-FileHash (Join-Path $publish 'StarSurvey.Views.dll')).Hash.ToLowerInvariant()); no cshtml; no RuntimeCompilation binaries")
            # A fresh independent copy prevents stale .Views.dll from masking the broken migration.
            $negative = Join-Path $scratch 'missing-views'
            Copy-Consumer $directory $negative
            $project = Join-Path $negative 'StarSurvey.fsproj'
            $text = [IO.File]::ReadAllText($project).Replace('<RazorCompileOnBuild>true','<RazorCompileOnBuild>false').Replace('<RazorCompileOnPublish>true','<RazorCompileOnPublish>false')
            [IO.File]::WriteAllText($project,$text)
            Push-Location $negative
            try {
              $negativePublish = Join-Path $scratch 'missing-publish'
              $log = (& $hostDotnet publish -c Release -p:RestoreLockedMode=true -o $negativePublish | Out-String).Trim()
              if (Test-Path (Join-Path $negativePublish 'StarSurvey.Views.dll')) { throw 'Negative control contains stale compiled views.' }
              Push-Location $negativePublish
              try { $negativeRun = (& $hostDotnet './StarSurvey.dll' --expect-missing | Out-String).Trim() } finally { Pop-Location }
              $observations.Add("NEGATIVE $hostDotnet publish -c Release -p:RestoreLockedMode=true -o $negativePublish; cwd=$negativePublish; $hostDotnet ./StarSurvey.dll --expect-missing\n$log\n$negativeRun")
            } finally { Pop-Location }
            Write-Host 'PASS actual clean published F# view, same load context and build-success/missing-view negative control.'
          }
        } elseif ($release -eq '11') {
          $probeDirectory = Join-Path $scratch 'obsolete-token-binding'
          Copy-Consumer $directory $probeDirectory
          Copy-Item (Join-Path $root 'samples/net10.0/NightArchive/Program.cs') (Join-Path $probeDirectory 'Program.cs')
          Push-Location $probeDirectory
          try {
            $PSNativeCommandUseErrorActionPreference = $false
            $log = (& $hostDotnet build -c Release -p:TreatWarningsAsErrors=false 2>&1 | Out-String).Trim()
            $exitCode = $LASTEXITCODE
            $PSNativeCommandUseErrorActionPreference = $true
            if ($exitCode -ne 0 -or $log -notmatch 'warning ASPDEPR010') { throw "Expected actual ASPDEPR010: $log" }
            $observations.Add("DIAGNOSTIC $hostDotnet build -c Release -p:TreatWarningsAsErrors=false; historical Program.cs; exit=$exitCode\n$log")
          } finally { Pop-Location }
          Write-Host 'PASS actual ASPDEPR010 from the .NET 11 compiler/reference pair.'
        }
        if ($RecordEvidence) {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $hostDotnet
          $recordPath = Join-Path $directory 'verification-evidence.json'
          $record = Get-Content $recordPath -Raw | ConvertFrom-Json -AsHashtable
          $record.output += "`n" + ($observations -join "`n")
          $record.command += '; exact-asset, diagnostic and clean published-consumer controls: eng/Test-WebRetirement.ps1 -RecordEvidence'
          [IO.File]::WriteAllText($recordPath,(ConvertTo-Json $record -Depth 50).Replace("`r`n","`n") + "`n")
        } else {
          & $hostDotnet build -c Release --no-restore
          & $hostDotnet "bin/Release/net$release.0/$name.dll"
        }
      } finally { Pop-Location }
    }
  }
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
Write-Host 'PASS four actual consumers, audited/locked assets, diagnostic and published-view controls; PATH unchanged.'
