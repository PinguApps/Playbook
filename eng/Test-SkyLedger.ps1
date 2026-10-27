[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [switch]$RecordEvidence
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$paths = @{}
foreach ($scope in @('Process','User','Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_ENVIRONMENT','ASPNETCORE_ENVIRONMENT')) { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('sky-ledger-' + [guid]::NewGuid().ToString('N'))
$server = $null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  New-Item -ItemType Directory -Path $temporary | Out-Null
  $toolDirectory = Join-Path $root 'artifacts/sky-ledger/tools'
  $tool = Join-Path $toolDirectory $(if ($IsWindows) { 'nswag.exe' } else { 'nswag' })
  if (-not (Test-Path -LiteralPath $tool)) {
    & $Dotnet tool install NSwag.ConsoleCore --version 14.7.1 --tool-path $toolDirectory | Out-Host
  }
  if ((& $tool version | Out-String) -notmatch '14\.7\.1') { throw 'Expected NSwag 14.7.1.' }
  foreach ($release in @('10','11')) {
    $hostDotnet = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$release.0/SkyLedger/SkyLedger.csproj"
    $directory = Join-Path $root "samples/net$release.0/SkyLedger"
    $before = Get-EvidenceInputs $root $sample
    $output = @()
    Push-Location $directory
    try {
      foreach ($environment in @('Development','Production')) {
        $destination = Join-Path $temporary "$release-$environment"
        $arguments = @('build','SkyLedger.csproj','-c','Release','-t:Rebuild','-p:RestoreLockedMode=true','-p:OpenApiGenerateDocuments=true',"-p:OpenApiDocumentsDirectory=$destination")
        # The older package has no OpenApiGenerationEnvironment property.
        $env:DOTNET_ENVIRONMENT = if ($release -eq '10') { $environment } else { 'Production' }
        $env:ASPNETCORE_ENVIRONMENT = $env:DOTNET_ENVIRONMENT
        if ($release -eq '11') { $arguments += "-p:OpenApiGenerationEnvironment=$environment" }
        $build = (& $hostDotnet @arguments | Out-String).Trim()
        $output += "$hostDotnet $($arguments -join ' ')`n$build"
        $contract = Get-Content -LiteralPath (Join-Path $destination 'SkyLedger_environment.json') -Raw | ConvertFrom-Json -AsHashtable
        if ($contract.info.title -cne "Sky ledger: $environment" -or $contract.paths.Contains('/observatory/calibration') -ne ($environment -eq 'Development')) { throw "Wrong $release/$environment endpoints or transformer." }
        Write-Host "PASS .NET $release build-time ${environment}: endpoint inclusion and transformer title."
        $output += "PASS .NET $release build-time ${environment}: calibration present=$($environment -eq 'Development'); title=$($contract.info.title)."
        $output += @(Get-ChildItem -LiteralPath $destination -Filter '*.json' | Sort-Object Name | ForEach-Object { "$environment/$($_.Name) SHA256=$((Get-FileHash -LiteralPath $_.FullName).Hash.ToLowerInvariant())" })
      }
      # Normal runtime checks still execute in Production, independently of the build property.
      $start = [Diagnostics.ProcessStartInfo]::new($hostDotnet)
      $start.ArgumentList.Add((Join-Path $directory "bin/Release/net$release.0/SkyLedger.dll"))
      $start.UseShellExecute = $false
      $start.CreateNoWindow = $true
      $start.RedirectStandardOutput = $true
      $start.RedirectStandardError = $true
      $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
      $start.Environment['SKY_LEDGER_SERVE'] = '1'
      $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
      $start.Environment['ASPNETCORE_ENVIRONMENT'] = 'Production'
      $server = [Diagnostics.Process]::Start($start)
      $errors = $server.StandardError.ReadToEndAsync()
      $line = $server.StandardOutput.ReadLineAsync().WaitAsync([TimeSpan]::FromSeconds(45)).GetAwaiter().GetResult()
      if ($line -notmatch '^SKY_ADDRESS=(http://127\.0\.0\.1:\d+)$') { throw "Server did not pass its checks/start: $line" }
      $address = $Matches[1]
      $consumer = Join-Path $temporary "consumer-$release"
      New-Item -ItemType Directory -Path $consumer | Out-Null
      $document = Join-Path $temporary "$release-Production/SkyLedger_client.json"
      & $tool openapi2csclient "/Input:$document" "/Output:$consumer/Client.cs" /ClassName:SkyGenerated /Namespace:SkyConsumer /JsonLibrary:SystemTextJson /UseBaseUrl:false /OperationGenerationMode:SingleClientFromOperationId | Out-Host
      Copy-Item -LiteralPath (Join-Path $root 'samples/net11.0/SkyLedger/GeneratedConsumer.cs.txt') -Destination (Join-Path $consumer 'Program.cs')
      [IO.File]::WriteAllText((Join-Path $consumer 'Consumer.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>disable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
      [IO.File]::WriteAllText((Join-Path $consumer 'global.json'), '{"sdk":{"version":"10.0.402","rollForward":"disable"}}')
      Push-Location $consumer
      try { $result = (& $Dotnet run -c Release -- $address | Out-String).Trim(); Write-Host $result }
      finally { Pop-Location }
      $output += "NSwag 14.7.1 openapi2csclient: $document; independent net10.0 Consumer.csproj; $Dotnet run -c Release -- $address`n$result"
      $output += "Generated Client.cs SHA256=$((Get-FileHash -LiteralPath (Join-Path $consumer 'Client.cs')).Hash.ToLowerInvariant())"
      $server.Kill($true); $server.WaitForExit(); $server.Dispose(); $server = $null
      $after = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Inputs changed during check.' }
      if ($RecordEvidence) {
        $manifest = Get-Content -LiteralPath verification.json -Raw | ConvertFrom-Json
        $record = [ordered]@{
          kind='execution'; sample=$sample; target="net$release.0"; sdk=if ($release -eq '10') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }; language=if ($release -eq '10') { '14.0' } else { '15.0' }
          command="eng/Test-SkyLedger.ps1 -Dotnet '$Dotnet' -Net11Dotnet '$Net11Dotnet' -RecordEvidence"
          scope='Real build-time documents in Development/Production; .NET 10 process environment versus .NET 11 OpenApiGenerationEnvironment; endpoint and transformer differences; NSwag 14.7.1 generated typed client from selected OpenAPI 3.0, independently compiled on .NET 10 and executed against this actual server. No SSE client-generator or OpenAPI 3.2 generator support claim.'
          recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& $git -C $root rev-parse HEAD).Trim(); sourceDirty=[bool](& $git -C $root status --porcelain -- samples eng/Test-SkyLedger.ps1)
          inputs=$after; dependencies=@($manifest.dependencies); workloads=@(); nativeToolchains=@()
          host=[Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
          sdkInfo=(& $hostDotnet --info | Out-String).Trim(); workloadInfo=(& $hostDotnet workload list | Out-String).Trim()
          output=$output -join "`n"
        }
        [IO.File]::WriteAllText((Join-Path $directory 'contracts.evidence.json'), (ConvertTo-Json $record -Depth 40).Replace("`r`n","`n")+"`n")
      }
    } finally { Pop-Location }
  }
}
finally {
  if ($server -and -not $server.HasExited) { $server.Kill($true); $server.WaitForExit(); $server.Dispose() }
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  $resolved = [IO.Path]::GetFullPath($temporary)
  if ((Split-Path $resolved -Parent) -cne [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) -or (Split-Path $resolved -Leaf) -notmatch '^sky-ledger-[a-f0-9]{32}$') { throw 'Unsafe temporary cleanup path.' }
  if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
