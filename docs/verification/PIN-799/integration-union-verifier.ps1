$ErrorActionPreference='Stop'
$git='C:\Program Files\Git\cmd\git.exe'
$main='c905a5f438dcc75d0744b68be3c500047c990f6d'
$ours='746d24a851751815bcef57beade67528a4383caf'
$lf=[string][char]10
function Source([string]$Ref,[string]$Path) { ((& $git show ($Ref+':'+$Path)) -join $lf).TrimEnd() }
function Current([string]$Path) { [IO.File]::ReadAllText((Join-Path (Get-Location) $Path)).Replace([string][char]13,'').TrimEnd() }
function Hash([string]$Value) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($Value))) }
$steps='tests/PinguApps.Playbook.Tests/Steps/InstalledToolSteps.cs'
$merged=Current $steps
$mainSteps=Source $main $steps
$ownPattern='(?ms)\n    \[Then\("custom event migration retains complete canonical guidance and collision requirements offline"\)\]\n    public async Task CheckCustomEventMigrationAsync\(\)\n    \{.*?^    \}\n'
$withoutOwn=[regex]::Replace($merged,$ownPattern,'')
if($withoutOwn -ceq $merged){throw 'Own complete method not found'}
if([regex]::Replace($withoutOwn,"\n{3,}",$lf+$lf) -cne [regex]::Replace($mainSteps,"\n{3,}",$lf+$lf)){throw 'Complete peer Steps file differs after removing only own method'}
$methods=@()
foreach($name in @('CheckBrowserComponentActivityAsync','CheckHarbourStateAsync','CheckStandaloneGatewayAsync','CheckWebRecipesAsync')){
  $pattern='(?ms)^    (?:public|private) [^\n]*\b'+$name+'\([^\n]*\)\n    \{.*?^    \}'
  $expected=[regex]::Match($mainSteps,$pattern).Value
  $actual=[regex]::Match($merged,$pattern).Value
  if(-not $expected -or $expected -cne $actual){throw "Complete peer method mismatch $name"}
  $methods+=@{name=$name;sha256=(Hash $actual)}
}
$ownExpected=[regex]::Match((Source $ours $steps),$ownPattern).Value
$ownActual=[regex]::Match($merged,$ownPattern).Value
if($ownExpected -cne $ownActual){throw 'Own complete method changed'}
$feature='tests/PinguApps.Playbook.Tests/Features/InstalledTool.feature'
$withoutStep=[regex]::Replace((Current $feature),'(?m)^    And custom event migration retains complete canonical guidance and collision requirements offline\n','')
if($withoutStep -cne (Source $main $feature)){throw 'Complete peer feature differs after removing only own step'}
$project='tests/PinguApps.Playbook.Tests/PinguApps.Playbook.Tests.csproj'
$withoutResource=[regex]::Replace((Current $project),'(?m)^    <EmbeddedResource Include="../../samples/net\*/CairnSignals/[^\n]+\n','')
if($withoutResource -cne (Source $main $project)){throw 'Complete peer resource project differs after removing only own resource'}
$fixture='eng/Test-Evidence.ps1'
$withoutCopy=[regex]::Replace((Current $fixture),"(?m)^  Copy-Item [^\n]*'CairnSignals.prepare.js'[^\n]*\n",'')
if($withoutCopy -cne (Source $main $fixture)){throw 'Complete peer fixture script differs after removing only own copy'}
foreach($path in @('docs/contributing.md','docs/inspect-guidance.md')){
  $own=Source $ours $path
  $mainText=Source $main $path
  $base=Source '3048fc393385e8dbf1562e9bc1fcbf3c95424fed' $path
  $header=$base.Substring(0,$base.IndexOf($lf+$lf)+2)
  $addition=$own.Substring($header.Length,$own.Length-$base.Length)
  if(-not (Current $path).Contains($addition)){throw "Missing complete own doc paragraph $path"}
  if((Current $path).Replace($addition,'') -cne $mainText){throw "Complete peer doc differs $path"}
}
$path='docs/planning/audits/blazor.dispositions.json'
$mergedDisposition=Current $path | ConvertFrom-Json -AsHashtable -DateKind String
$mainDisposition=Source $main $path | ConvertFrom-Json -AsHashtable -DateKind String
$mergedDisposition.toolingGroups=@($mergedDisposition.toolingGroups | Where-Object id -ne 'custom-event-name-collision')
if(($mergedDisposition|ConvertTo-Json -Depth 100 -Compress) -cne ($mainDisposition|ConvertTo-Json -Depth 100 -Compress)){throw 'Complete peer source-disposition object differs after removing own entry'}
$path='docs/planning/audits/blazor.md'
$mainAudit=Source $main $path
$mergedAudit=Current $path
$mainRow=[regex]::Match($mainAudit,'(?m)^\| MathML rendering[^\n]+').Value
$ownRow=[regex]::Match($mergedAudit,'(?m)^\| MathML rendering[^\n]+').Value
if(-not $mainRow -or -not $ownRow -or $mergedAudit.Replace($ownRow,$mainRow) -cne $mainAudit){throw 'Complete peer audit differs beyond own row'}
$shared=@('docs/contributing.md','docs/inspect-guidance.md',$steps,$feature,$project,$fixture,'docs/planning/audits/blazor.dispositions.json','docs/planning/audits/blazor.md')
$peers=@()
foreach($path in (& $git diff --name-only '3048fc393385e8dbf1562e9bc1fcbf3c95424fed' $main)){
  if($path -in $shared){continue}
  $expected=(& $git rev-parse ($main+':'+$path)).Trim()
  $actual=(& $git hash-object --no-filters -- $path).Trim()
  if($expected -cne $actual){throw "Exact peer file changed $path"}
  $peers+=@{path=$path;gitBlob=$actual}
}
$ownInputs=@()
foreach($major in @(10,11)){
  $record=Get-Content "samples/net$major.0/CairnSignals/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable -DateKind String
  foreach($path in $record.inputs.Keys){
    $actual=(Get-FileHash $path).Hash.ToLowerInvariant()
    if($actual -cne $record.inputs[$path]){throw "Relevant canonical input changed $path"}
    $ownInputs+=@{path=$path;sha256=$actual}
  }
}
$artifacts=Get-Content docs/verification/PIN-799/canonical-http.evidence.json -Raw | ConvertFrom-Json -AsHashtable -DateKind String
foreach($artifact in $artifacts.checks){
  $dll="samples/net$($artifact.version).0/CairnSignals/bin/Release/net$($artifact.version).0/CairnSignals.dll"
  if((Get-FileHash $dll).Hash.ToLowerInvariant() -cne $artifact.dllSha256){throw "Executed sample DLL changed $dll"}
  $version=if($artifact.version -eq 10){'10.0.13'}else{'11.0.0-rc.2.26504.105'}
  $assets=Get-Content "samples/net$($artifact.version).0/CairnSignals/obj/Release/net$($artifact.version).0/staticwebassets.build.json" -Raw | ConvertFrom-Json
  $js=@($assets.Assets | Where-Object { $_.Identity.EndsWith('\blazor.web.js') })[0].Identity
  if((Get-FileHash $js).Hash.ToLowerInvariant() -cne $artifact.assetSha256){throw "Executed browser asset changed $js"}
}
@{kind='complete-peer-union-and-reuse-audit';main=$main;originalHead=$ours;capturedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceDirty=$true;completeMethods=$methods;wholePeerStepsMinusOwnMethod=$true;wholePeerFeatureMinusOwnStep=$true;wholePeerResourceProjectMinusOwnResource=$true;wholePeerFixtureMinusOwnCopy=$true;wholePeerDocsAndDispositionUnion=$true;exactPeerFiles=$peers;canonicalInputs=$ownInputs;executedDllAndServedJsEqual=$true;verifierSha256=(Get-FileHash $PSCommandPath).Hash.ToLowerInvariant()} | ConvertTo-Json -Depth 20 | Set-Content artifacts/pin799/integration-union.evidence.json
Write-Host "PASS complete shared file/method/resource/fixture/doc/disposition union; $($peers.Count) exact peer files; all canonical input and executed DLL/JS hashes equal."
