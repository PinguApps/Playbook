[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{}; foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$previous=@{}; foreach($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER','UseSharedCompilation','DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE','DOTNET_CLI_TELEMETRY_OPTOUT'){$previous[$key]=[Environment]::GetEnvironmentVariable($key)}
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; $env:MSBUILDUSESERVER='0'; $env:UseSharedCompilation='false'; $env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
 foreach($source in (Get-Content samples/SupportPaths/sources.json -Raw|ConvertFrom-Json).sources){$bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray();if([Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) -cne $source.sha256){throw 'Source drift'}}
 foreach($major in 10,11){
  $dn=if($major -eq 10){(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source}else{(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source}
  foreach($name in 'ReedStation','ReedStationBrowser'){
   $sample="samples/net$major.0/$name/$name.csproj"
   if($Record){$kind=if($name -eq 'ReedStation'){'compile-and-run'}else{'compile'}; & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dn -Kind $kind}
   if(!$Record -and $name -eq 'ReedStation'){Push-Location (Split-Path $sample);try{& $dn run -c Release -p:RestoreLockedMode=true}finally{Pop-Location}}
   $audit=Get-Content docs/planning/audits/runtime-security.assets.json -Raw|ConvertFrom-Json
   $baseline=@($audit.baselines|Where-Object target -CEQ "net$major.0")[0]
   $native=Join-Path ([Environment]::SystemDirectory) 'iphlpapi.dll'
   $identities=@("NATIVE $native sha256=$((Get-FileHash $native).Hash)")
   foreach($assembly in 'System.Runtime.dll','System.Net.NetworkInformation.dll'){
    $path=Join-Path (Split-Path $dn) "packs/Microsoft.NETCore.App.Ref/$($baseline.referencePack)/ref/net$major.0/$assembly"
    $hash=(Get-FileHash $path).Hash.ToLowerInvariant()
    if($hash -cne @($baseline.framework|Where-Object name -CEQ $assembly)[0].sha256){throw "Audit reference drift: $path"}
    $identities+="ARTIFACT $path sha256=$hash"
   }
   $sdk=(Get-Content (Join-Path (Split-Path $sample) 'global.json') -Raw|ConvertFrom-Json).sdk.version
   $compiler=Join-Path (Split-Path $dn) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
   $identities+="COMPILER $compiler sha256=$((Get-FileHash $compiler).Hash)"
   if($Record){$p=Join-Path (Split-Path $sample) 'verification-evidence.json';$receipt=Get-Content $p -Raw|ConvertFrom-Json -AsHashtable;$receipt.output+="`n"+($identities -join "`n");[IO.File]::WriteAllText((Join-Path $root $p),($receipt|ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")}
   if($name -eq 'ReedStationBrowser'){Push-Location (Split-Path $sample);try{& $dn publish -c Release --no-restore -p:PublishTrimmed=false}finally{Pop-Location}}
  }
 }
 $cleanup=(& "$PSScriptRoot/Test-SupportPathsCleanup.ps1" | Out-String)
 Write-Host $cleanup
 $network=(& "$PSScriptRoot/Test-SupportPaths.ps1" | Out-String)
 Write-Host $network
 & 'C:/Program Files/nodejs/node.exe' "$PSScriptRoot/Test-SupportPaths.mjs" 'C:/Users/pingu/AppData/Local/npm-cache/_npx/e41f203b7505f1fb/node_modules/playwright-core' 'C:/Users/pingu/AppData/Local/ms-playwright/chromium-1248/chrome-win64/chrome.exe'
 if($Record){foreach($major in 10,11){foreach($name in 'ReedStation','ReedStationBrowser'){
  $directory="samples/net$major.0/$name"
  $receipt=Get-Content "$directory/verification-evidence.json" -Raw|ConvertFrom-Json -AsHashtable
  $receipt.kind='execution';$receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
  $receipt.command='eng/Record-SupportPaths.ps1 -Record: actual paired compile/publish, real Docker missing-image cleanup regression, internal interface attach/detach and Chromium browser consumers'
  $receipt.output+= "`n" + $cleanup + "`n" + $network + "`n" + (Get-Content artifacts/support-paths/browser.json -Raw)
  $receipt.scope='Actual paired Windows desktop/Mono single-thread Chromium and Linux Docker consumers: score48, pre-cancellation, real address event, no callback for change after disposal, browser manual-refresh fallback. No other platforms, WASI, threaded browser, in-flight cancellation or GA proof.'
  [IO.File]::WriteAllText((Join-Path $root "$directory/behavior.evidence.json"),($receipt|ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
 }}}
}finally{
 Pop-Location
 foreach($key in $previous.Keys){[Environment]::SetEnvironmentVariable($key,$previous[$key])}
 foreach($scope in $persistent.Keys){if(([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress) -cne $persistent[$scope]){throw "Unexpected persistent $scope environment change; shared settings preserved."}}
 foreach($scope in $before.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "Unexpected $scope PATH change; shared settings preserved."}}
}
