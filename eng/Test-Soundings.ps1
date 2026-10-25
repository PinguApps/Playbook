[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Output = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin814/execution.json')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$before = @{}
foreach ($scope in @('Process','User','Machine')) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$session = Join-Path ([IO.Path]::GetTempPath()) ('soundings-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $session | Out-Null
$records = @()
$samples = @()
try {
  foreach ($major in @(10,11)) {
    $sample = "samples/net$major.0/Soundings/Soundings.csproj"
    $directory = Split-Path (Join-Path $root $sample)
    Push-Location -LiteralPath $directory
    try {
      $build = (& $Dotnet build ./Soundings.csproj -c Release -p:RestoreLockedMode=true | Out-String)
      $dll = Join-Path $directory "bin/Release/net$major.0/Soundings.dll"
      $samples += @{ sample=$sample; sdk=(& $Dotnet --version | Out-String).Trim(); build=$build; assemblySha256=(Get-FileHash $dll).Hash.ToLowerInvariant(); inputs=(Get-EvidenceInputs $root $sample) }
    } finally { Pop-Location }
  }
  foreach ($server in @(10,11)) {
    $addressFile = Join-Path $session "server-$server.txt"
    $info = [Diagnostics.ProcessStartInfo]::new($Dotnet)
    $info.WorkingDirectory = $session
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    foreach ($argument in @((Join-Path $root "samples/net$server.0/Soundings/bin/Release/net$server.0/Soundings.dll"),'--serve',$addressFile)) { $info.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($info)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    try {
      $deadline = [DateTimeOffset]::UtcNow.AddSeconds(20)
      while (-not (Test-Path -LiteralPath $addressFile)) {
        if ($process.HasExited) { throw "Server $server exited: $($stderr.GetAwaiter().GetResult())" }
        if ([DateTimeOffset]::UtcNow -gt $deadline) { throw "Server $server did not become ready in 20 seconds." }
        Start-Sleep -Milliseconds 100
      }
      $address = Get-Content -LiteralPath $addressFile -Raw
      foreach ($client in @(10,11)) {
        $dll = Join-Path $root "samples/net$client.0/Soundings/bin/Release/net$client.0/Soundings.dll"
        Push-Location -LiteralPath $session
        try { $result = (& $Dotnet $dll --client $address "$server" | Out-String) } finally { Pop-Location }
        if (-not $result.Contains('PASS:')) { throw "Missing client $client/server $server result." }
        $records += @{ client=$client; server=$server; output=$result; workingDirectory=$session }
        Write-Host $result
      }
    } finally {
      if (-not $process.HasExited) { $process.Kill($true); $process.WaitForExit() }
      $serverOutput = $stdout.GetAwaiter().GetResult()
      $serverError = $stderr.GetAwaiter().GetResult()
      $records += @{ server=$server; serverOutput=$serverOutput; serverError=$serverError }
      $process.Dispose()
    }
  }
  foreach ($sample in $samples) {
    $current = Get-EvidenceInputs $root $sample.sample
    if (($current | ConvertTo-Json -Compress) -cne ($sample.inputs | ConvertTo-Json -Compress)) { throw 'Sample inputs changed during execution.' }
  }
  New-Item -ItemType Directory -Path (Split-Path $Output) -Force | Out-Null
  @{ kind='real-dotnet-client-and-kestrel-websocket-execution'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); samples=$samples; records=$records; scope='Real non-streaming InvokeAsync over loopback WebSockets, both matched and both mixed .NET 10/11 pairs. Client cancellation, server state, connection reuse, normal completion and disconnect fallback. Held work and HTTP observation are local test instrumentation, not mocks. No browser/Java/Swift, MessagePack, service provider, external resource, platform matrix or GA claim.' } | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $Output
  Write-Host 'PASS: both matching pairs and both mixed-version capability controls.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
