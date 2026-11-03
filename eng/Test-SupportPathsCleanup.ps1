[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe'
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$output=[Collections.Generic.List[string]]::new()
$failure=$null
$previousCompilation=[Environment]::GetEnvironmentVariable('UseSharedCompilation')
try {
 $env:UseSharedCompilation='cleanup-test-sentinel'
 & "$PSScriptRoot/Test-SupportPaths.ps1" -Image ('playbook-cleanup-absent:'+ [guid]::NewGuid().ToString('N')) | ForEach-Object {$output.Add([string]$_)}
} catch {$failure=$_}
$networks=@($output | Where-Object {$_ -cmatch '^[a-f0-9]{64}$'})
try {
 if($networks.Count -ne 2){throw 'Expected two real owned networks before the controlled missing-image failure.'}
 $remaining=@(& $docker network ls --no-trunc --format '{{.ID}}')
 $leaked=@($networks | Where-Object {$_ -cin $remaining})
 if($leaked.Count){throw "Startup failure leaked $($leaked.Count) owned networks; cleanup masked or skipped the remaining work."}
 if(!$failure -or $failure.InvocationInfo.Line -notmatch '\$docker run') {throw 'The original Docker startup failure was not preserved.'}
 if($env:UseSharedCompilation -cne 'cleanup-test-sentinel'){throw 'The child process environment was not restored.'}
 if('ENVIRONMENT unchanged; owned Docker resources removed' -cnotin $output){throw 'Environment restoration and verification were skipped.'}
 foreach($scope in $before.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "Unexpected $scope PATH change; shared settings preserved."}}
 Write-Output 'PASS: real missing-image startup failure preserved, both owned networks removed, environment verification reached.'
} finally {
 [Environment]::SetEnvironmentVariable('UseSharedCompilation',$previousCompilation)
 # Remove only this test's exact network IDs if the regression is present.
 $remaining=@(& $docker network ls --no-trunc --format '{{.ID}}')
 foreach($id in $networks){if($id -cin $remaining){& $docker network rm $id}}
 $global:LASTEXITCODE=0
}
