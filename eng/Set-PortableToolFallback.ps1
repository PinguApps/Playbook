[CmdletBinding()]
param([Parameter(Mandatory)][string]$SettingsPath)

$ErrorActionPreference = 'Stop'
[xml]$settings = Get-Content -LiteralPath $SettingsPath -Raw
$packages = $settings.DotNetCliTool.RuntimeIdentifierPackages
$portable = $packages.SelectSingleNode("RuntimeIdentifierPackage[@RuntimeIdentifier='any']")
if (-not $portable) { throw 'The SDK did not generate the portable tool reference.' }

# The .NET 11 RID graph otherwise prefers glibc linux-x64/linux-arm64 to any.
if (-not $packages.SelectSingleNode("RuntimeIdentifierPackage[@RuntimeIdentifier='linux-musl']")) {
  $alias = $portable.CloneNode($true)
  $alias.SetAttribute('RuntimeIdentifier', 'linux-musl')
  $null = $packages.AppendChild($alias)
  $settings.Save((Resolve-Path -LiteralPath $SettingsPath).Path)
}
