[CmdletBinding()]
param([string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe',[switch]$Record)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
. (Join-Path $PSScriptRoot 'QuayProof.ps1')
$scratch=Join-Path $root ('artifacts/quay-signatures/'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory $scratch
$log=[Collections.Generic.List[string]]::new()
$before=@{};foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{};foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$proof=Join-Path $root 'samples/QuayContainerProof'
$pins=Get-Content (Join-Path $proof 'artifacts.json') -Raw|ConvertFrom-Json -AsHashtable
$registry=$null
function Use-Trust([string]$Name,[string]$Scope='mcr.microsoft.com/dotnet/runtime',[switch]$Untrusted){
  $config=Join-Path $scratch $Name
  $environment=@{NOTATION_CONFIG=$config;NOTATION_CACHE=(Join-Path $config 'cache');NOTATION_LIBEXEC=(Join-Path $config 'plugins')}
  $pwsh=(Get-Process -Id $PID).Path
  $null=Invoke-Quay $pwsh @('-NoProfile','-File',(Join-Path $root 'samples/net11.0/QuayWorker/Initialize-Trust.ps1'),'-Notation',$notation,'-Configuration',$config)
  foreach($certificate in $pins.certificates){
    $file=Join-Path $proof $certificate.file
    if((Get-FileHash $file).Hash.ToLowerInvariant() -cne $certificate.sha256){throw 'Certificate drift'}
    $log.Add("ROOT $($certificate.url);sha256=$($certificate.sha256)")
    if($Untrusted -and $certificate.type -eq 'ca'){$file=Join-Path $proof 'timestamp.crt'}
    if($Untrusted -and $certificate.type -eq 'ca'){
      $ca=Join-Path $config 'truststore/x509/ca/quay-microsoft/supply-chain.crt'
      Copy-Item -LiteralPath $file -Destination $ca -Force
    }
  }
  $policy=Get-Content (Join-Path $config 'input-policy.json') -Raw|ConvertFrom-Json
  $policy.trustPolicies[0].registryScopes=@($Scope)
  $path=Join-Path $config 'input-policy.json'
  Write-QuayFile $path ($policy|ConvertTo-Json -Depth 12)
  $null=Invoke-Quay $notation @('policy','import','--force',$path) -Environment $environment
  return $environment
}
try{
  foreach($name in 'notation','oras'){
    $pin=$pins[$name];$zip=Join-Path $scratch "$name.zip"
    Invoke-WebRequest $pin.url -OutFile $zip
    if((Get-FileHash $zip).Hash.ToLowerInvariant() -cne $pin.sha256){throw "$name archive drift"}
    Expand-Archive -LiteralPath $zip -DestinationPath (Join-Path $scratch $name)
    $log.Add("TOOL $name/$($pin.version);url=$($pin.url);sha256=$($pin.sha256)")
  }
  $notation=Join-Path $scratch 'notation/notation.exe';$oras=Join-Path $scratch 'oras/oras.exe'
  Assert-QuayText (Invoke-Quay $notation @('version')) '1.3.2'
  Assert-QuayText (Invoke-Quay $oras @('version')) '1.3.6'
  $trusted=Use-Trust 'trusted';$untrusted=Use-Trust 'untrusted' -Untrusted
  $wrong=Use-Trust 'wrong-scope' 'mcr.microsoft.com/dotnet/sdk'
  foreach($major in '10','11'){
    $image=$pins.images[$major]
    $manifest=Invoke-Quay $oras @('manifest','fetch',"mcr.microsoft.com/dotnet/runtime@$($image.index)")|ConvertFrom-Json
    $selected=@($manifest.manifests|Where-Object {$_.platform.os -eq 'linux' -and $_.platform.architecture -eq 'amd64'})
    if($selected.Count -ne 1 -or $selected[0].digest -cne $image.amd64){throw 'Selected platform digest drift'}
    foreach($digest in $image.index,$image.amd64){
      $reference="mcr.microsoft.com/dotnet/runtime@$digest"
      $discovery=Invoke-Quay $oras @('discover',$reference,'--format','json')
      Assert-QuayText $discovery 'application/vnd.cncf.notary.signature'
      $null=Invoke-Quay $notation @('inspect',$reference) -Environment $trusted
      Assert-QuayText (Invoke-Quay $notation @('verify',$reference) -Environment $trusted) 'Successfully verified signature'
    }
    $reference="mcr.microsoft.com/dotnet/runtime@$($image.amd64)"
    $pwsh=(Get-Process -Id $PID).Path
    Assert-QuayText (Invoke-Quay $pwsh @('-NoProfile','-File',(Join-Path $root "samples/net$major.0/QuayWorker/Verify-Base.ps1"),'-Notation',$notation,'-Configuration',$trusted.NOTATION_CONFIG,'-Image',$reference)) 'Successfully verified signature'
    $rejected=Invoke-Quay $notation @('verify',$reference) -Environment $untrusted -Failure
    Assert-QuayText $rejected 'signature verification failed'
    $rejected=Invoke-Quay $notation @('verify',$reference) -Environment $wrong -Failure
    Assert-QuayText $rejected 'no applicable trust policy'
    $log.Add("PASS net${major}: index and selected linux/amd64 signatures verified; wrong-root and wrong-repository rejected; older image signing is not exclusive to .NET11.")
  }
  # A disposable loopback registry holds real copied bytes, never a production image.
  $name='playbook-quay-'+[guid]::NewGuid().ToString('N')
  $registry=(Invoke-Quay $Docker @('run','--rm','-d','--name',$name,'-p','127.0.0.1::5000',$pins.registry)).Trim()
  $address=(Invoke-Quay $Docker @('port',$registry,'5000/tcp')).Trim()
  $local="$address/quay/runtime";$digest=$pins.images['11'].amd64
  $reference="$local@$digest"
  $localTrust=Use-Trust 'local-fixture' $local
  $null=Invoke-Quay $oras @('cp','--to-plain-http',"mcr.microsoft.com/dotnet/runtime@$digest","${local}:fixture")
  $missing=Invoke-Quay $notation @('verify','--insecure-registry',$reference) -Environment $localTrust -Failure
  Assert-QuayText $missing 'no signature'
  $null=Invoke-Quay $oras @('cp','--recursive','--to-plain-http',"mcr.microsoft.com/dotnet/runtime@$digest","${local}:fixture")
  Assert-QuayText (Invoke-Quay $notation @('verify','--insecure-registry',$reference) -Environment $localTrust) 'Successfully verified signature'
  $discovery=Invoke-Quay $oras @('discover','--plain-http',$reference,'--format','json')|ConvertFrom-Json
  $signatures=@($discovery.referrers|Where-Object artifactType -EQ 'application/vnd.cncf.notary.signature')
  if($signatures.Count -eq 0){throw 'Copy lost signatures'}
  $signature=Invoke-Quay $oras @('manifest','fetch','--plain-http',"$local@$($signatures[0].digest)")|ConvertFrom-Json
  if($signature.layers.Count -ne 1){throw 'Expected one signature envelope'}
  $blob=Join-Path $scratch 'tampered.cose'
  $null=Invoke-Quay $oras @('blob','fetch','--plain-http',"$local@$($signature.layers[0].digest)",'-o',$blob)
  $bytes=[IO.File]::ReadAllBytes($blob);$bytes[$bytes.Length-1]=$bytes[$bytes.Length-1] -bxor 1;[IO.File]::WriteAllBytes($blob,$bytes)
  # Remove genuine envelopes before adding the altered one; otherwise one valid signature could pass.
  foreach($original in $signatures){$null=Invoke-Quay $oras @('manifest','delete','--plain-http','--force',"$local@$($original.digest)")}
  $null=Invoke-Quay $oras @('attach','--plain-http','--artifact-type','application/vnd.cncf.notary.signature',$reference,'tampered.cose:application/cose') $scratch
  $tampered=Invoke-Quay $notation @('verify','--insecure-registry',$reference) -Environment $localTrust -Failure
  Assert-QuayText $tampered 'failed'
  $log.Add('PASS missing signature rejected; recursive real signed copy verified; altered COSE envelope rejected after removing all genuine signatures. HTTP exception restricted to disposable loopback registry.')
  if($Record){
    foreach($major in 10,11){
      $sample="samples/net$major.0/QuayWorker/QuayWorker.csproj"
      $sdk=if($major -eq 10){'10.0.402'}else{'11.0.100-rc.2.26504.105'}
      $expected=@{sample=$sample;kind='compile-and-run';target="net$major.0";sdk=$sdk;language=$(if($major -eq 10){'14.0'}else{'15.0'});scope='Previously observed, fresh paired worker compilation/execution.'}
      $workerRecord=Read-VerificationEvidence $root $expected
      $workerRecord.Remove('freshness');$workerRecord.command='eng/Test-QuayWorker.ps1 -Record; eng/Test-QuaySignatures.ps1 -Record'
      $workerRecord.scope='Real worker build/run plus index/platform signature verification and wrong-root/scope/missing/tampered controls in isolated trust stores.'
      $workerRecord.recordedAt=[DateTimeOffset]::UtcNow.ToString('O');$workerRecord.output += "`n"+($log -join "`n").Replace($root,'<repo>').Replace($scratch,'<session>')
      Write-QuayFile (Join-Path (Split-Path (Join-Path $root $sample)) 'signatures.evidence.json') (($workerRecord|ConvertTo-Json -Depth 40)+"`n")
    }
  }
  Write-Host 'PASS real Quay base-image signature and rejection checks.'
}finally{
  if($registry){$null=Invoke-Quay $Docker @('stop',$registry)}
  foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; shared settings preserved."}}
  foreach($scope in $persistent.Keys){if($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress)){throw "Unexpected persistent $scope environment change."}}
  Write-Host "Disposable output: $scratch"
}
