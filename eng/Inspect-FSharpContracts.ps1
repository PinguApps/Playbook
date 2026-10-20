[CmdletBinding()]
param(
  [string]$Manifest = (Join-Path $PSScriptRoot '../docs/verification/PIN-681.sources.json'),
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$ReferencePackVersion = '10.0.13',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/fsharp-contracts.json'),
  [string]$CompareWith
)

# Inspect downloaded source and ECMA metadata only; never execute inspected code.
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) {
  throw 'Comparison output must not overwrite the committed baseline.'
}
$inputs = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-fsharp-audit-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $scratch | Out-Null
function Get-Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Get-Ordinal($Values) {
  [string[]]$items = @($Values)
  [Array]::Sort($items, [StringComparer]::Ordinal)
  $items
}
try {
  foreach ($source in $inputs.sources) {
    $file = Join-Path $scratch 'source'
    Invoke-WebRequest $source.url -OutFile $file
    if ((Get-Hash $file) -ne $source.sha256) { throw "Source hash mismatch: $($source.path)" }
  }
  $sdk = Join-Path $Net10Root 'sdk/10.0.402'
  Add-Type -Path (Join-Path $sdk 'System.Reflection.MetadataLoadContext.dll')
  $framework = @(Get-ChildItem (Join-Path $Net10Root "packs/Microsoft.NETCore.App.Ref/$ReferencePackVersion/ref/net10.0") -Filter '*.dll' | ForEach-Object FullName)
  if (-not $framework.Count) { throw 'Install the declared .NET 10 reference pack.' }
  $results = @()
  $sdkCore = [pscustomobject]@{ id = 'FSharp.Core'; version = 'SDK-11.0.100-rc.2.26504.105'; assets = @('FSharp.Core.dll'); local = $true }
  foreach ($package in @($inputs.packages) + @($sdkCore)) {
    if ($package.local) {
      $folder = Join-Path $Net11Root 'sdk/11.0.100-rc.2.26504.105/FSharp'
    } else {
      $archive = Join-Path $scratch "$($package.id).$($package.version).nupkg"
      Invoke-WebRequest $package.url -OutFile $archive
      if ((Get-Hash $archive) -ne $package.sha256) { throw "Package hash mismatch: $($package.id)" }
      $folder = Join-Path $scratch "$($package.id).$($package.version)"
      Expand-Archive -LiteralPath $archive -DestinationPath $folder
    }
    foreach ($asset in $package.assets) {
      $file = Join-Path $folder $asset
      $core = if ($package.id -eq 'FSharp.Core') { $file } else {
        Join-Path $scratch 'FSharp.Core.11.0.101-rc2.26504.105/lib/netstandard2.0/FSharp.Core.dll'
      }
      $resolver = [Reflection.PathAssemblyResolver]::new([string[]](@($file, $core) + $framework | Select-Object -Unique))
      $context = [Reflection.MetadataLoadContext]::new($resolver, 'System.Runtime')
      try {
        $assembly = $context.LoadFromAssemblyPath($file)
        $lines = @()
        $types = if ($package.id -eq 'FSharp.Core') {
          @('Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers', 'Microsoft.FSharp.Collections.Array2DModule', 'Microsoft.FSharp.Control.TaskBuilderBase')
        } else { @('FSharp.Compiler.CodeAnalysis.FSharpChecker') }
        foreach ($name in $types) {
          $type = $assembly.GetType($name, $true)
          foreach ($method in $type.GetMethods([Reflection.BindingFlags]'Public,Instance,Static,DeclaredOnly')) {
            $include = switch ($name) {
              'Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers' { $method.Name -like '__runtimeAsync*' }
              'Microsoft.FSharp.Collections.Array2DModule' { $method.Name -in @('Create','Initialize','Rebase','Map','MapIndexed','Copy','ZeroCreate','CreateBased','InitializeBased','ZeroCreateBased') }
              'Microsoft.FSharp.Control.TaskBuilderBase' { $method.Name -eq 'Using' }
              'FSharp.Compiler.CodeAnalysis.FSharpChecker' { $method.Name -in @('Create','ClearCaches') }
            }
            if (-not $include) { continue }
            $parameters = @($method.GetParameters() | ForEach-Object { "$($_.ParameterType.ToString()) $($_.Name)" }) -join ', '
            $constraints = @($method.GetGenericArguments() | ForEach-Object { "$($_.Name):$((@($_.GetGenericParameterConstraints() | ForEach-Object ToString)) -join '&')" }) -join ';'
            $attributes = @(Get-Ordinal @($method.GetCustomAttributesData() | Where-Object { $_.AttributeType.Name -eq 'RequiresDynamicCodeAttribute' } | ForEach-Object { "$($_.AttributeType.FullName):$($_.ConstructorArguments[0].Value)" })) -join ';'
            $lines += "${name}::$($method.Name)($parameters) -> $($method.ReturnType.ToString()) constraints=[$constraints] $attributes"
          }
        }
        $targetFramework = @($assembly.GetCustomAttributesData() | Where-Object { $_.AttributeType.FullName -eq 'System.Runtime.Versioning.TargetFrameworkAttribute' })[0].ConstructorArguments[0].Value
        $results += [ordered]@{ package = $package.id; version = $package.version; asset = $asset; sha256 = Get-Hash $file; assemblyVersion = $assembly.GetName().Version.ToString(); targetFramework = $targetFramework; declarations = @(Get-Ordinal $lines) }
        Write-Host "$($package.id) $($package.version) $asset : $($lines.Count) declarations"
      } finally { $context.Dispose() }
    }
  }
  $json = ConvertTo-Json -InputObject $results -Depth 8
  New-Item -ItemType Directory -Force (Split-Path $output -Parent) | Out-Null
  [IO.File]::WriteAllText($output, $json + "`n")
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 8
    if ($json -cne $expected) { throw 'F# asset metadata differs from the committed baseline.' }
  }
  Write-Host "PASS: $($inputs.sources.Count) source hashes, $($inputs.packages.Count) package hashes and $($results.Count) asset surfaces."
} finally {
  # Only the fresh directory created above is removed.
  Remove-Item -LiteralPath $scratch -Recurse -Force
}
