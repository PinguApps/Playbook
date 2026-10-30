# snippet:initialize
[CmdletBinding()]
param([Parameter(Mandatory)][string]$Notation,[Parameter(Mandatory)][string]$Configuration)
$ErrorActionPreference='Stop'
if(-not [IO.Path]::IsPathFullyQualified($Notation) -or -not [IO.Path]::IsPathFullyQualified($Configuration)){
  throw 'Use absolute paths for the executable and new isolated configuration directory.'
}
if(Test-Path -LiteralPath $Configuration){throw 'Choose a fresh configuration directory.'}
$null=New-Item -ItemType Directory -Path $Configuration
function Invoke-QuayNotation([string[]]$Arguments){
  $start=[Diagnostics.ProcessStartInfo]::new($Notation)
  foreach($argument in $Arguments){$start.ArgumentList.Add($argument)}
  $start.UseShellExecute=$false;$start.CreateNoWindow=$true
  $start.Environment['NOTATION_CONFIG']=$Configuration
  $start.Environment['NOTATION_CACHE']=Join-Path $Configuration 'cache'
  $start.Environment['NOTATION_LIBEXEC']=Join-Path $Configuration 'plugins'
  $process=[Diagnostics.Process]::Start($start)
  try{
    if(-not $process.WaitForExit(120000)){$process.Kill($true);throw 'Notation setup timed out.'}
    if($process.ExitCode -ne 0){throw 'Notation setup failed.'}
  }finally{$process.Dispose()}
}
$roots=@(
  @{Name='supply-chain.crt';Type='ca';Hash='23ffe2b8bdb9a1711515d4cffda04bc7f793d513c76c243f1020507d8669b7db';Url='https://www.microsoft.com/pkiops/certs/Microsoft%20Supply%20Chain%20RSA%20Root%20CA%202022.crt'},
  @{Name='timestamp.crt';Type='tsa';Hash='df545bf919a2439c36983b54cdfc903dfa4f37d3996d8d84b4c31eec6f3c163e';Url='https://www.microsoft.com/pki/certs/MicRooCerAut_2010-06-23.crt'}
)
foreach($root in $roots){
  $file=Join-Path $Configuration $root.Name
  Invoke-WebRequest $root.Url -OutFile $file
  if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -cne $root.Hash){throw 'Root certificate changed; review before trusting new bytes.'}
  Invoke-QuayNotation @('certificate','add','--type',$root.Type,'--store','quay-microsoft',$file)
}
$policy=@{
  version='1.0';trustPolicies=@(@{
    name='quay-runtime';registryScopes=@('mcr.microsoft.com/dotnet/runtime')
    signatureVerification=@{level='strict'};trustStores=@('ca:quay-microsoft','tsa:quay-microsoft')
    trustedIdentities=@('x509.subject: CN=Microsoft SCD Products RSA Signing,O=Microsoft Corporation,L=Redmond,S=Washington,C=US')
  })
}
$policyPath=Join-Path $Configuration 'input-policy.json'
[IO.File]::WriteAllText($policyPath,($policy|ConvertTo-Json -Depth 10))
Invoke-QuayNotation @('policy','import',$policyPath)
# endsnippet:initialize
