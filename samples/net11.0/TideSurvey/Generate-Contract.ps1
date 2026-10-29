# snippet:complete
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$PackageRoot,
  [Parameter(Mandatory)][string]$Generator,
  [string]$OutputDirectory = (Join-Path $PSScriptRoot 'obj')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not [Environment]::Is64BitProcess -or -not $IsWindows) { throw 'This pinned generator is Windows x64 only.' }
$expected = '214ecb46f982f3c8f1ac07ded35ea01ef87ca4a53e018ea9832f9e67b999ceac'
if ((Get-Item -LiteralPath $Generator).Length -ne 10039808 -or (Get-FileHash -LiteralPath $Generator).Hash.ToLowerInvariant() -cne $expected) {
  throw 'Use the verified protoc-gen-openapiv2 v2.31.0 Windows x64 release artifact.'
}
$protocRoot = Join-Path $PackageRoot 'grpc.tools/2.64.0'
$imports = Join-Path $PackageRoot 'microsoft.aspnetcore.grpc.jsontranscoding/11.0.0-rc.2.26504.105/content/protos'
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $output | Out-Null
& (Join-Path $protocRoot 'tools/windows_x64/protoc.exe') `
  "-I=$PSScriptRoot" "-I=$protocRoot/build/native/include" "-I=$imports" `
  "--plugin=protoc-gen-openapiv2=$Generator" "--openapiv2_out=$output" `
  '--openapiv2_opt=Mtides.proto=example.invalid/tides' (Join-Path $PSScriptRoot 'tides.proto')
if ($LASTEXITCODE -ne 0) { throw 'Document generation failed.' }
$document = Get-Content (Join-Path $output 'tides.swagger.json') -Raw | ConvertFrom-Json -AsHashtable
$operation = $document.paths['/v1/tides/{station}'].get
$reference = $operation.responses['200'].schema.'$ref'.Split('/')[-1]
$schema = $document.definitions[$reference]
if ($document.swagger -cne '2.0' -or $document.paths.Count -ne 1 -or
    $operation.summary -notlike '*next surveyed tide*' -or
    $schema.properties.heightCm.type -cne 'integer' -or
    $schema.properties.heightCm.description -notlike '*local survey marker*') { throw 'Incomplete generated contract.' }
Write-Host 'PASS actual protoc/OpenAPIv2 operations, schemas and protobuf comments.'
# endsnippet:complete
