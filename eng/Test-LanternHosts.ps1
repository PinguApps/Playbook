[CmdletBinding()]
param(
 [string]$Net10Dotnet=(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
 [string]$Net11Dotnet=(Join-Path $PSScriptRoot '../artifacts/native-dotnet/dotnet.exe'),
 [string]$Node='C:/Program Files/nodejs/node.exe',
 [switch]$Record,[switch]$VerifySources
)
$ErrorActionPreference='Stop';$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/HostContractEvidence.ps1"
$before=@{};foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$old=@{};foreach($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER'){$old[$key]=[Environment]::GetEnvironmentVariable($key)}
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0';$env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false';$env:MSBUILDUSESERVER='0'
 if($VerifySources){Test-HostPublisherSources}
 $tsc=Join-Path $root 'artifacts/host-types/node_modules/typescript/bin/tsc'
 if(-not(Test-Path $tsc)) {
  & $Node (Join-Path (Split-Path $Node) 'node_modules/npm/bin/npm-cli.js') install --prefix "$root/artifacts/host-types" --ignore-scripts --no-audit --no-fund typescript@5.9.3
 }
 $audit=Get-Content docs/planning/audits/runtime-security.assets.json -Raw | ConvertFrom-Json
 $routes=@(@('net10.0','Mono','10.0.14',$Net10Dotnet),@('net11.0','Mono','11.0.0',$Net11Dotnet),@('net11.0','CoreCLR','11.0.0',$Net11Dotnet))
 foreach($route in $routes) {
  $target,$flavor,$version,$dotnet=$route
  $name="LanternHost$flavor";$directory=Join-Path $root "samples/$target/$name";$project="$directory/$name.csproj"
  $types=Join-Path $root "artifacts/host-types/$target-$flavor";New-Item -ItemType Directory -Force "$types/_framework" | Out-Null
  $id=if($flavor -eq 'Mono'){'microsoft.netcore.app.runtime.mono.browser-wasm'}else{'microsoft.netcore.app.runtime.browser-wasm'}
  $package=Join-Path $HOME ".nuget/packages/$id/$version"
  $declarations="$package/runtimes/browser-wasm/native/dotnet.d.ts"
  $expected=@(($audit.baselines | Where-Object target -CEQ $target).browser | Where-Object id -CEQ $id)[0]
  $archive="$package/$id.$version.nupkg"
  if(-not(Test-Path $declarations)) {
   $package=Join-Path (Split-Path $dotnet) "packs/$id/$version"
   $declarations="$package/runtimes/browser-wasm/native/dotnet.d.ts"
   $archive=Join-Path $root "artifacts/host-types/$id.$version.nupkg"
   if(-not(Test-Path $archive)){Invoke-WebRequest $expected.url -OutFile $archive}
  }
  if((Get-FileHash $declarations).Hash.ToLowerInvariant() -cne $expected.declarationsSha256){throw "Declaration drift: $id"}
  if((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $expected.sha256){throw "Archive drift: $id"}
  Copy-Item $declarations "$types/_framework/dotnet.d.ts";Copy-Item "$directory/main.ts" "$types/main.ts"
  & $Node $tsc "$types/main.ts" --strict --target es2022 --module es2022 --lib es2022,dom --outDir "$types/out"
  # Reject the opposite diagnostic-provider spelling against the same actual declaration.
  $correct=if($target -eq 'net10.0'){'provider_name'}else{'providerName'};$wrong=if($target -eq 'net10.0'){'providerName'}else{'provider_name'}
  (Get-Content "$types/main.ts" -Raw).Replace($correct+':',$wrong+':') | Set-Content "$types/invalid.ts"
  $PSNativeCommandUseErrorActionPreference=$false
  try {$negative=(& $Node $tsc "$types/invalid.ts" --strict --target es2022 --module es2022 --lib es2022,dom --noEmit 2>&1 | Out-String);$exitCode=$LASTEXITCODE}finally{$PSNativeCommandUseErrorActionPreference=$true}
  if($exitCode -eq 0 -or $negative -notmatch 'TS2561|TS2353'){throw "Provider spelling negative failed: $negative"}
  @'
import { type DotnetModuleConfig, type AssetEntry } from './_framework/dotnet.js';
const config: DotnetModuleConfig = { configSrc: './legacy.json' };
const asset: AssetEntry = { name: 'lantern.dll', behavior: 'assembly', virtualPath: '/lantern.dll', loadRemote: true };
void config; void asset;
'@ | Set-Content "$types/legacy.ts"
  $PSNativeCommandUseErrorActionPreference=$false
  try {$legacy=(& $Node $tsc "$types/legacy.ts" --strict --target es2022 --module es2022 --lib es2022,dom --noEmit 2>&1 | Out-String);$legacyExit=$LASTEXITCODE}finally{$PSNativeCommandUseErrorActionPreference=$true}
  if($target -eq 'net10.0') {if($legacyExit -ne 0){throw "Older asset contract failed: $legacy"}}
  elseif($legacyExit -eq 0 -or $legacy -notmatch 'TS2305|TS2614' -or $legacy -notmatch 'TS2353|TS2561'){throw "Expected removed legacy contract: $legacy"}
  $artifacts=Get-HostArtifactProof $dotnet $project $target 'Microsoft.Interop.JavaScript.JSImportGenerator.dll'
  $artifacts+="PACKAGE $id/$version archive=$($expected.sha256) dotnet.d.ts=$($expected.declarationsSha256)"
  $artifacts+="TypeScript 5.9.3 compiler=$((Get-FileHash $tsc).Hash.ToLowerInvariant())`n$negative`n$legacy"
  Push-Location $directory
  try {
   & $dotnet publish -c Release -p:PublishTrimmed=false -p:RestoreLockedMode=true
   if($Record){& "$PSScriptRoot/Record-Evidence.ps1" -Sample "samples/$target/$name/$name.csproj" -Dotnet $dotnet -Kind compile -NativeToolchains @('Emscripten/'+$(if($target -eq 'net10.0'){'3.1.56'}else{'6.0.2'}))}
  } finally {Pop-Location}
  $published="$directory/bin/Release/$target/publish/wwwroot"
  Copy-Item "$types/out/main.js" "$published/main.js"
  $artifacts+=@(Get-ChildItem "$published/_framework" -File | Where-Object Name -Match '^dotnet\..*\.(js|wasm)$' | ForEach-Object {"DEPLOYED $($_.Name) sha256=$((Get-FileHash $_.FullName).Hash.ToLowerInvariant())"})
  if($Record){
   $receipt=Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
   $receipt.command+="; $Node $tsc $types/main.ts --strict --target es2022 --module es2022 --lib es2022,dom --outDir $types/out; TypeScript opposite-provider negative; $dotnet publish -c Release -p:PublishTrimmed=false -p:RestoreLockedMode=true; Get-HostArtifactProof"
   $receipt.output+="`n"+($artifacts -join "`n");$receipt.scope='Actual managed/native browser build, canonical TypeScript compilation and rejected opposite diagnostic spelling; original audit archive/declaration/reference/generator hashes agree.'
   [IO.File]::WriteAllText("$directory/verification-evidence.json",($receipt | ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
  }
 }
 & $Node eng/Test-LanternHosts.mjs
 if($Record){
  $output=[IO.File]::ReadAllText("$root/artifacts/lantern-hosts/browser.json")
  foreach($route in $routes){
   $target,$flavor,$version,$dotnet=$route;$name="LanternHost$flavor";$directory="$root/samples/$target/$name"
   $receipt=Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
   $receipt.kind='execution';$receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O');$receipt.command="$Node $root/eng/Test-LanternHosts.mjs";$receipt.scope='Real Chromium per-runtime startup/main return17, live export/exit rejection, configured environment/args, resource-loader success/native404 failure, old legacy-config failure/current ignored override, and real selected EventSource trace. No other-browser/platform/AOT/GA proof.'
   $receipt.output=$output
   [IO.File]::WriteAllText("$directory/browser.evidence.json",($receipt | ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
  }
 }
} finally {
 Pop-Location;foreach($key in $old.Keys){[Environment]::SetEnvironmentVariable($key,$old[$key])}
 foreach($scope in 'Process','User','Machine'){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "PATH changed: $scope"}}
}
