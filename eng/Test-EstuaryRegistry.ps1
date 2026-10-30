[CmdletBinding()]
param(
  [string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe',
  [string]$Dotnet='C:/Program Files/dotnet/dotnet.exe',
  [switch]$Record
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'EstuaryProof.ps1')
$root=Split-Path $PSScriptRoot -Parent
$scratch=Join-Path $root ('artifacts/estuary-registry/'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory $scratch -Force
$log=[Collections.Generic.List[string]]::new()
$paths=@{};foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{};foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$owned=[Collections.Generic.List[string]]::new();$tags=[Collections.Generic.List[string]]::new()
$network='estuary-'+[guid]::NewGuid().ToString('N');$backend=$network+'-registry'
$createdNetwork=$false
$tar=(Get-Command tar -CommandType Application|Select-Object -First 1).Source
$dockerConfig=Join-Path $scratch 'docker-config';$null=New-Item -ItemType Directory $dockerConfig
function Invoke-RegistryDocker([string[]]$Arguments){ Invoke-Estuary $Docker $root (@('--config',$dockerConfig)+$Arguments) }
try {
  $pins=Get-Content (Join-Path $root 'samples/EstuarySdk/registry-artifacts.json') -Raw|ConvertFrom-Json -AsHashtable
  $archive=Join-Path $scratch 'oras.zip'
  Invoke-WebRequest $pins.oras.url -OutFile $archive
  if((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $pins.oras.sha256){throw 'ORAS archive drift'}
  Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $scratch 'oras')
  $oras=Join-Path $scratch 'oras/oras.exe'
  $helperInfo=Invoke-Estuary $Dotnet $root @('--info')
  $null=Invoke-Estuary $Dotnet $root @('restore','samples/EstuarySdk/RegistryFixture')
  $helperInputs=Initialize-EstuaryInputs 'samples/EstuarySdk/RegistryFixture/RegistryFixture.csproj'
  $null=Invoke-Estuary $Dotnet $root @('build','samples/EstuarySdk/RegistryFixture','-c','Release','--no-restore')
  $null=Invoke-RegistryDocker @('network','create','--internal',$network)
  $createdNetwork=$true
  $null=Invoke-RegistryDocker @('run','-d','--name',$backend,'--network','bridge','--publish','127.0.0.1::5000',$pins.registry)
  $owned.Add($backend)
  $null=Invoke-RegistryDocker @('network','connect',$network,$backend)
  $ports=(Invoke-RegistryDocker @('port',$backend,'5000/tcp')).Trim()
  if($ports -notmatch '^127\.0\.0\.1:(\d+)$'){throw "Unexpected private backend binding: $ports"}
  $port=$Matches[1]
  $ready=$false
  for($attempt=0;$attempt -lt 30;$attempt++){
    try { $ready=(Invoke-WebRequest "http://127.0.0.1:$port/v2/" -ConnectionTimeoutSeconds 1 -OperationTimeoutSeconds 1).StatusCode -eq 200 } catch { $ready=$false }
    if($ready){break}
    [Threading.Thread]::Sleep(100)
  }
  if(-not $ready){throw 'Disposable registry did not become ready'}
  foreach($major in '10','11'){
    $pin=$pins.lanes[$major]
    $base="127.0.0.1:$port/estuary/base:$major"
    # ORAS runs on Windows, where the loopback port is published. The Docker
    # daemon's own 127.0.0.1 is a different machine under Docker Desktop.
    $null=Invoke-Estuary $oras $root @('cp','--to-plain-http',$pin.base,$base) -Environment @{DOCKER_CONFIG=$dockerConfig}
    $source=Join-Path $root "samples/net$major.0/EstuaryRegistry"
    $sample="samples/net$major.0/EstuaryRegistry/EstuaryRegistry.csproj"
    $inputs=Get-EvidenceInputs $root $sample
    $lane=Join-Path $scratch "net$major.0";$null=New-Item -ItemType Directory $lane
    $name=$network+"-sdk$major";$owned.Add($name)
    $arguments=@('run','--name',$name,'--network',$network,'--add-host','estuary.test:127.0.0.1','--add-host','auth.estuary.test:127.0.0.1',
      '--env','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0','--env','DOTNET_CLI_TELEMETRY_OPTOUT=1','--env','DOTNET_ROLL_FORWARD=Major','--env','DOTNET_ROLL_FORWARD_TO_PRERELEASE=1',
      '--mount',"type=bind,source=$source,target=/source,readonly",'--mount',"type=bind,source=$lane,target=/work",'--mount',
      "type=bind,source=$(Join-Path $root 'samples/EstuarySdk/RegistryFixture/bin/Release/net10.0'),target=/fixture,readonly",
      $pin.sdkImage,'/usr/share/dotnet/dotnet','/fixture/RegistryFixture.dll','/source','/work',"http://${backend}:5000",$major)
    $result=Invoke-RegistryDocker $arguments
    Assert-Estuary $result 'PASS: real TLS'
    Write-Host $result
    foreach($tag in 'https','local-http'){
      $image="127.0.0.1:$port/estuary/cargo:$tag"
      $layout=Join-Path $lane "oci-$tag";$archive=Join-Path $lane "$tag.tar"
      $uniqueTag="$network-$major-$tag"
      $null=Invoke-Estuary $oras $root @('cp','--from-plain-http','--to-oci-layout',$image,"${layout}:$uniqueTag") -Environment @{DOCKER_CONFIG=$dockerConfig}
      # Docker Desktop preserves the OCI reference literally. Supply a complete
      # repository:tag so a subsequent CLI lookup does not append a different tag.
      $indexPath=Join-Path $layout 'index.json'
      $index=Get-Content $indexPath -Raw|ConvertFrom-Json -AsHashtable
      $imageId=$index.manifests[0].digest
      $localTag="localhost/${uniqueTag}:fixture"
      $index.manifests[0].annotations['org.opencontainers.image.ref.name']=$localTag
      Write-EstuaryFile $indexPath ($index|ConvertTo-Json -Depth 20)
      $null=Invoke-Estuary $tar $root @('-C',$layout,'-cf',$archive,'.')
      $loaded=Invoke-RegistryDocker @('load','--input',$archive)
      Assert-Estuary $loaded "Loaded image: $localTag"
      $tags.Add($localTag)
      $null=Invoke-RegistryDocker @('image','inspect',$imageId)
      $run=Invoke-RegistryDocker @('run','--rm','--network','none',$imageId)
      Assert-Estuary $run "ESTUARY_CONTAINER: copper lanterns; runtime=$major."
      $log.Add("EXECUTED net$major image $tag`n$run")
    }
    $manifestPath=Join-Path $source 'verification.json'
    $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json -AsHashtable
    $dependencies=@((Get-Content (Join-Path $lane 'app/obj/project.assets.json') -Raw|ConvertFrom-Json -AsHashtable).libraries.Keys|Sort-Object -CaseSensitive)
    if($dependencies.Count -ne 0){throw 'Unexpected unpinned registry sample packages'}
    $wire=Get-Content (Join-Path $lane 'transcript.txt') -Raw
    Assert-EstuaryInputs $inputs (Get-EvidenceInputs $root $sample)
    if($Record){
      $recordValue=[ordered]@{
        kind='compile-and-run';sample=$sample;target="net$major.0";sdk=$pin.sdk;language=$pin.language
        command="eng/Test-EstuaryRegistry.ps1 -Docker '$Docker' -Dotnet '$Dotnet' -Record"
        scope='Real isolated Linux x64 SDK build/run, HTTPS and explicitly insecure local registry token exchange and image push/run; SDK11 rejects invalid realm controls before token requests. Disposable Distribution3 backend, generated fixture credentials, private child trust bundle, no host trust/config changes.'
        recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim();sourceDirty=$true
        inputs=$inputs;dependencies=$dependencies;workloads=@();nativeToolchains=@()
        host="Docker Linux x64; SDK image $($pin.sdkImage); base $($pin.base); registry $($pins.registry)"
        sdkInfo=$wire.Substring(0,[Math]::Min(5000,$wire.Length));workloadInfo='No optional workloads; package-free original app. Fixture built net10 and run on matched installed container runtime with explicit Major/prerelease roll-forward.'
        output=$wire+"`n"+($log -join "`n")
      }
      Write-EstuaryFile (Join-Path $source 'verification-evidence.json') (($recordValue|ConvertTo-Json -Depth 30)+"`n")
    }
  }
  Save-EstuaryEvidence 'samples/EstuarySdk/RegistryFixture/RegistryFixture.csproj' '10.0.402' '14.0' 'net10.0' 'Actual Windows SDK10 helper compilation and real helper execution on the two pinned Linux container runtimes, orchestrating TLS and insecure-local challenges, original worker publication and SDK11 refusal controls. Fixture Major/prerelease roll-forward is explicit.' $helperInfo 'eng/Test-EstuaryRegistry.ps1 -Record' -ExpectedInputs $helperInputs
  Write-Host 'PASS: paired SDK registry realm and actual published image execution'
} finally {
  foreach($name in $owned){$null=Invoke-RegistryDocker @('rm','--force',$name)}
  if($createdNetwork){$null=Invoke-RegistryDocker @('network','rm',$network)}
  foreach($tag in ($tags|Select-Object -Unique)){$null=Invoke-RegistryDocker @('image','rm',$tag)}
  foreach($scope in $paths.Keys){if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change"}}
  foreach($scope in $persistent.Keys){if($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress)){throw "Unexpected $scope persistent environment change"}}
  Write-Host "Disposable registry commands/exchanges: $scratch"
}
