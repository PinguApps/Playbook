[CmdletBinding()]
param(
  [string]$Manifest = (Join-Path $PSScriptRoot '../docs/planning/audits/automatic-exclusions.sources.json'),
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/automatic-exclusions.assets.json'),
  [string]$CompareWith
)

# Download source/package bytes and read ECMA metadata; never execute inspected code.
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) {
  throw 'Comparison output must not overwrite the committed baseline.'
}
$inputs = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-exclusions-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
function Get-Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
try {
  foreach ($source in $inputs.sources) {
    $file = Join-Path $scratch 'source'
    Invoke-WebRequest -Uri $source.url -OutFile $file
    if ((Get-Hash $file) -ne $source.sha256) { throw "Source hash mismatch: $($source.id)" }
  }
  $sdk = Join-Path $Net10Root 'sdk/10.0.402'
  Add-Type -Path (Join-Path $sdk 'System.Reflection.MetadataLoadContext.dll')
  $framework = @(Get-ChildItem (Join-Path $Net10Root 'packs/Microsoft.NETCore.App.Ref/10.0.13/ref/net10.0') -Filter '*.dll' | ForEach-Object FullName)
  if (-not $framework.Count) { throw 'Install the declared .NET 10 reference pack.' }
  $assets = @(
    @{ identity = 'SDK 10.0.402'; asset = 'FSharp/FSharp.Core.dll'; file = (Join-Path $sdk 'FSharp/FSharp.Core.dll') },
    @{ identity = 'SDK 11.0.100-rc.2.26504.105'; asset = 'FSharp/FSharp.Core.dll'; file = (Join-Path $Net11Root 'sdk/11.0.100-rc.2.26504.105/FSharp/FSharp.Core.dll') }
  )
  foreach ($package in $inputs.packages) {
    $archive = Join-Path $scratch "$($package.id).$($package.version).nupkg"
    Invoke-WebRequest -Uri $package.url -OutFile $archive
    if ((Get-Hash $archive) -ne $package.sha256) { throw "Package hash mismatch: $($package.version)" }
    $folder = Join-Path $scratch $package.version
    Expand-Archive -LiteralPath $archive -DestinationPath $folder
    foreach ($asset in $package.assets) {
      $assets += @{ identity = "$($package.id) $($package.version)"; asset = $asset; file = (Join-Path $folder $asset) }
    }
  }
  $results = @()
  foreach ($asset in $assets) {
    $resolver = [Reflection.PathAssemblyResolver]::new([string[]](@($asset.file) + $framework))
    $context = [Reflection.MetadataLoadContext]::new($resolver, 'System.Runtime')
    try {
      $assembly = $context.LoadFromAssemblyPath($asset.file)
      $type = $assembly.GetType('Microsoft.FSharp.Core.OptimizeClosureIfNotInlinedAttribute', $false)
      $declarations = @()
      if ($type) {
        $declarations += "public=$($type.IsPublic); sealed=$($type.IsSealed); base=$($type.BaseType.FullName)"
        foreach ($constructor in $type.GetConstructors()) { $declarations += $constructor.ToString() }
        foreach ($attribute in $type.GetCustomAttributesData()) {
          $arguments = @($attribute.ConstructorArguments | ForEach-Object { "$($_.ArgumentType.FullName)=$($_.Value)" }) -join ', '
          $named = @($attribute.NamedArguments | ForEach-Object { "$($_.MemberName)=$($_.TypedValue.Value)" }) -join ', '
          $declarations += "$($attribute.AttributeType.FullName)($arguments) [$named]"
        }
        [string[]]$sortedDeclarations = $declarations
        [Array]::Sort($sortedDeclarations, [StringComparer]::Ordinal)
        $declarations = $sortedDeclarations
      }
      $target = @($assembly.GetCustomAttributesData() | Where-Object { $_.AttributeType.FullName -eq 'System.Runtime.Versioning.TargetFrameworkAttribute' })[0].ConstructorArguments[0].Value
      $results += [ordered]@{
        identity = $asset.identity; asset = $asset.asset; sha256 = Get-Hash $asset.file
        assemblyVersion = $assembly.GetName().Version.ToString(); targetFramework = $target
        attributePresent = $null -ne $type; declarations = $declarations
      }
      Write-Host "$($asset.identity) $($asset.asset): attributePresent=$($null -ne $type)"
    } finally { $context.Dispose() }
  }
  $json = ConvertTo-Json -InputObject $results -Depth 8
  New-Item -ItemType Directory -Force -Path (Split-Path $output -Parent) | Out-Null
  [IO.File]::WriteAllText($output, $json + "`n")
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 8
    if ($json -cne $expected) { throw 'F# callback attribute metadata differs from the committed baseline.' }
  }
  Write-Host "PASS: $($inputs.sources.Count) source hashes, $($inputs.packages.Count) package hashes and $($results.Count) metadata assets."
} finally {
  # Only this invocation's literal, freshly created temporary directory is removed.
  if ([IO.Path]::GetFullPath($scratch).StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) {
    Remove-Item -LiteralPath $scratch -Recurse -Force
  }
}
