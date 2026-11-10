[CmdletBinding()]
param([switch]$Record, [switch]$RequireMissingBindingRejection,
  [string]$Dotnet='C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe',
  [string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$saved=@{}; foreach($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_CLI_TELEMETRY_OPTOUT','MSBUILDUSESERVER','UseSharedCompilation'){$saved[$key]=[Environment]::GetEnvironmentVariable($key)}
$container='playbook-gannet-'+[guid]::NewGuid().ToString('N')
$image='mcr.microsoft.com/dotnet/sdk@sha256:9c7b6b263d9d4c2e4718e16dae6d75cd8f4b5ccf206f1e7d48a98eb429981b03'
$hash='3b059283f689d4338a2dee33e1b433f2218f9d754e25b96821ff68281ea5d0f354d4f284bba00d8b594cf2c166531593b538e3f8308a349ca6e844ee74c03ebb'
Push-Location $root
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBUILDUSESERVER='0'; $env:UseSharedCompilation='false'
  $archive=Join-Path $root 'artifacts/authentication/runtime11.tar.gz'
  $null=New-Item -ItemType Directory (Split-Path $archive) -Force
  if(!(Test-Path $archive)){Invoke-WebRequest 'https://builds.dotnet.microsoft.com/dotnet/Runtime/11.0.0/dotnet-runtime-11.0.0-linux-x64.tar.gz' -OutFile $archive}
  if((Get-FileHash $archive -Algorithm SHA512).Hash.ToLowerInvariant() -cne $hash){throw 'Runtime archive differs from publisher release-metadata/11.0/releases.json SHA512.'}
  & $Docker run --pull never -d --name $container --hostname station.gannet.test --mount "type=bind,source=$root,target=/src,readonly" $image /bin/sleep 900
  & $Docker exec $container /bin/sh /src/eng/Prepare-GannetRealm.sh
  & $Docker exec $container /bin/sh -ec 'mkdir /runtime11; tar -xzf /src/artifacts/authentication/runtime11.tar.gz -C /runtime11'
  # No external network is needed once the private KDC and dependencies exist.
  & $Docker network disconnect bridge $container
  $identity=(& $Docker exec $container /bin/sh -ec 'uname -m; cat /etc/os-release; dpkg-query -W krb5-kdc libgssapi-krb5-2 libkrb5-3 openssl; sha256sum /usr/share/dotnet/shared/Microsoft.NETCore.App/*/System.Net.Security.dll /runtime11/shared/Microsoft.NETCore.App/*/System.Net.Security.dll /usr/share/dotnet/shared/Microsoft.NETCore.App/*/libSystem.Net.Security.Native.so /runtime11/shared/Microsoft.NETCore.App/*/libSystem.Net.Security.Native.so'|Out-String).Trim()
  Write-Host $identity
  foreach($major in 10,11){
    $sample="samples/net$major.0/GannetBinding/GannetBinding.csproj"
    if($Record){& "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $Dotnet -Kind compile}
    else {Push-Location (Split-Path $sample);try{& $Dotnet build -c Release -p:RestoreLockedMode=true}finally{Pop-Location}}
    $runtime=if($major -eq 10){'/usr/share/dotnet/dotnet'}else{'/runtime11/dotnet'}
    $dll="/src/samples/net$major.0/GannetBinding/bin/Release/net$major.0/GannetBinding.dll"
    $output=(& $Docker exec $container /usr/bin/env KRB5_CONFIG=/gannet/krb5.conf KRB5_KTNAME=/gannet/server.keytab KRB5CCNAME=FILE:/gannet/ccache $runtime $dll|Out-String).Trim()
    Write-Host $output
    if($major -eq 11){
      $negative=(& $Docker exec $container /usr/bin/env KRB5_CONFIG=/gannet/krb5.conf KRB5_KTNAME=/gannet/server.keytab KRB5CCNAME=FILE:/gannet/absent $runtime $dll --no-credentials|Out-String).Trim()
      Write-Host $negative; $output+="`n$negative"
      if($RequireMissingBindingRejection){
        # The retained security gate intentionally fails on the measured RC3/MIT route.
        & $Docker exec $container /usr/bin/env KRB5_CONFIG=/gannet/krb5.conf KRB5_KTNAME=/gannet/server.keytab KRB5CCNAME=FILE:/gannet/ccache $runtime $dll --require-missing-rejection
      }
    }
    if($Record){
      $p=Join-Path (Split-Path $sample) 'verification-evidence.json'
      $receipt=Get-Content $p -Raw|ConvertFrom-Json -AsHashtable
      $receipt.kind='execution'; $receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
      $receipt.command='eng/Test-GannetBinding.ps1 -Record: compile paired pinned consumers; disposable MIT KDC/keytabs; real loopback TLS and GSSAPI Kerberos; missing-client credential-cache negative on net11. No host credentials or published ports.'
      $receipt.scope='Documented matching/mismatched binding behavior and actual omission limitation. Missing-client rejection remains UNMET; not a proof of mandatory binding enforcement.'
      $receipt.host='Disposable Ubuntu24.04 x64 Docker container; host Windows x64 compilation'
      $receipt.output+="`nIMAGE $image`nRUNTIME_ARCHIVE_SHA512 $hash`n$identity`n$output"
      [IO.File]::WriteAllText((Join-Path $root (Join-Path (Split-Path $sample) 'behavior.evidence.json')),($receipt|ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
    }
  }
} finally {
  try { & $Docker rm -f $container | Out-Null } finally {
    Pop-Location
    foreach($key in $saved.Keys){[Environment]::SetEnvironmentVariable($key,$saved[$key],'Process')}
    foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected PATH change: $scope"}}
  }
}
