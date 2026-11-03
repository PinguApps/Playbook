function Test-HostPublisherSources {
 foreach($source in (Get-Content "$PSScriptRoot/../docs/planning/audits/host-contracts.sources.json" -Raw | ConvertFrom-Json).sources) {
  $bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray()
  if([Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) -cne $source.sha256){throw "Publisher source drift: $($source.path)"}
 }
 Write-Host 'PASS immutable publisher source hashes'
}
function Get-HostArtifactProof([string]$Dotnet,[string]$Project,[string]$Target,[string]$Generator) {
 $root=Split-Path $PSScriptRoot -Parent
 $audit=Get-Content "$root/docs/planning/audits/runtime-security.assets.json" -Raw | ConvertFrom-Json
 $baseline=@($audit.baselines | Where-Object target -CEQ $Target)[0]
 $pin=Get-Content (Join-Path (Split-Path $Project) 'global.json') -Raw | ConvertFrom-Json
 $compiler=Join-Path (Split-Path $Dotnet) "sdk/$($pin.sdk.version)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
 Push-Location (Split-Path $Project)
 try {$pack=(& $Dotnet msbuild $Project -t:ResolveFrameworkReferences -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App'}
 finally {Pop-Location}
 if($pack.NuGetPackageVersion -cne $baseline.referencePack){throw 'Reference version drift'}
 $paths=@($compiler)
 foreach($name in @('System.Runtime.dll','System.Runtime.InteropServices.dll','System.Runtime.InteropServices.JavaScript.dll')) {
  $reference=Join-Path $pack.Path "ref/$Target/$name"
  $expected=@($baseline.framework | Where-Object name -CEQ $name)[0].sha256
  if((Get-FileHash $reference).Hash.ToLowerInvariant() -cne $expected){throw "Reference drift: $reference"}
  $paths+=$reference
 }
 if($Generator) {
  $path=Join-Path $pack.Path "analyzers/dotnet/cs/$Generator"
  $expected=@($baseline.tooling | Where-Object {$_.path.EndsWith('/'+$Generator)})[0].sha256
  if((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected){throw "Generator drift: $path"}
  $paths+=$path
 }
 @($paths | ForEach-Object {"ARTIFACT $_ sha256=$((Get-FileHash $_).Hash.ToLowerInvariant())"})
}
