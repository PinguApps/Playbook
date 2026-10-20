[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/blazor-removed-apis.assets.json'),
  [string]$CompareWith
)

# A bounded authoring audit, independent of production content and sample evidence.
# Requires the two pinned SDKs. Only the WASM packages are downloaded (from NuGet).
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$versions = @(
  @{ Dotnet = $Net10Dotnet; Sdk = '10.0.402'; Version = '10.0.13'; Target = 'net10.0' },
  @{ Dotnet = $Net11Dotnet; Sdk = '11.0.100-rc.2.26504.105'; Version = '11.0.0-rc.2.26504.105'; Target = 'net11.0' }
)
$selectors = [ordered]@{
  'Microsoft.AspNetCore.Components.Routing.Router' = '^(get|set)_PreferExactMatches$'
  'Microsoft.AspNetCore.Components.ResourceAsset' = '^\.ctor$'
  'Microsoft.AspNetCore.Components.Forms.EditContextDataAnnotationsExtensions' = '^(Add|Enable)DataAnnotationsValidation$'
  'Microsoft.AspNetCore.Components.Forms.RemoteBrowserFileStreamOptions' = '.'
  'Microsoft.AspNetCore.Components.Forms.BrowserFileStreamOptions' = '.'
  'Microsoft.AspNetCore.Components.Forms.IBrowserFile' = '^OpenReadStream$'
  'Microsoft.AspNetCore.Components.Web.WebEventCallbackFactoryEventArgsExtensions' = '^Create$'
  'Microsoft.AspNetCore.Components.EventCallbackFactory' = '^Create$'
  'Microsoft.AspNetCore.Components.NavigationManager' = '^(NavigateTo|get_HistoryEntryState|add_LocationChanged|remove_LocationChanged)$'
  'Microsoft.AspNetCore.Components.NavigationOptions' = '^(get|set)_(HistoryEntryState|ForceLoad|ReplaceHistoryEntry)$'
  'Microsoft.AspNetCore.Components.RenderTree.WebRenderer' = '^(\.ctor|get_RendererId|set_RendererId|GetWebRendererId)$'
  'Microsoft.AspNetCore.Components.WebAssembly.Infrastructure.JSInteropMethods' = '^NotifyLocationChanged$'
  'Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager' = '.'
  'Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteAuthenticationService`3' = '^\.ctor$'
  'Microsoft.AspNetCore.Components.WebAssembly.Authentication.AccessTokenResult' = '^(\.ctor|get_RedirectUrl|get_InteractiveRequestUrl|get_InteractionOptions)$'
  'Microsoft.AspNetCore.Components.WebAssembly.Authentication.AccessTokenNotAvailableException' = '^(\.ctor|Redirect)$'
  'Microsoft.AspNetCore.Components.WebAssembly.Authentication.NavigationManagerExtensions' = '^NavigateTo(LogIn|Login|Logout)$'
}
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) {
  throw 'Comparison output must not overwrite the committed baseline.'
}
New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
$temporary = Join-Path ([IO.Path]::GetTempPath()) "playbook-blazor-audit-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temporary | Out-Null
$results = @()
try {
  foreach ($baseline in $versions) {
    $sdkLine = @(& $baseline.Dotnet --list-sdks) | Where-Object { $_.StartsWith("$($baseline.Sdk) [") }
    if (@($sdkLine).Count -ne 1) { throw "Required SDK not found: $($baseline.Sdk) via $($baseline.Dotnet)" }
    $sdkDirectory = Join-Path ([regex]::Match($sdkLine, '\[(.+)\]').Groups[1].Value) $baseline.Sdk
    $dotnetRoot = Split-Path (Split-Path $sdkDirectory -Parent) -Parent
    if (-not ('System.Reflection.MetadataLoadContext' -as [type])) {
      Add-Type -Path (Join-Path $sdkDirectory 'System.Reflection.MetadataLoadContext.dll')
    }
    $paths = @()
    foreach ($pack in @('Microsoft.NETCore.App.Ref', 'Microsoft.AspNetCore.App.Ref')) {
      $directory = Join-Path $dotnetRoot "packs/$pack/$($baseline.Version)/ref/$($baseline.Target)"
      $paths += @(Get-ChildItem -LiteralPath $directory -Filter '*.dll' | ForEach-Object FullName)
    }
    $packages = @()
    foreach ($id in @('microsoft.aspnetcore.components.webassembly', 'microsoft.aspnetcore.components.webassembly.authentication')) {
      $url = "https://api.nuget.org/v3-flatcontainer/$id/$($baseline.Version)/$id.$($baseline.Version).nupkg"
      $zip = Join-Path $temporary "$id.$($baseline.Version).nupkg"
      Invoke-WebRequest -Uri $url -OutFile $zip
      $directory = "$zip.contents"
      [IO.Compression.ZipFile]::ExtractToDirectory($zip, $directory)
      $paths += @(Get-ChildItem "$directory/lib/$($baseline.Target)" -Filter '*.dll' | ForEach-Object FullName)
      $packages += [ordered]@{ id = $id; version = $baseline.Version; url = $url; sha256 = (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
    $resolver = [System.Reflection.PathAssemblyResolver]::new([string[]]$paths)
    $context = [System.Reflection.MetadataLoadContext]::new($resolver, 'System.Runtime')
    try {
      $components = @($paths | Where-Object { [IO.Path]::GetFileName($_) -like 'Microsoft.AspNetCore.Components*.dll' } | Sort-Object { [IO.Path]::GetFileName($_) } -Culture 'en-US' -CaseSensitive)
      $assemblies = @($components | ForEach-Object { $context.LoadFromAssemblyPath($_) })
      $types = @()
      foreach ($name in $selectors.Keys) {
        $typeMatches = @($assemblies | ForEach-Object { $_.GetType($name, $false) } | Where-Object { $null -ne $_ })
        if ($typeMatches.Count -gt 1) { throw "Ambiguous type: $name" }
        $members = @()
        foreach ($type in $typeMatches) {
          $flags = [Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
          foreach ($member in @($type.GetConstructors($flags)) + @($type.GetMethods($flags))) {
            if (-not ($member.IsPublic -or $member.IsFamily -or $member.IsFamilyOrAssembly)) { continue }
            if ($member.Name -notmatch $selectors[$name]) { continue }
            $access = if ($member.IsPublic) { 'public' } elseif ($member.IsFamilyOrAssembly) { 'protected-internal' } else { 'protected' }
            $modifiers = @()
            if ($member.IsStatic) { $modifiers += 'static' }
            if ($member.IsVirtual) { $modifiers += 'virtual' }
            if ($member -is [Reflection.MethodInfo]) {
              $modifiers += @($member.ReturnParameter.GetRequiredCustomModifiers() | ForEach-Object FullName)
            }
            $defaults = @($member.GetParameters() | Where-Object IsOptional | ForEach-Object {
              $value = if ($null -eq $_.RawDefaultValue) { 'null' } else { [string]$_.RawDefaultValue }
              "$($_.Position)=$value"
            })
            $members += [ordered]@{ signature = "$member"; access = $access; modifiers = @($modifiers); optionalParameters = @($defaults) }
          }
        }
        $types += [ordered]@{
          type = $name
          assembly = if ($typeMatches.Count) { $typeMatches[0].Assembly.GetName().Name } else { $null }
          members = @($members | Sort-Object signature -Culture 'en-US' -CaseSensitive)
        }
      }
      $results += [ordered]@{
        sdk = $baseline.Sdk; version = $baseline.Version; target = $baseline.Target
        packages = $packages
        assemblies = @($components | ForEach-Object { [ordered]@{ file = [IO.Path]::GetFileName($_); sha256 = (Get-FileHash $_ -Algorithm SHA256).Hash.ToLowerInvariant() } })
        types = $types
      }
    }
    finally { $context.Dispose() }
  }
  $json = ConvertTo-Json -InputObject @($results) -Depth 15
  Set-Content -LiteralPath $output -Value $json -Encoding utf8
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 15
    if ($json -cne $expected) { throw "API/artifact baseline differs from $CompareWith; inspect $output before revising the audit." }
    Write-Host 'PASS: both pinned artifact hashes and all selected API signatures match the committed audit.'
  }
  Write-Host "Audit written to $output (metadata only; no sample compilation or behavior claim)."
}
finally {
  # This absolute path was created above beneath the OS temporary directory.
  if ((Split-Path $temporary -Parent) -ne [IO.Path]::GetTempPath().TrimEnd([IO.Path]::DirectorySeparatorChar)) { throw 'Unexpected cleanup path' }
  Remove-Item -LiteralPath $temporary -Recurse -Force
}
