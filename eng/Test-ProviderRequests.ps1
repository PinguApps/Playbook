[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{}; foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$keys='DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER','UseSharedCompilation','DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE'
$previous=@{}; foreach($key in $keys){$previous[$key]=[Environment]::GetEnvironmentVariable($key)}
$docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe'
$container='playbook-rookery-'+[guid]::NewGuid().ToString('N')
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; $env:MSBUILDUSESERVER='0'; $env:UseSharedCompilation='false'; $env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'
 foreach($source in (Get-Content samples/ProviderKeys/sources.json -Raw|ConvertFrom-Json).sources){
  $bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray()
  if([Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) -cne $source.sha256){throw "Source drift: $($source.url)"}
  if($source.url.EndsWith('/SafeEvpPKeyHandle.OpenSsl.cs')){
   $text=[Text.Encoding]::UTF8.GetString($bytes)
   $methods=[regex]::Matches($text,'(?s)((?:\s*\[UnsupportedOSPlatform\("[^"]+"\)\])+\s*)public static SafeEvpPKeyHandle OpenKeyFromProvider\(')
   $expected=if($source.url.Contains('/820230c')){2}else{1}
   if($methods.Count -ne $expected){throw 'Provider overload source count changed'}
   foreach($method in $methods){foreach($platform in 'android','browser','ios','tvos','windows'){
    if(!$method.Groups[1].Value.Contains('[UnsupportedOSPlatform("'+$platform+'")]')){throw "Missing provider platform annotation: $platform"}
   }}
   Write-Output "SOURCE provider overloads=$expected unsupported=android,browser,ios,tvos,windows sha256=$($source.sha256)"
  }
 }
 $archive=Join-Path $root 'artifacts/provider-keys/runtime11.tar.gz'
 $null=New-Item -ItemType Directory (Split-Path $archive) -Force
 if(!(Test-Path $archive)){Invoke-WebRequest 'https://builds.dotnet.microsoft.com/dotnet/Runtime/11.0.0-rc.2.26504.105/dotnet-runtime-11.0.0-rc.2.26504.105-linux-x64.tar.gz' -OutFile $archive}
 # Microsoft's versioned archive .sha512: check both cached and newly downloaded bytes before extraction.
 $expectedArchiveHash='59db88134fdd9012fc6c43f4018189d33f3ff3cc871722f8a6205d75f114f2d9f5bc71605361888ca0f74911ace06f97f9b419eeb316d520be0c241031233920'
 if((Get-FileHash $archive -Algorithm SHA512).Hash.ToLowerInvariant() -cne $expectedArchiveHash){throw 'Runtime archive SHA512 mismatch; no extraction performed.'}
 Write-Output "RUNTIME_ARCHIVE verified SHA512=$expectedArchiveHash"
 $image='mcr.microsoft.com/dotnet/sdk@sha256:0d99ebf1ccad15037b5c98c9fe8979d6b8dbac23812feae2277a0b277b5c5ffa'
 & $docker run --pull never -d --name $container --network none --mount "type=bind,source=$root,target=/src,readonly" $image sleep 1800
 $native=(& $docker exec $container sh -ec 'mkdir /proof /runtime11; tar -xzf /src/artifacts/provider-keys/runtime11.tar.gz -C /runtime11; openssl version -a; openssl list -providers -provider default -provider base; sha256sum /usr/lib/x86_64-linux-gnu/libcrypto.so.3 /usr/lib/x86_64-linux-gnu/libssl.so.3 /runtime11/shared/Microsoft.NETCore.App/*/libSystem.Security.Cryptography.Native.OpenSsl.so; openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out /proof/rookery.pem 2>/dev/null' | Out-String)
 $native+="IMAGE $image`nRUNTIME_ARCHIVE sha256=$((Get-FileHash $archive).Hash)"
 $audit=Get-Content docs/planning/audits/runtime-security.assets.json -Raw|ConvertFrom-Json
 foreach($major in 10,11){
  $dn=if($major -eq 10){'C:/Program Files/dotnet/dotnet.exe'}else{'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'}
  foreach($name in 'RookeryKeys','RookeryProperty','MoorlandReports'){
   $sample="samples/net$major.0/$name/$name.csproj"
   $kind=if($name -eq 'RookeryKeys'){'compile'}else{'compile-and-run'}
   if($Record){& "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dn -Kind $kind}
   Push-Location (Split-Path $sample)
   try {
    if(!$Record){& $dn build -c Release -p:RestoreLockedMode=true; if($name -ne 'RookeryKeys'){& $dn "bin/Release/net$major.0/$name.dll"}}
    $pack=(& $dn msbuild "$name.csproj" -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack|ConvertFrom-Json).Items.ResolvedTargetingPack|Where-Object Identity -CEQ 'Microsoft.NETCore.App'
   }finally{Pop-Location}
   $baseline=@($audit.baselines|Where-Object target -CEQ "net$major.0")[0]
   if($pack.NuGetPackageVersion -cne $baseline.referencePack){throw 'Reference pack drift'}
   $identities=@()
   foreach($assembly in 'System.Runtime.dll','System.Security.Cryptography.dll','System.Net.Http.dll'){
    $path=Join-Path $pack.Path "ref/net$major.0/$assembly"; $hash=(Get-FileHash $path).Hash.ToLowerInvariant()
    if($hash -cne @($baseline.framework|Where-Object name -CEQ $assembly)[0].sha256){throw "Audit reference mismatch: $path"}
    $identities+="ARTIFACT $path sha256=$hash"
   }
   $sdk=(Get-Content (Join-Path (Split-Path $sample) 'global.json') -Raw|ConvertFrom-Json).sdk.version
   $compiler=Join-Path (Split-Path $dn) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
   $identities+="COMPILER $compiler sha256=$((Get-FileHash $compiler).Hash)"
   $output=''
   if($name -eq 'RookeryKeys'){
    $runtime=if($major -eq 10){'/usr/share/dotnet/dotnet'}else{'/runtime11/dotnet'}
    $output=(& $docker exec $container $runtime "/src/samples/net$major.0/RookeryKeys/bin/Release/net$major.0/RookeryKeys.dll"|Out-String)
    if($output -notmatch 'PASS provider RSA'){throw 'Missing real provider result'}
    if($major -eq 11 -and $output -notmatch 'PASS ordered'){throw 'Missing real property-query result'}
    $output+=$native
   }
   if($name -eq 'MoorlandReports'){
    Push-Location (Split-Path $sample)
    try{$probe=(& $dn build -c Release --no-restore -p:DefineConstants=PLATFORM_PROBE -p:TreatWarningsAsErrors=false -p:WarningsAsErrors= -t:Rebuild | Out-String)}finally{Pop-Location}
    if(($major -eq 11) -ne ($probe -match 'warning CA1416')){throw "Unexpected analyzer result for .NET$major"}
    $output+="`nANALYZER synthetic android call-site, not mobile compilation/execution:`n$probe"
   }
   Write-Host $output
   if($Record){
    $path=Join-Path $root (Join-Path (Split-Path $sample) 'verification-evidence.json')
    $receipt=Get-Content $path -Raw|ConvertFrom-Json -AsHashtable
    $receipt.output+="`n"+($identities -join "`n")+"`n"+$output
    [IO.File]::WriteAllText($path,($receipt|ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
    if($name -eq 'RookeryKeys'){
     $receipt.kind='execution';$receipt.command='eng/Test-ProviderRequests.ps1 -Record: pinned network-none Docker fixture, openssl genpkey, actual old/new provider consumers'
     $receipt.scope='Real Linux x64 OpenSSL default/base RSA signing, verification/tamper, missing provider/key, independent handle ownership/disposal; .NET11 ordered provider lists and property-query acceptance/rejection. No FIPS/HSM/custom-provider or GA proof.'
     [IO.File]::WriteAllText((Join-Path (Split-Path $path) 'provider.evidence.json'),($receipt|ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
    }
   }
  }
 }
}finally{
 try{ $existing=@(& $docker container ls -a --format '{{.Names}}'); if($container -cin $existing){& $docker rm -f $container} }finally{
  Pop-Location
  foreach($key in $keys){[Environment]::SetEnvironmentVariable($key,$previous[$key])}
  foreach($scope in $persistent.Keys){if(([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress) -cne $persistent[$scope]){throw "Unexpected persistent $scope environment change; shared settings preserved."}}
  foreach($scope in $before.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "Unexpected $scope PATH change; shared settings preserved."}}
 }
}
