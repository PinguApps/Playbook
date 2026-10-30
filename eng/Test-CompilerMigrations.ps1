[CmdletBinding()]
param(
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$BaselineDotnet = (Join-Path $PSScriptRoot '../artifacts/compiler-tools/net10.0.100/dotnet.exe'),
  [switch]$SetupBaseline,
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$output = Join-Path $root ('artifacts/compiler-migrations/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$previousCertificate = $env:DOTNET_GENERATE_ASPNET_CERTIFICATE
$log = [Collections.Generic.List[string]]::new()
function Invoke-Checked([string]$Exe, [string[]]$Arguments, [int]$ExpectedExit = 0, [string]$Diagnostic) {
  $command = $Exe + ' ' + ($Arguments -join ' ')
  $PSNativeCommandUseErrorActionPreference = $false
  $text = (& $Exe @Arguments 2>&1 | Out-String).Trim()
  $exit = $LASTEXITCODE
  $log.Add("COMMAND: $command`nEXIT: $exit`n$text")
  if ($exit -ne $ExpectedExit -or ($Diagnostic -and $text -notmatch $Diagnostic)) {
    throw "Unexpected result: $command`n$text"
  }
  Write-Host $text
  $global:LASTEXITCODE = 0
  return $text
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $BaselineDotnet = [IO.Path]::GetFullPath($BaselineDotnet)
  if (-not (Test-Path -LiteralPath $BaselineDotnet)) {
    if (-not $SetupBaseline -or -not $IsWindows) { throw 'Supply the portable Windows x64 SDK10.0.100 with -BaselineDotnet, or use -SetupBaseline on Windows.' }
    # Portable extraction only: no installer, shell integration or persistent settings.
    $url = 'https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.100/dotnet-sdk-10.0.100-win-x64.zip'
    $expected = '24b033418a3969effd49b4651ef7ebbffeb284773b99545d78dce61a82e57f38db7facdb013c609ba15573c072f0e093363ae470824a6847f3c6111078c1fb64'
    $archive = Join-Path $output 'sdk10.0.100.zip'
    Invoke-WebRequest $url -OutFile $archive
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash -ine $expected) { throw 'SDK10.0.100 archive hash differs.' }
    Expand-Archive -LiteralPath $archive -DestinationPath (Split-Path $BaselineDotnet -Parent)
  }
  $sets = @(
    @{ Name = 'baseline'; Dotnet = $BaselineDotnet; Sdk = '10.0.100'; Ref = '10.0.0'; Target = 'net10.0'; Language = '14.0'; Runtime = '10.0.0' },
    @{ Name = 'previous'; Dotnet = $Dotnet; Sdk = '10.0.402'; Ref = '10.0.13'; Target = 'net10.0'; Language = '14.0'; Runtime = '10.0.13' },
    @{ Name = 'current'; Dotnet = $Net11Dotnet; Sdk = '11.0.100-rc.2.26504.105'; Ref = '11.0.0-rc.2.26504.105'; Target = 'net11.0'; Language = '15.0'; Runtime = '11.0.0-rc.2.26504.105' }
  )
  foreach ($set in $sets) {
    $set.Dotnet = [IO.Path]::GetFullPath($set.Dotnet)
    $install = Split-Path $set.Dotnet -Parent
    $compiler = Join-Path $install "sdk/$($set.Sdk)/Roslyn/bincore/csc.dll"
    $references = Join-Path $install "packs/Microsoft.NETCore.App.Ref/$($set.Ref)/ref/$($set.Target)"
    $version = Invoke-Checked $set.Dotnet @($compiler, '-version')
    $log.Add("ARTIFACT $($set.Name): compiler=$version; sdk=$($set.Sdk); refs=$($set.Ref); runtime=$($set.Runtime); language=$($set.Language); native/packages/workloads=none")
    foreach ($file in @($compiler, (Join-Path (Split-Path $compiler) 'Microsoft.CodeAnalysis.CSharp.dll'), (Join-Path $references 'System.Runtime.dll'),
        (Join-Path $install "shared/Microsoft.NETCore.App/$($set.Runtime)/System.Private.CoreLib.dll"))) {
      $log.Add("SHA256 $file $((Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant())")
    }
    $compilerArgs = @($compiler, '-nologo', '-nostdlib+', "-langversion:$($set.Language)")
    $refs = @(Get-ChildItem -LiteralPath $references -Filter '*.dll' | Sort-Object Name | ForEach-Object { '-r:' + $_.FullName })
    if (-not $refs.Count) { throw 'Missing declared reference pack.' }
    $span = Join-Path $root 'samples/net11.0/SpanStorage/SpanEscape.cs'
    $expectedExit = if ($set.Name -eq 'baseline') { 0 } else { 1 }
    $diagnostic = if ($expectedExit) { 'error CS8352' } else { $null }
    $null = Invoke-Checked $set.Dotnet ($compilerArgs + $refs + @('-target:library', "-out:$output/span-$($set.Name).dll", $span)) $expectedExit $diagnostic
    # Same artifact, ReadOnlySpan variant: changed context is not Span-only.
    $readonlySpan = Join-Path $output 'ReadOnlySpanEscape.cs'
    (Get-Content -LiteralPath $span -Raw).Replace('Span<int>', 'ReadOnlySpan<int>') | Set-Content -LiteralPath $readonlySpan
    $null = Invoke-Checked $set.Dotnet ($compilerArgs + $refs + @('-target:library', "-out:$output/readonly-span-$($set.Name).dll", $readonlySpan)) $expectedExit $diagnostic
    $dynamic = Join-Path $root 'samples/net11.0/DynamicConditions/DynamicConditions.cs'
    $binary = Join-Path $output "dynamic-$($set.Name).dll"
    $diagnostic = if ($expectedExit) { 'error CS7083' } else { $null }
    $result = Invoke-Checked $set.Dotnet ($compilerArgs + $refs + @('-target:exe', '-define:INTERFACE_LEFT', "-out:$binary", $dynamic)) $expectedExit $diagnostic
    if ($expectedExit -and @([regex]::Matches($result, 'error CS7083')).Count -ne 2) { throw 'Require separate && and || diagnostics.' }
    if (-not $expectedExit) {
      @{ runtimeOptions = @{ tfm = $set.Target; framework = @{ name = 'Microsoft.NETCore.App'; version = $set.Runtime }; rollForward = 'Disable' } } |
        ConvertTo-Json -Depth 10 | Set-Content -LiteralPath ([IO.Path]::ChangeExtension($binary, '.runtimeconfig.json'))
      $runtime = Invoke-Checked $set.Dotnet @($binary)
      if (@([regex]::Matches($runtime, 'EXPECTED legacy')).Count -ne 2) { throw 'Both old operators must fail in the real runtime binder.' }
    }
    # Determine the defining reference assembly, not a runtime type forwarding name.
    $stream = [IO.File]::OpenRead((Join-Path $references 'System.Runtime.dll'))
    $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
    try {
      $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      $found = @($metadata.TypeDefinitions | Where-Object {
        $type = $metadata.GetTypeDefinition($_)
        $metadata.GetString($type.Name) -ceq 'InAttribute' -and $metadata.GetString($type.Namespace) -ceq 'System.Runtime.InteropServices'
      })
      if ($found.Count -ne 1) { throw 'Expected InAttribute definition in the supported core reference assembly.' }
      $log.Add("PASS $($set.Name): System.Runtime.dll defines InAttribute; removing it also removes core type definitions, not a supported deficient reference route.")
    }
    finally { $pe.Dispose(); $stream.Dispose() }
  }
  foreach ($set in @($sets | Where-Object Name -ne 'baseline')) {
    foreach ($name in 'SpanStorage','DynamicConditions','ReadonlyMetadata') {
      $sample = "samples/$($set.Target)/$name/$name.csproj"
      Push-Location -LiteralPath (Join-Path $root "samples/$($set.Target)/$name")
      try {
        if ($Record) { & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $set.Dotnet }
        else { $null = Invoke-Checked $set.Dotnet @('build', "$name.csproj", '-c', 'Release', '-p:RestoreLockedMode=true') }
        $null = Invoke-Checked $set.Dotnet @('--fx-version', $set.Runtime, (Join-Path $PWD "bin/Release/$($set.Target)/$name.dll"))
      }
      finally { Pop-Location }
    }
  }
  $log.Add('EXCLUSION PIN-1033: no supported net10/net11 deficient reference route established. No fabricated corlib, InAttribute shim, obsolete target or placeholder recipe. Ordinary closures and actual readonly metadata/calls verified instead; missing-reference CS0518 repro/repair remains outside this bounded route.')
  $log | Set-Content -LiteralPath (Join-Path $output 'checks.txt')
  if ($Record) {
    foreach ($set in @($sets | Where-Object Name -ne 'baseline')) {
      foreach ($name in 'SpanStorage','DynamicConditions','ReadonlyMetadata') {
        $sample = "samples/$($set.Target)/$name/$name.csproj"
        $directory = Join-Path $root "samples/$($set.Target)/$name"
        $capture = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
        $capture.kind = 'execution'
        $capture.command = 'eng/Test-CompilerMigrations.ps1 -Record (exact executed commands follow in output)'
        $capture.scope = 'Actual pinned compiler/ref/runtime controls and corrected sample behavior; PIN-1033 is an evidenced supported-target exclusion, not a missing-reference repair.'
        $capture.output = $log -join "`n"
        $capture.inputs = Get-EvidenceInputs $root $sample
        [IO.File]::WriteAllText((Join-Path $directory 'migration-checks.evidence.json'), (ConvertTo-Json $capture -Depth 30).Replace("`r`n", "`n") + "`n")
      }
    }
  }
  Write-Host "PASS: compiler migration controls and corrected consumers. Disposable capture: $output"
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = $previousCertificate
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
