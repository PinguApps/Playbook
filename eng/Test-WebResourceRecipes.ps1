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
$scratch = Join-Path $root ('artifacts/web-resources-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $scratch | Out-Null
$audit = Get-Content (Join-Path $root 'docs/planning/audits/web.assets.json') -Raw | ConvertFrom-Json
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($release in '10','11') {
    $hostDotnet = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
    $installation = Split-Path $hostDotnet -Parent
    $baseline = $audit.baselines | Where-Object target -CEQ "net$release.0"
    foreach ($name in 'EstuaryBudgets','GlasshousePatch','StrataTests') {
      $sample = "samples/net$release.0/$name/$name.csproj"
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
          if ($assets.libraries[$library].type -eq 'project') { continue }
          $identity = $library.Split('/',2)
          $packageDirectory = Join-Path $packageRoot $library.ToLowerInvariant()
          $package = Join-Path $packageDirectory ($library.Replace('/','.').ToLowerInvariant() + '.nupkg')
          $hash = (Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant()
          $audited = @($baseline.packages | Where-Object { "$($_.id)/$($_.version)" -ieq $library })
          if ($audited.Count -eq 1) {
            if ($hash -cne $audited[0].sha256) { throw "Audited package mismatch: $library" }
            foreach ($asset in $audited[0].assets) {
              if ((Get-FileHash -LiteralPath (Join-Path $packageDirectory $asset.path)).Hash.ToLowerInvariant() -cne $asset.surface.sha256) { throw "Audited compile asset mismatch: $library" }
            }
          } elseif ($identity[0] -ne 'Microsoft.Extensions.DependencyModel' -or $identity[1] -cne $baseline.referencePack) {
            throw "Unqualified additional package: $library"
          }
          # DependencyModel is the matching Mvc.Testing supporting closure, qualified
          # by actual entrypoint/deps discovery, startup and TestServer requests here.
          [xml]$nuspec = Get-Content (Join-Path $packageDirectory ($identity[0].ToLowerInvariant() + '.nuspec')) -Raw
          $observations.Add("PACKAGE $library SHA256=$hash auditMatch=$($audited.Count -eq 1) repository=$($nuspec.package.metadata.repository | ConvertTo-Json -Compress)")
        }
        foreach ($assembly in 'Microsoft.AspNetCore.Server.Kestrel.Transport.Quic.dll','Microsoft.AspNetCore.dll') {
          $reference = $baseline.framework | Where-Object name -CEQ $assembly
          $path = Join-Path $installation $reference.path
          $hash = (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()
          if ($hash -cne $reference.sha256) { throw "Audited framework mismatch: $assembly" }
          $observations.Add("AUDIT-MATCH $path SHA256=$hash")
          $runtime = Join-Path $installation "shared/Microsoft.AspNetCore.App/$($baseline.referencePack)/$assembly"
          $observations.Add("RUNTIME $runtime SHA256=$((Get-FileHash -LiteralPath $runtime).Hash.ToLowerInvariant())")
        }
        foreach ($relative in @("sdk/$($baseline.sdk)/Roslyn/bincore/csc.dll", "dotnet.exe", "shared/Microsoft.NETCore.App/$($baseline.referencePack)/coreclr.dll")) {
          $path = Join-Path $installation $relative
          $observations.Add("ARTIFACT $path VERSION=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) SHA256=$((Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant())")
        }
        if ($name -eq 'EstuaryBudgets') {
          $native = Join-Path $installation "shared/Microsoft.NETCore.App/$($baseline.referencePack)/msquic.dll"
          if ([Diagnostics.FileVersionInfo]::GetVersionInfo($native).FileVersion -cne '2.5.11.159415436') { throw 'Requalify actual MsQuic version before recording.' }
        }
        # Diagnostic controls use complete independent source copies, never mutate
        # a canonical source or its saved freshness inputs during a build.
        if (($name -eq 'EstuaryBudgets' -and $release -eq '10') -or ($name -eq 'GlasshousePatch' -and $release -eq '11')) {
          $copy = Join-Path $scratch "$name-$release"
          New-Item -ItemType Directory -Path $copy | Out-Null
          Get-ChildItem -LiteralPath $directory -File | Where-Object Name -NotLike '*evidence.json' | Copy-Item -Destination $copy
          $source = Join-Path $copy 'Program.cs'
          $text = [IO.File]::ReadAllText($source)
          if ($name -eq 'EstuaryBudgets') {
            $text = $text.Replace('#pragma warning disable CA2252', '#pragma warning restore CA2252')
            $diagnostic = 'CA2252'
          } else {
            $text = $text.Replace('operation.OperationType is OperationType.Move or OperationType.Copy', 'operation.ShouldSerializeFrom()')
            $diagnostic = 'CS0618'
          }
          [IO.File]::WriteAllText($source,$text)
          Push-Location $copy
          try {
            $PSNativeCommandUseErrorActionPreference = $false
            $probe = (& $hostDotnet build -c Release -p:TreatWarningsAsErrors=false 2>&1 | Out-String).Trim()
            $exit = $LASTEXITCODE
            $PSNativeCommandUseErrorActionPreference = $true
            if ($exit -ne 0 -or $probe -notmatch "warning $diagnostic") { throw "Expected actual default diagnostic $diagnostic : $probe" }
            $observations.Add("PROBE $hostDotnet build -c Release -p:TreatWarningsAsErrors=false; exit=$exit`n$probe")
            Write-Host "PASS actual $diagnostic compiler/analyzer control."
          } finally { Pop-Location }
        }
        if ($RecordEvidence) {
          [string[]]$nativeVersions = @()
          if ($name -eq 'EstuaryBudgets') { $nativeVersions += 'MsQuic/2.5.11.159415436' }
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $hostDotnet -RuntimeDotnet $hostDotnet -NativeToolchains $nativeVersions
          $record = Get-Content 'verification-evidence.json' -Raw | ConvertFrom-Json -AsHashtable
          $record.output += "`n" + ($observations -join "`n")
          $record.command += '; additional exact asset qualification and isolated diagnostic controls: eng/Test-WebResourceRecipes.ps1 -RecordEvidence'
          [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'), (ConvertTo-Json $record -Depth 50).Replace("`r`n","`n") + "`n")
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
Write-Host 'PASS six actual web-resource consumers, locked assets, diagnostic controls and unchanged PATH scopes.'
