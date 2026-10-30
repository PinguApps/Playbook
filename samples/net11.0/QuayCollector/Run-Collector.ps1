# snippet:run
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Dotnet,
  [Parameter(Mandatory)][string]$Console,
  [Parameter(Mandatory)][string]$TestAssembly,
  [Parameter(Mandatory)][string]$CollectorDirectory,
  [Parameter(Mandatory)][string]$Settings,
  [Parameter(Mandatory)][string]$ResultsDirectory,
  [switch]$Collect
)
$ErrorActionPreference='Stop'
foreach($path in $Dotnet,$Console,$TestAssembly,$CollectorDirectory,$Settings,$ResultsDirectory){
  if(-not [IO.Path]::IsPathFullyQualified($path)){throw 'Supply absolute paths.'}
}
$start=[Diagnostics.ProcessStartInfo]::new($Dotnet)
$arguments=@($Console,$TestAssembly,('/TestAdapterPath:'+(Split-Path $TestAssembly)+';'+$CollectorDirectory),
  ('/ResultsDirectory:'+$ResultsDirectory),'/Logger:trx;LogFileName=quay.trx','/Logger:console;verbosity=detailed')
if($Collect){$arguments+=@(('/Settings:'+$Settings),'/Collect:QuayObservations')}
foreach($argument in $arguments){$start.ArgumentList.Add($argument)}
$start.UseShellExecute=$false;$start.CreateNoWindow=$true
$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
$start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH']='0'
$start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE']='false'
foreach($name in 'DOTNET_ROOT','DOTNET_ROOT_X64'){$start.Environment[$name]=Split-Path $Dotnet}
foreach($name in 'DOTNET_HOST_PATH','VSTEST_DOTNET_HOST_PATH'){$start.Environment[$name]=$Dotnet}
$process=[Diagnostics.Process]::Start($start)
try{
  $stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync()
  if(-not $process.WaitForExit(120000)){$process.Kill($true);throw 'Test run timed out.'}
  Write-Output ($stdout.GetAwaiter().GetResult()+$stderr.GetAwaiter().GetResult())
  exit $process.ExitCode
}finally{$process.Dispose()}
# endsnippet:run
